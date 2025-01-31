using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSrv.FsCreator;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LightningRod.Frontend.ViewModels;
using Newtonsoft.Json;

namespace LightningRod.Frontend.Views;

public partial class MainWindow : Window
{
    public static FilePickerFileType SwitchFiles { get; } =
        new("Nintendo Switch Packages") { Patterns = ["*.nsp", "*.xci"] };

    public static FilePickerFileType ConfigFiles { get; } =
        new("LightningRod Config Files") { Patterns = ["*.config", "*.json"] };

    IFileSystem HoianBaseFiles;

    SharedRef<IFileSystem> HoianTempBase,
        HoianTempUpdate,
        HoianTempDLC;

    IFileSystem HoianFSTempBase,
        HoianFSTempDLC;

    BaseHandler thunderBackend;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();

        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        // if (File.Exists(System.IO.Path.Combine(homePath, ".switch", "prod.keys"))) // Check for prod keys and hope to god they're not dumb enough to put title keys separately!
        //     Model.IsConsoleKeysLoaded = true;
    }

    public MainWindowViewModel Model => (MainWindowViewModel)DataContext;

    public async void HandleNSPButtonsAsync(object? sender, RoutedEventArgs e)
    {
        switch ((sender as Button).Name)
        {
            case "LoadBaseNSP":
                if (Model.UseRomFSInstead)
                    await LoadRomFSAsync(GameType.Base);
                else
                    await LoadGameFileAsync(GameType.Base);
                break;
            case "LoadUpdateNSP": // Update has no romfs equivalent, bundled with base
                await LoadGameFileAsync(GameType.Update);
                break;
            case "LoadDLCNSP":
                if (Model.UseRomFSInstead)
                    await LoadRomFSAsync(GameType.DLC);
                else
                    await LoadGameFileAsync(GameType.DLC);
                break;
            default:
                throw new NotImplementedException();
        }
    }

    public static async Task<string> OpenPickerPathAndReturn(
        bool isDirectory,
        FilePickerOpenOptions options = null
    )
    {
        var mainWindow =
            (
                (
                    Avalonia.Application.Current.ApplicationLifetime
                    as IClassicDesktopStyleApplicationLifetime
                )?.MainWindow
            ) ?? throw new Exception();
        var storageProvider = mainWindow.StorageProvider;

        if (isDirectory)
        {
            var loadedFolderData = await storageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions { Title = "Open Directory", AllowMultiple = false }
            );

            if (loadedFolderData.Count < 1)
                return null;
            return loadedFolderData[0].Path.LocalPath;
        }

        var loadedData = await storageProvider.OpenFilePickerAsync(options);

        if (loadedData.Count < 1)
            return null;
        return loadedData[0].Path.LocalPath;
    }

    public async Task LoadRomFSAsync(GameType gameType)
    {
        var romFSData = await OpenPickerPathAndReturn(true);

        if (romFSData != null)
        {
            switch (gameType)
            {
                case GameType.Base:
                    HoianFSTempBase = new LocalFileSystem(romFSData);
                    this.FindControl<Button>("LoadBaseNSP").Content = "romFS Directory loaded!";
                    Model.IsBaseLoaded = romFSData is not null;
                    break;
                case GameType.DLC:
                    HoianFSTempDLC = new LocalFileSystem(romFSData);
                    this.FindControl<Button>("LoadDLCNSP").Content = "DLC romFS Directory loaded!";
                    Model.IsDLCLoaded = romFSData is not null;
                    break;
            }
        }

        
    }

    public async Task LoadGameFileAsync(GameType gameType)
    {
        var gameFilePath = await OpenPickerPathAndReturn(
            false,
            new FilePickerOpenOptions
            {
                Title = "Open Game File",
                FileTypeFilter = [SwitchFiles],
                AllowMultiple = false,
            }
        );

        if (gameFilePath != null)
        {
            using FileStream nspStream = File.OpenRead(gameFilePath);
            SharedRef<IStorage> LibHacFile = new(
                new FileStorage(new LocalFile(gameFilePath, OpenMode.Read))
            );

            switch (System.IO.Path.GetExtension(gameFilePath))
            {
                case ".nsp":
                    switch (gameType)
                    {
                        case GameType.Base:
                            HandlePartitionData(ref HoianTempBase, ref LibHacFile);
                            Model.IsBaseLoaded = true;
                            this.FindControl<Button>("LoadBaseNSP").Content = "Base Loaded (NSP)";
                            break;
                        case GameType.Update:
                            HandlePartitionData(ref HoianTempUpdate, ref LibHacFile);
                            Model.IsUpdateLoaded = true;
                            this.FindControl<Button>("LoadUpdateNSP").Content = "Update Loaded";
                            break;
                        case GameType.DLC:
                            HandlePartitionData(ref HoianTempDLC, ref LibHacFile);
                            Model.IsDLCLoaded = true;
                            this.FindControl<Button>("LoadDLCNSP").Content = "DLC Loaded";
                            break;
                    }
                    break;
                case ".xci":
                    KeySet keys = SetupKeyset();
                    Xci xci = new(keys, LibHacFile.Get);

                    HoianTempBase = new SharedRef<IFileSystem>(
                        xci.OpenPartition(XciPartitionType.Secure)
                    );
                    Model.IsBaseLoaded = true;
                    this.FindControl<Button>("LoadBaseNSP").Content = "Base Loaded (XCI)";

                    break;
                default:
                    throw new Exception("This file is not a .nsp or .xci file!");
            }
        }
    }

    private void HandlePartitionData(
        ref SharedRef<IFileSystem> hoianData,
        ref SharedRef<IStorage> libHacFile
    )
    {
        new PartitionFileSystemCreator().Create(ref hoianData, ref libHacFile).ThrowIfFailure();
    }

    private void SendDataToBackend(object? sender, RoutedEventArgs e)
    {
        if (Model.UseRomFSInstead)
        {
            HoianBaseFiles = Model.IsDLCLoaded
                ? new LayeredFileSystem(HoianFSTempBase, HoianFSTempDLC)
                : HoianFSTempBase;
        }
        else
        {
            List<IFileSystem> addedFilesystems = [HoianTempBase.Get];
            if (Model.IsUpdateLoaded)
                addedFilesystems.Add(HoianTempUpdate.Get);
            if (Model.IsDLCLoaded)
                addedFilesystems.Add(HoianTempDLC.Get);
            HoianBaseFiles = new LayeredFileSystem(addedFilesystems);

            var filesystem = SwitchFs.OpenNcaDirectory(SetupKeyset(), HoianBaseFiles);
            addedFilesystems = [];

            IFileSystem baseNcaData = filesystem
                .Titles[0x0100C2500FC20000]
                .MainNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);
            addedFilesystems.Add(baseNcaData);

            if (Model.IsUpdateLoaded)
            {
                IFileSystem updateNcaData = filesystem
                    .Titles[filesystem.Applications[0x0100C2500FC20000].Patch.Id]
                    .MainNca.OpenFileSystem(
                        NcaSectionType.Data,
                        IntegrityCheckLevel.IgnoreOnInvalid
                    );

                addedFilesystems.Add(updateNcaData);
            }

            if (Model.IsDLCLoaded)
            {
                IFileSystem dlcNcaData = filesystem
                    .Titles[filesystem.Applications[0x0100C2500FC20000].AddOnContent[0].Id] // Side Order is AOC102, but Inkopolis Plaza/Add On Bonus will never be loaded
                    .MainNca.OpenFileSystem(
                        NcaSectionType.Data,
                        IntegrityCheckLevel.IgnoreOnInvalid
                    );

                addedFilesystems.Add(dlcNcaData);
            }

            // LibHac reads filesystems front-to-back, this prevents 1.0.0 from being read over a newer version due to RegionLangMask
            addedFilesystems.Reverse();

            HoianBaseFiles = new LayeredFileSystem(addedFilesystems);
        }

        thunderBackend = new BaseHandler(HoianBaseFiles);
        Model.GameDataLoaded = true;
    }

    // So it'll attach to the button lol
    private void StartRandomizer(object? sender, RoutedEventArgs e)
    {
        StartRandomizerAsync(sender, e);
    }

    private async void StartRandomizerAsync(object? sender, RoutedEventArgs e)
    {
        var gameFilePath = await OpenPickerPathAndReturn(true);

        var observableFields = typeof(MainWindowViewModel)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(field => Attribute.IsDefined(field, typeof(ObservablePropertyAttribute)));

        foreach (var field in observableFields)
        {
            Options.SetOption(field.Name, field.GetValue(Model));
        }

        thunderBackend.TriggerRandomizers(gameFilePath);
    }

    private void ExportOptions(object? sender, RoutedEventArgs e)
    {
        AsyncExportOptions();
    }

    private async void AsyncExportOptions()
    {
        var optionFolder = await OpenPickerPathAndReturn(true);
        var jsonData = JsonConvert.SerializeObject(Model);

        using (StreamWriter fileStream = new StreamWriter(optionFolder + "\\LightningRod.config"))
        {
            fileStream.Write(jsonData);
        }
    }

    private void ImportOptions(object? sender, RoutedEventArgs e)
    {
        AsyncImportOptions();
    }

    private async void AsyncImportOptions()
    {
        var optionPath = await OpenPickerPathAndReturn(
            false,
            new FilePickerOpenOptions
            {
                Title = "Open Config File",
                FileTypeFilter = [ConfigFiles],
                AllowMultiple = false,
            }
        );
        Model.ImportOptions(optionPath);
    }

    private void SwapRomFSandNSPInput(object sender, RoutedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        var button = this.FindControl<Button>("LoadBaseNSP");
        var buttonUpdate = this.FindControl<Button>("LoadDLCNSP");

        if (checkBox != null && button != null)
        {
            if (checkBox.IsChecked == true)
            {
                button.Content = "Load RomFS Dump";
                buttonUpdate.Content = "Load DLC RomFS Dump"; // lol this is unused but idgaf enough to remove // NOT ANYMORE!
                buttonUpdate.Width = 300;
            }
            else
            {
                button.Content = "Load Base (.nsp/.xci File)";
                buttonUpdate.Content = "Load DLC";
                buttonUpdate.Width = 147.5;
            }
        }
    }

    private KeySet SetupKeyset()
    {
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string homeTitleKeyFile = System.IO.Path.Combine(homePath, ".switch", "title.keys");
        string prodKeyFile = System.IO.Path.Combine(homePath, ".switch", "prod.keys");

        try
        {
            return ExternalKeyReader.ReadKeyFile(prodKeyFile, homeTitleKeyFile);
        }
        catch (Exception e)
        {
            throw new Exception("Failed to load keys! " + e.Message);
        }
    }

    public enum GameType
    {
        Base,
        Update,
        DLC,
    }
}
