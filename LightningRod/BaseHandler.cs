using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.FsSystem;
using LibHac.Tools.FsSystem;
using LightningRod.Randomizers;

namespace LightningRod;

public class BaseHandler(IFileSystem fileSystem)
{
    private readonly IFileSystem nspFS = fileSystem;
    private LayeredFileSystem layeredFS;

    public void triggerRandomizers(
        long seed,
        WeaponKitRandomizer.WeaponKitConfig weaponKitConfig,
        VSStageRandomizer.VSStageConfig versusStageConfig,
        ParameterRandomizer.ParameterConfig parameterConfig,
        string saveFolder
    )
    {
        string fsPath = $"{saveFolder}/romfs/";

        if (Directory.Exists(fsPath))
            Directory.Delete(fsPath, true);
        Directory.CreateDirectory(fsPath);
        layeredFS = new LayeredFileSystem(nspFS, new LocalFileSystem(fsPath));

        string RegionLangData = System.Text.Encoding.UTF8.GetString(
            layeredFS.ReadWholeFile("/System/RegionLangMask.txt")
        );
        var version = RegionLangData.Split("\n")[2][..3];
        RandomizerUtil.DebugPrint($"Using game version {version}");

        if (weaponKitConfig.randomizeKits)
        {
            WeaponKitRandomizer weaponKitRandom = new(weaponKitConfig, layeredFS, saveFolder);
            weaponKitRandom.Randomize(seed, version);
        }

        VSStageRandomizer versusStageRandomizer = new(versusStageConfig, layeredFS, saveFolder);
        versusStageRandomizer.Randomize(seed, version);

        ParameterRandomizer parameterRandomizer = new(parameterConfig, layeredFS, saveFolder);
        parameterRandomizer.Randomize(seed, version);
    }
}
