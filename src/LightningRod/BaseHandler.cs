using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using LightningRod.Randomizers;
using LightningRod.Randomizers.Solo;
using LightningRod.Randomizers.Versus;
using LightningRod.Utilities;

namespace LightningRod;

public class BaseHandler(IFileSystem baseFs)
{
    public void TriggerRandomizers(string saveFolder)
    {
        Console.WriteLine("blud");
        GameData.DataPath = $"{saveFolder}/romfs/";
        GameData.FileSystem = new LayeredFileSystem(baseFs, new LocalFileSystem(GameData.DataPath));
        GameData.Random = new LongRNG(Convert.ToInt64(Options.GetOption("randomizerSeed")));

        if (Directory.Exists(GameData.DataPath))
            Directory.Delete(GameData.DataPath, true);
        Directory.CreateDirectory(GameData.DataPath);

        string RegionLangData = System.Text.Encoding.UTF8.GetString(
            GameData.FileSystem.GetFile("/System/RegionLangMask.txt")
        );
        GameData.GameVersion = RegionLangData.Split("\n")[2][..3];

        Logger.Log($"Game Version: {GameData.GameVersion}");
        Logger.Log($"Randomizer Seed: {Options.GetOption("randomizerSeed")}");

        if (Options.GetOption("randomizeKits"))
        {
            WeaponKitRandomizer.Randomize();
        }

        SdodrRandomizer.Randomize();
        CoopRandomizer.Randomize();
        VSStageRandomizer.Randomize();
        ParameterRandomizer.Randomize();
        MiscRandomizer.Randomize();
        BigWorldRandomizer.Randomize();
        InkColorRandomizer.Randomize();
        SingletonRandomizer.Randomize();
        HeroParameterRandomizer.Randomize();

        Logger.MakeFile();
    }
}
