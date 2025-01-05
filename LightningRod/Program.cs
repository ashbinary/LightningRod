using LibHac.FsSystem;

namespace LightningRod;

public class Program
{
    public static void Main(string[] args)
    {
        BaseHandler handler = new BaseHandler(
            new LocalFileSystem(@"E:\Dumped Games\Splatoon 3\9.1.0\Program\Data")
        );

        var defaultOptions = new Dictionary<string, dynamic> // This is for debugging shut up
        {
            { "unimplemented", false },
            { "randomizerSeed", "1234567890123456" }, 
            { "dataUnloaded", true },
            { "dataUpdateUnloaded", true },
            { "gameDataLoaded", false },
            { "useRomFSInstead", false },
            { "randomizeKits", true },
            { "heroSubSelection", false },
            { "coopSplatBomb", false },
            { "allSubWeapons", false },
            { "heroModeSuperLanding", false },
            { "useRainmaker", false },
            { "useIkuraShoot", false },
            { "useAllSpecials", false },
            { "include170To220p", true },
            { "noPFSIncrementation", false },
            { "matchPeriscopeKits", true },
            { "randomFogLevels", false },
            { "swapStageEnv", false },
            { "randomStageEnv", false },
            { "tweakStageLayouts", true },
            { "tweakLevel", 3 },
            { "tweakStageLayoutPos", true },
            { "tweakStageLayoutRot", false },
            { "tweakStageLayoutSiz", false },
            { "mismatchedStages", true },
            { "randomizeParameters", false },
            { "parameterSeverity", 2 },
            { "maxInkConsume", true },
            { "randomizeInkColors", true },
            { "randomizeInkColorLock", true }
        };


        foreach (KeyValuePair<string, dynamic> option in defaultOptions)
        {
            Options.SetOption(option.Key, option.Value);
        }

        handler.TriggerRandomizers(
            0123456789012345,
            @"C:\Users\Ash\Documents\TestData"
        );
    }
}