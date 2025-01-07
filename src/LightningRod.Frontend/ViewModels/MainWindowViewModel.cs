using System;
using Avalonia.Utilities;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LightningRod.Frontend.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
#pragma warning disable CA1822 // Mark members as static
    public string ToolVersion => "0.9";
    public string VersionString => $"(v{ToolVersion})";
#pragma warning restore CA1822 // Mark members as static

    [ObservableProperty]
    private bool unimplemented;

    [ObservableProperty]
    private string randomizerSeed = string.Empty;

    [ObservableProperty]
    private bool dataUnloaded;

    [ObservableProperty]
    private bool dataUpdateUnloaded;

    [ObservableProperty]
    private bool gameDataLoaded;

    [ObservableProperty]
    private bool useRomFSInstead;

    [ObservableProperty]
    private bool randomizeKits;

    [ObservableProperty]
    private bool heroSubSelection;

    [ObservableProperty]
    private bool coopSplatBomb;

    [ObservableProperty]
    private bool allSubWeapons;

    [ObservableProperty]
    private bool heroModeSuperLanding;

    [ObservableProperty]
    private bool useRainmaker;

    [ObservableProperty]
    private bool useIkuraShoot;

    [ObservableProperty]
    private bool useAllSpecials;

    [ObservableProperty]
    private bool include170To220p;

    [ObservableProperty]
    private bool noPFSIncrementation;

    [ObservableProperty]
    private bool matchPeriscopeKits;

    [ObservableProperty]
    private bool randomFogLevels;

    [ObservableProperty]
    private bool swapStageEnv;

    [ObservableProperty]
    private bool randomStageEnv;

    [ObservableProperty]
    private bool tweakStageLayouts;

    [ObservableProperty]
    private int tweakLevel;

    [ObservableProperty]
    private bool tweakStageLayoutPos;

    [ObservableProperty]
    private bool tweakStageLayoutRot;

    [ObservableProperty]
    private bool tweakStageLayoutSiz;

    [ObservableProperty]
    private bool mismatchedStages;

    [ObservableProperty]
    private bool randomizeParameters;

    [ObservableProperty]
    private int parameterSeverity;

    [ObservableProperty]
    private bool maxInkConsume;

    [ObservableProperty]
    private bool randomizeInkColors;

    [ObservableProperty]
    private bool randomizeInkColorLock;

    [ObservableProperty]
    private bool randomizeInkColorSdodr;
    
    [ObservableProperty]
    private bool randomizeInkColorMsn;

    [ObservableProperty]
    private int inkColorBias;

    [ObservableProperty]
    private bool randomizeDialogue;

    [ObservableProperty]
    private bool randomizeAllText;

    [ObservableProperty]
    private bool notRandomizeWeaponNames;

    [ObservableProperty]
    private bool notRandomizeLevelNames;

    public MainWindowViewModel()
    {
        InitializeProperties();
    }

    private void InitializeProperties()
    {
        Unimplemented = false;
        DataUnloaded = true;
        DataUpdateUnloaded = true;
        UseRomFSInstead = false;
        GameDataLoaded = false;
        RandomizeKits = true;

        Random rand = new();
        RandomizerSeed = Math.Floor(
                (rand.NextDouble() * (9999999999999999 - 1000000000000000)) + 1000000000000000
            )
            .ToString();

        HeroSubSelection = false;
        CoopSplatBomb = false;
        AllSubWeapons = false;
        HeroModeSuperLanding = false;
        UseRainmaker = false;
        UseIkuraShoot = false;
        UseAllSpecials = false;
        Include170To220p = true;
        NoPFSIncrementation = false;
        MatchPeriscopeKits = true;

        RandomFogLevels = false;
        RandomStageEnv = false;
        SwapStageEnv = false;
        TweakStageLayouts = true;
        TweakLevel = 3;
        TweakStageLayoutPos = true;
        TweakStageLayoutRot = false;
        TweakStageLayoutSiz = false;
        MismatchedStages = true;

        RandomizeParameters = false;
        ParameterSeverity = 2;
        MaxInkConsume = true;
        RandomizeInkColors = true;
        RandomizeInkColorLock = true;
        
    }
}
