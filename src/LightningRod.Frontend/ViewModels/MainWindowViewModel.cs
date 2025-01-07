using System;
using CommunityToolkit.Mvvm.ComponentModel;

namespace LightningRod.Frontend.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public static string ToolVersion => "0.9";
    public static string VersionString => $"(v{ToolVersion})";

    /* Generic Randomizer Options */
    [ObservableProperty] private bool unimplemented = false;
    [ObservableProperty] private string randomizerSeed = GenerateRandomizerSeed();
    [ObservableProperty] private bool dataUnloaded = true;
    [ObservableProperty] private bool dataUpdateUnloaded = true;
    [ObservableProperty] private bool gameDataLoaded = false;
    [ObservableProperty] private bool useRomFSInstead = false;
    /* Weapon Kit Randomization */
    [ObservableProperty] private bool randomizeKits = true;
    [ObservableProperty] private bool heroSubSelection = false;
    [ObservableProperty] private bool coopSplatBomb = false;
    [ObservableProperty] private bool allSubWeapons = false;
    [ObservableProperty] private bool heroModeSuperLanding = false;
    [ObservableProperty] private bool useRainmaker = false;
    [ObservableProperty] private bool useIkuraShoot = false;
    [ObservableProperty] private bool useAllSpecials = false;
    [ObservableProperty] private bool include170To220p = true;
    [ObservableProperty] private bool noPFSIncrementation = false;
    [ObservableProperty] private bool matchPeriscopeKits = true;
    /* Stage Randomization */
    [ObservableProperty] private bool randomFogLevels = false;
    [ObservableProperty] private bool swapStageEnv = false;
    [ObservableProperty] private bool randomStageEnv = false;
    [ObservableProperty] private bool tweakStageLayouts = false;
    [ObservableProperty] private int tweakLevel = 3;
    [ObservableProperty] private bool tweakStageLayoutPos = true;
    [ObservableProperty] private bool tweakStageLayoutRot = true;
    [ObservableProperty] private bool tweakStageLayoutSiz = true;
    [ObservableProperty] private bool mismatchedStages = true;
    /* Parameter + Ink Randomization */
    [ObservableProperty] private bool randomizeParameters = true;
    [ObservableProperty] private int parameterSeverity = 2;
    [ObservableProperty] private bool maxInkConsume = true;
    [ObservableProperty] private bool randomizeInkColors = true;
    [ObservableProperty] private bool randomizeInkColorLock = false;
    [ObservableProperty] private bool randomizeInkColorMsn = false;
    [ObservableProperty] private int inkColorBias = 0;
    /* Misc Randomization */
    [ObservableProperty] private bool randomizeDialogue = true;
    [ObservableProperty] private bool randomizeAllText = false;
    [ObservableProperty] private bool notRandomizeWeaponNames = true;
    [ObservableProperty] private bool notRandomizeLevelNames = true;
    /* Hero Mode Randomization */
    [ObservableProperty] private bool randomizeHeroWeapons = true;
    [ObservableProperty] private bool ensureFirstHeroWeapon = true;
    [ObservableProperty] private bool randomizeLevelReward = false;
    [ObservableProperty] private bool randomizeKettles = true;
    [ObservableProperty] private bool randomizeStageSkybox = true;
    [ObservableProperty] private bool randomizeStageMusic = true;
    

    public MainWindowViewModel() { }

    public static string GenerateRandomizerSeed()
    {
        Random random = new();
        string randomNumber = string.Empty;

        for (int i = 0; i < 16; i++)
            randomNumber += random.Next(0, 9);

        return randomNumber;
    }
}
