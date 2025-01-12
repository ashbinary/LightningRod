using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using LibHac.Arp.Impl;
using LibHac.Common;
using LibHac.Common.Keys;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSrv;
using LibHac.FsSrv.FsCreator;
using LibHac.FsSystem;
using LibHac.Tools.Fs;
using LibHac.Tools.FsSystem;
using LibHac.Tools.FsSystem.NcaUtils;
using LightningRod.Frontend.ViewModels;
using LightningRod.Randomizers;
using Microsoft.CodeAnalysis;
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
        HoianTempUpdate;

    BaseHandler thunderBackend;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
    }

    public MainWindowViewModel Model => (MainWindowViewModel)DataContext;

    public async void HandleNSPButtonsAsync(object? sender, RoutedEventArgs e)
    {
        switch ((sender as Button).Name)
        {
            case "LoadBaseNSP":
                if (Model.UseRomFSInstead)
                    await LoadRomFSAsync();
                else
                    await LoadGameFileAsync(false);
                break;
            case "LoadUpdateNSP":
                await LoadGameFileAsync(true);
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

    public async Task LoadRomFSAsync()
    {
        var romFSData = await OpenPickerPathAndReturn(true);

        if (romFSData != null)
        {
            HoianBaseFiles = new LocalFileSystem(romFSData);
            this.FindControl<Button>("LoadBaseNSP").Content = "romFS Directory loaded!";
        }

        Model.DataUnloaded = romFSData is null;
    }

    public async Task LoadGameFileAsync(bool isUpdateFile)
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
                    if (!isUpdateFile)
                    {
                        new PartitionFileSystemCreator()
                            .Create(ref HoianTempBase, ref LibHacFile)
                            .ThrowIfFailure();
                        Model.DataUnloaded = false;
                        this.FindControl<Button>("LoadBaseNSP").Content = "Base Loaded (NSP)";
                    }
                    else
                    {
                        new PartitionFileSystemCreator()
                            .Create(ref HoianTempUpdate, ref LibHacFile)
                            .ThrowIfFailure();
                        Model.DataUpdateUnloaded = false;
                        this.FindControl<Button>("LoadUpdateNSP").Content = "Update Loaded";
                    }

                    break;
                case ".xci":
                    KeySet keys = setupKeyset();
                    Xci xci = new(keys, LibHacFile.Get);

                    HoianTempBase = new SharedRef<IFileSystem>(
                        xci.OpenPartition(XciPartitionType.Secure)
                    );
                    Model.DataUnloaded = false;
                    this.FindControl<Button>("LoadBaseNSP").Content = "Base Loaded (XCI)";

                    break;
                default:
                    throw new Exception("This file is not a .nsp or .xci file!");
            }
        }
    }

    private void SendDataToBackend(object? sender, RoutedEventArgs e)
    {
        if (!Model.UseRomFSInstead)
        {
            HoianBaseFiles = Model.DataUpdateUnloaded
                ? HoianTempBase.Get
                : new LayeredFileSystem(HoianTempBase.Get, HoianTempUpdate.Get);

            var filesystem = SwitchFs.OpenNcaDirectory(setupKeyset(), HoianBaseFiles);

            // Get base NCA data
            Title baseNcaTitle = filesystem.Titles[0x0100C2500FC20000];
            SwitchFsNca baseNca = baseNcaTitle.MainNca;
            IFileSystem baseNcaData = baseNca.OpenFileSystem(
                NcaSectionType.Data,
                IntegrityCheckLevel.IgnoreOnInvalid
            );

            if (Model.DataUpdateUnloaded)
            {
                HoianBaseFiles = baseNcaData;
            }
            else
            {
                IFileSystem updateNcaData = filesystem
                    .Titles[filesystem.Applications[0x0100C2500FC20000].Patch.Id]
                    .MainNca.OpenFileSystem(
                        NcaSectionType.Data,
                        IntegrityCheckLevel.IgnoreOnInvalid
                    );

                HoianBaseFiles = new LayeredFileSystem(baseNcaData, updateNcaData);
            }
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

    private KeySet setupKeyset()
    {
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string homeTitleKeyFile = System.IO.Path.Combine(homePath, ".switch", "title.keys");
        string prodKeyFile = System.IO.Path.Combine(homePath, ".switch", "prod.keys");

        return ExternalKeyReader.ReadKeyFile(prodKeyFile, homeTitleKeyFile);
    }
}
