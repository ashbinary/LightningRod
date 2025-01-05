using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using LightningRod.Randomizers;

namespace LightningRod;

public class BaseHandler(IFileSystem baseFs)
{
    public void TriggerRandomizers(
        long seed,
        WeaponKitRandomizer.WeaponKitConfig weaponKitConfig,
        VSStageRandomizer.VSStageConfig versusStageConfig,
        ParameterRandomizer.ParameterConfig parameterConfig,
        string saveFolder
    )
    {
        GameData.DataPath = $"{saveFolder}/romfs/";
        GameData.FileSystem = new LayeredFileSystem(baseFs, new LocalFileSystem(GameData.DataPath));
        GameData.Random = new LongRNG(seed);

        if (Directory.Exists(GameData.DataPath))
            Directory.Delete(GameData.DataPath, true);
        Directory.CreateDirectory(GameData.DataPath);

        string RegionLangData = System.Text.Encoding.UTF8.GetString(
            GameData.FileSystem.ReadWholeFile("/System/RegionLangMask.txt")
        );
        GameData.GameVersion = RegionLangData.Split("\n")[2][..3];

        RandomizerUtil.DebugPrint($"Using game version {GameData.GameVersion}");

        if (weaponKitConfig.randomizeKits)
        {
            WeaponKitRandomizer weaponKitRandom = new(weaponKitConfig);
            weaponKitRandom.Randomize();
        }

        VSStageRandomizer versusStageRandomizer = new(versusStageConfig);
        versusStageRandomizer.Randomize();

        ParameterRandomizer parameterRandomizer = new(parameterConfig);
        parameterRandomizer.Randomize();
    }
}
