using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
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
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Scripting.Hosting;
using ReactiveUI;

namespace LightningRod.Frontend.Views;

public partial class MainWindow : Window
{
    public static FilePickerFileType SwitchFiles { get; } = new("Nintendo Switch Packages") { Patterns = ["*.nsp", "*.xci"] };

    IFileSystem HoianBaseFiles;
    SharedRef<IFileSystem> HoianTempBase, HoianTempUpdate;

    BaseHandler thunderBackend;

    public MainWindow()
    {
        InitializeComponent();
    }

    public MainWindowViewModel Model => (MainWindowViewModel)DataContext;

    public async void HandleNSPButtonsAsync(object? sender, RoutedEventArgs e) {
        switch ((sender as Button).Name) {
            case "LoadBaseNSP":
                if (Model.UseRomFSInstead) await LoadRomFSAsync();
                else await LoadGameFileAsync(false);
                break;
            case "LoadUpdateNSP":
                await LoadGameFileAsync(true);
                break;
            default: throw new NotImplementedException();
        }
    }

    public static async Task<string> OpenPickerPathAndReturn(bool isRomFS) {
        var mainWindow = ((Avalonia.Application.Current.ApplicationLifetime as IClassicDesktopStyleApplicationLifetime)?.MainWindow) ?? throw new Exception();
        var storageProvider = mainWindow.StorageProvider;

        if (isRomFS) {
            var loadedFolderData = await storageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions { Title = "Open Directory", AllowMultiple = false });

            if (loadedFolderData.Count < 1) throw new Exception("Missing game folder data!");
            return loadedFolderData[0].Path.LocalPath;
        }   

        var loadedData = await storageProvider.OpenFilePickerAsync(
            new FilePickerOpenOptions { Title = "Open Game File", FileTypeFilter = [SwitchFiles], AllowMultiple = false });

        if (loadedData.Count < 1) throw new Exception("Missing game file data!");
        return loadedData[0].Path.LocalPath;
    }

    public async Task LoadRomFSAsync() {
        var romFSData = await OpenPickerPathAndReturn(true);

        if (romFSData != null)
            HoianBaseFiles = new LocalFileSystem(romFSData);

        this.FindControl<Button>("LoadBaseNSP").Content = "romFS Directory loaded!";
        Model.DataUnloaded = romFSData is null;
    }

    public async Task LoadGameFileAsync(bool isUpdateFile) {
        var gameFilePath = await OpenPickerPathAndReturn(false);

        if (gameFilePath != null) {
            using FileStream nspStream = File.OpenRead(gameFilePath);
            SharedRef<IStorage> LibHacFile = new(new FileStorage(
                new LocalFile(gameFilePath, OpenMode.Read)));

            switch (System.IO.Path.GetExtension(gameFilePath)) {
                case ".nsp":
                    if (isUpdateFile) {
                        new PartitionFileSystemCreator().Create(ref HoianTempBase, ref LibHacFile).ThrowIfFailure();
                        Model.DataUpdateUnloaded = false;
                        this.FindControl<Button>("LoadUpdateNSP").Content = "Update NSP file loaded!";
                    } else {
                        new PartitionFileSystemCreator().Create(ref HoianTempUpdate, ref LibHacFile).ThrowIfFailure();
                        Model.DataUnloaded = false;
                        this.FindControl<Button>("LoadBaseNSP").Content = "NSP file loaded!";
                    }
                    
                    break;  
                case ".xci":
                    KeySet keys = setupKeyset();
                    Xci xci = new(keys, LibHacFile.Get);

                    HoianTempBase = new SharedRef<IFileSystem>(xci.OpenPartition(XciPartitionType.Secure));
                    Model.DataUnloaded = false;
                    this.FindControl<Button>("LoadBaseNSP").Content = "XCI file loaded!";

                    break;
                default: throw new Exception("This file is not a .nsp or .xci file!");
            }
        }
    }

    private void SendDataToBackend(object? sender, RoutedEventArgs e) {
        if (!Model.UseRomFSInstead && !Model.DataUpdateUnloaded) { // if it sucks shit
            HoianBaseFiles = new LayeredFileSystem(HoianTempBase.Get, HoianTempUpdate.Get);
            var filesystem = SwitchFs.OpenNcaDirectory(setupKeyset(), HoianBaseFiles);  

            IFileSystem baseNcaData = filesystem.Titles[0x0100C2500FC20000].MainNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);
            IFileSystem updateNcaData = filesystem.Titles[filesystem.Applications[0x0100C2500FC20000].Patch.Id].MainNca.OpenFileSystem(NcaSectionType.Data, IntegrityCheckLevel.IgnoreOnInvalid);
        
            HoianBaseFiles = new LayeredFileSystem(baseNcaData, updateNcaData);
        }

        thunderBackend = new BaseHandler(HoianBaseFiles);
        Model.GameDataLoaded = true;
    }

    // So it'll attach to the button lol
    private void StartRandomizer(object? sender, RoutedEventArgs e) {
        _ = StartRandomizerAsync(sender, e);
    }    

    private async Task StartRandomizerAsync(object? sender, RoutedEventArgs e) {
        var gameFilePath = await OpenPickerPathAndReturn(true);
        
        Randomizers.WeaponKitRandomizer.WeaponKitConfig weaponKitConfig = new(
            Model.HeroSubSelection,
            Model.CoopSplatBomb,
            Model.AllSubWeapons,
            Model.HeroModeSuperLanding,
            Model.UseRainmaker,
            Model.UseIkuraShoot,
            Model.UseAllSpecials,
            Model.Include170To220p,
            Model.NoPFSIncrementation,
            Model.MatchPeriscopeKits,
            Model.RandomizeKits
        );

        thunderBackend.triggerRandomizers(Convert.ToInt64(Model.RandomizerSeed), weaponKitConfig, gameFilePath);
    }

    private void SwapRomFSandNSPInput(object sender, RoutedEventArgs e)
    {
        var checkBox = sender as CheckBox;
        var button = this.FindControl<Button>("LoadBaseNSP");
        var buttonUpdate = this.FindControl<Button>("LoadUpdateNSP");

        if (checkBox != null && button != null)
        {
            if (checkBox.IsChecked == true)
            {
                button.Content = "Load romFS Directory";
                buttonUpdate.Content = "Disabled"; // lol this is unused but idgaf enough to remove
                button.Height = 75;
            }
            else
            {
                button.Content = "Load Base .nsp/.xci File";
                buttonUpdate.Content = "Load Update .nsp File";
                button.Height = 35;
            }
        }
    }

    private KeySet setupKeyset() {
        string homePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string homeTitleKeyFile = System.IO.Path.Combine(homePath, ".switch", "title.keys");
        string prodKeyFile = System.IO.Path.Combine(homePath, ".switch", "prod.keys");

        return ExternalKeyReader.ReadKeyFile(prodKeyFile, homeTitleKeyFile);
    }
}