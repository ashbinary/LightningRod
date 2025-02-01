using System;
using System.Collections.Generic;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;

namespace LightningRod.Frontend.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public static string ToolVersion => "0.9";
    public static string VersionString => $"(v{ToolVersion})";

    /* Generic Randomizer Options */
    [ObservableProperty, JsonIgnore] private bool unimplemented = false;
    [ObservableProperty, JsonIgnore] private string randomizerSeed = GenerateRandomizerSeed();
    [ObservableProperty, JsonIgnore] private bool isConsoleKeysLoaded = false;
    [ObservableProperty, JsonIgnore] private bool isBaseLoaded = false;
    [ObservableProperty, JsonIgnore] private bool isUpdateLoaded = false;
    [ObservableProperty, JsonIgnore] private bool isDLCLoaded = false;
    [ObservableProperty, JsonIgnore] private bool gameDataLoaded = false;
    [ObservableProperty, JsonIgnore] private bool useRomFSInstead = false;
    /* Weapon Kit Randomization */
    [ObservableProperty] private bool randomizeKits = true;
    [ObservableProperty] private bool heroSubSelection = false;
    [ObservableProperty] private bool coopSplatBomb = false;
    [ObservableProperty] private bool allSubWeapons = false;
    [ObservableProperty] private bool heroModeSuperLanding = false;
    [ObservableProperty] private bool useRainmaker = false;
    [ObservableProperty] private bool useAllSpecials = false;
    [ObservableProperty] private int minimumPFS = 180;
    [ObservableProperty] private int maximumPFS = 210;
    [ObservableProperty] private bool noPFSIncrementation = false;
    [ObservableProperty] private bool matchPeriscopeKits = true;
    /* Stage Randomization */
    [ObservableProperty] private bool tweakStageLayouts = false;
    [ObservableProperty] private int tweakLevel = 5;
    [ObservableProperty] private bool tweakStageLayoutPos = true;
    [ObservableProperty] private bool tweakStageLayoutRot = true;
    [ObservableProperty] private bool tweakStageLayoutSiz = true;
    [ObservableProperty] private bool swapStageEnv = false;
    [ObservableProperty] private bool mixDayNightEnv = true;
    [ObservableProperty] private bool randomStageEnv = false;
    [ObservableProperty] private bool randomFogLevels = true;
    [ObservableProperty] private bool randomLighting = false;
    [ObservableProperty] private double envIntensity = 5;
    [ObservableProperty] private bool mismatchedStages = true;
    /* Parameter + Ink Randomization */
    [ObservableProperty] private bool randomizeParameters = true;
    [ObservableProperty] private bool randomizeMsnParameters = true;
    [ObservableProperty, DLCOption] private bool randomizeSdodrParameters = false;
    [ObservableProperty] private bool randomizeLoanedParams = false;
    [ObservableProperty] private int parameterSeverity = 2;
    [ObservableProperty] private bool randomizeWeaponWeight = true; 
    [ObservableProperty] private bool randomizeVersusConstants = true;
    [ObservableProperty] private bool maxInkConsume = true;
    [ObservableProperty] private bool randomizeInkColors = true;
    [ObservableProperty] private bool randomizeInkColorLock = false;
    [ObservableProperty] private bool randomizeInkColorCoop = false;
    [ObservableProperty] private bool randomizeInkColorMsn = false;
    [ObservableProperty, DLCOption] private bool randomizeInkColorSdodr = false;
    [ObservableProperty] private int inkColorBias = 0;
    /* Misc Randomization */
    [ObservableProperty] private bool randomizeDialogue = true;
    [ObservableProperty] private bool randomizeAllText = false;
    [ObservableProperty] private bool notRandomizeWeaponNames = true;
    [ObservableProperty] private bool notRandomizeLevelNames = true;
    /* Hero Mode Randomization */
    [ObservableProperty] private bool randomizeHeroWeapons = true;
    [ObservableProperty] private bool ensureFirstHeroWeapon = true;
    [ObservableProperty] private bool randomizeOctarians = true; // Unused : )
    [ObservableProperty] private bool randomizeOctarianParams = true; // Unused : )
    [ObservableProperty] private bool randomizeFuzzballCost = false;
    [ObservableProperty] private bool randomizeHeroTree = true;
    [ObservableProperty] private bool randomizeDigUpPointEggs = false;
    [ObservableProperty] private int digUpPointEggCount = 25;
    [ObservableProperty] private bool randomizeLevelReward = false;
    [ObservableProperty] private bool randomizeLevelFee = false;
    [ObservableProperty] private bool randomizeKettles = true;
    [ObservableProperty] private bool randomizeKettlesCrater = true;
    [ObservableProperty] private bool randomizeStageSkybox = false;
    [ObservableProperty] private bool randomizeStageMusic = false;

    /* Salmon Run */
    [ObservableProperty] private bool randomizeLoanedSub = true;
    [ObservableProperty] private bool randomizeLoanedSpecials = false;
    [ObservableProperty] private bool allSpecialsLoanable = false;
    [ObservableProperty] private bool randomizeCoopLevels = true;
    [ObservableProperty] private bool tweakCoopStageLayouts = true;
    [ObservableProperty] private int coopTweakLevel = 2;
    [ObservableProperty] private bool randomizeSalmonidParams = true;
    [ObservableProperty] private bool randomizeKingParams = true;
    [ObservableProperty] private bool shuffleSalmonidActors = false;
    [ObservableProperty] private bool allowLesserKingSwap = false;

    /* Side Order */
    [ObservableProperty, DLCOption] private bool randomizeDefaultPalettes = true;
    [ObservableProperty, DLCOption] private bool randomizePaletteKits = true;
    [ObservableProperty, DLCOption] private bool randomizePaletteTones = true;
    [ObservableProperty, DLCOption] private bool swapColorChips = true;
    [ObservableProperty, DLCOption] private bool randomizeColorChipStats = true;
    [ObservableProperty, DLCOption] private bool colorChipStatsAlwaysUp = true;
    [ObservableProperty, DLCOption] private bool randomizeHackEffects = true;
    [ObservableProperty, DLCOption] private bool randomizeHackCost = true;
    [ObservableProperty, DLCOption] private bool unlockAllHacks = true;
    [ObservableProperty, DLCOption] private bool randomizeLockerJem = true;
    [ObservableProperty, DLCOption] private bool randomizeLockerOrder = true;
    [ObservableProperty, DLCOption] private bool randomizeEventChance = true;
    [ObservableProperty, DLCOption] private bool randomizeDangerChance = true;
    [ObservableProperty, DLCOption] private bool randomizeDangerCombos = true;
    [ObservableProperty, DLCOption] private bool randomizeAgent4Kits = true;
    [ObservableProperty, DLCOption] private bool tweakSdodrStageLayouts = true;
    [ObservableProperty, DLCOption] private int sdodrTweakLevel = 2;


    public MainWindowViewModel() { }

    public static string GenerateRandomizerSeed()
    {
        Random random = new();
        string randomNumber = string.Empty;

        for (int i = 0; i < 16; i++)
            randomNumber += random.Next(0, 9);

        return randomNumber;
    }

    public void ImportOptions(string filePath)
    {
        var jsonData = File.ReadAllText(filePath);
        var options = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonData);

        if (options == null) return;

        foreach (KeyValuePair<string, object> option in options)
        {
            var property = GetType().GetProperty(option.Key);
            if (property != null && property.CanWrite)
            {
                var value = Convert.ChangeType(option.Value, property.PropertyType);
                property.SetValue(this, value);
            }
        }
    }
}
