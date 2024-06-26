using System;
using ReactiveUI;

namespace LightningRod.Frontend.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    #pragma warning disable CA1822 // Mark members as static
    public string ToolVersion => "1.0";
    public string VersionString => $"(v{ToolVersion})";
    #pragma warning restore CA1822 // Mark members as static

    
    
    private string randomizerSeed;
    public string RandomizerSeed { get => randomizerSeed; set => SetProperty(ref randomizerSeed, value); }
    
    private bool dataUnloaded;
    public bool DataUnloaded {get => dataUnloaded; set => SetProperty(ref dataUnloaded, value); }
    
    private bool dataUpdateUnloaded;
    public bool DataUpdateUnloaded {get => dataUpdateUnloaded; set => SetProperty(ref dataUpdateUnloaded, value); }
    
    private bool gameDataLoaded;
    public bool GameDataLoaded {get => gameDataLoaded; set => SetProperty(ref gameDataLoaded, value); }
    
    private bool useRomFSInstead;
    public bool UseRomFSInstead {get => useRomFSInstead; set => SetProperty(ref useRomFSInstead, value); }
    
    
    private bool randomizeKits;
    public bool RandomizeKits {get => randomizeKits; set => SetProperty(ref randomizeKits, value); }
    
    private bool heroSubSelection;
    public bool HeroSubSelection {get => heroSubSelection; set => SetProperty(ref heroSubSelection, value); }
    
    private bool coopSplatBomb;
    public bool CoopSplatBomb {get => coopSplatBomb; set => SetProperty(ref coopSplatBomb, value); }
    
    private bool allSubWeapons;
    public bool AllSubWeapons {get => allSubWeapons; set => SetProperty(ref allSubWeapons, value); }
    
    private bool heroModeSuperLanding;
    public bool HeroModeSuperLanding {get => heroModeSuperLanding; set => SetProperty(ref heroModeSuperLanding, value); }
    
    private bool useRainmaker;
    public bool UseRainmaker {get => useRainmaker; set => SetProperty(ref useRainmaker, value); }
    
    
    private bool useIkuraShoot;
    public bool UseIkuraShoot {get => useIkuraShoot; set => SetProperty(ref useIkuraShoot, value); }
       
    private bool useAllSpecials;
    public bool UseAllSpecials {get => useAllSpecials; set => SetProperty(ref useAllSpecials, value); }
    
    private bool include170To220p;
    public bool Include170To220p {get => include170To220p; set => SetProperty(ref include170To220p, value); }
    
    private bool noPFSIncrementation;
    public bool NoPFSIncrementation {get => noPFSIncrementation; set => SetProperty(ref noPFSIncrementation, value); }
    
    private bool matchPeriscopeKits;
    public bool MatchPeriscopeKits {get => matchPeriscopeKits; set => SetProperty(ref matchPeriscopeKits, value); }

    public MainWindowViewModel()
    {
        dataUnloaded = true;
        dataUpdateUnloaded = true;
        useRomFSInstead = false;
        gameDataLoaded = false;

        randomizeKits = true;

        Random rand = new(); // seed
        randomizerSeed = Math.Floor((rand.NextDouble() * (9999999999999999 - 1000000000000000)) + 1000000000000000).ToString();

        heroSubSelection = false; // Initial value
        coopSplatBomb = false;
        allSubWeapons = false;
        heroModeSuperLanding = false;
        useRainmaker = false;
        useIkuraShoot = false;
        useAllSpecials = false;
        include170To220p = true;
        noPFSIncrementation = false;
        matchPeriscopeKits = true;
    }
}
