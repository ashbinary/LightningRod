using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using LightningRod.Randomizers;
namespace LightningRod;

public class BaseHandler(IFileSystem fileSystem)
{
    private readonly IFileSystem fs = fileSystem;
    private LayeredFileSystem layeredFS;

    public void triggerRandomizers(long seed,
        WeaponKitRandomizer.WeaponKitConfig weaponKitConfig,
        VSStageRandomizer.VSStageConfig versusStageConfig,
        string saveFolder
    ) {
        Directory.CreateDirectory($"{saveFolder}/romfs/");
        layeredFS = new LayeredFileSystem(fileSystem, new LocalFileSystem($"{saveFolder}/romfs/"));

        string RegionLangData = System.Text.Encoding.UTF8.GetString(layeredFS.ReadWholeFile("/System/RegionLangMask.txt"));
        var version = RegionLangData.Split("\n")[2][..3];
        RandomizerUtil.DebugPrint($"Using game version {version}");

        if (weaponKitConfig.randomizeKits) {
            WeaponKitRandomizer weaponKitRandom = new(weaponKitConfig, fs, saveFolder);
            weaponKitRandom.Randomize(seed, version);
        }

        VSStageRandomizer versusStageRandomizer = new(versusStageConfig, fs, saveFolder);
        versusStageRandomizer.Randomize(seed, version);
    }
}
