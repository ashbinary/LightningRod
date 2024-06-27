using System.Security.Cryptography;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LightningRod.Randomizers;

namespace LightningRod;

public class BaseHandler(IFileSystem fileSystem)
{
    private readonly IFileSystem fs = fileSystem;

    public void triggerRandomizers(long seed,
        WeaponKitRandomizer.WeaponKitConfig weaponKitConfig,
        VSStageRandomizer.VSStageConfig versusStageConfig,
        string saveFolder
    ) {
        string RegionLangData = GetFileAsciiData("/System/RegionLangMask.txt", fs);
        var version = RegionLangData.Split("\n")[2].Substring(0, 3);

        if (weaponKitConfig.randomizeKits) {
            WeaponKitRandomizer weaponKitRandom = new(weaponKitConfig, fs, saveFolder);
            weaponKitRandom.Randomize(seed, version);
        }

        VSStageRandomizer versusStageRandomizer = new(versusStageConfig, fs, saveFolder);
        versusStageRandomizer.Randomize(seed, version);
    }

    public static byte[] GetFileData(string filepath, IFileSystem fs) {
        UniqueRef<IFile> tempFile = new(); // mmph god im so full of memory
        fs.OpenFile(ref tempFile, filepath.ToU8Span(), OpenMode.Read);
        Console.WriteLine(filepath);

        tempFile.Get.GetSize(out long tempFileSize);
        FileReader fReader = new(tempFile.Get);

        return fReader.ReadBytes(0, (int)tempFileSize);     
    }

    public static string GetFileAsciiData(string filepath, IFileSystem fs) {
        UniqueRef<IFile> tempFile = new(); // mmph god im so full of memory
        fs.OpenFile(ref tempFile, filepath.ToU8Span(), OpenMode.Read);

        _ = tempFile.Get.GetSize(out long tempFileSize);
        FileReader fReader = new(tempFile.Get);

        return fReader.ReadAscii(0, (int)tempFileSize);     
    }
}
