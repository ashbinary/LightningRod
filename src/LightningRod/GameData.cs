using LibHac.Tools.FsSystem;
using LightningRod.Utilities;

namespace LightningRod;

public static class GameData
{
    public static LayeredFileSystem FileSystem { get; set; }
    public static string DataPath { get; set; }

    public static string GameVersion { get; set; }
    public static LongRNG Random { get; set; }

    public static WeaponInfo weaponNames = new WeaponInfo();

    public static void CommitToFileSystem(string filePath, byte[] newData)
    {
        string? directoryPath = Path.GetDirectoryName($"{filePath}");
        if (!Directory.Exists($"{DataPath}/{directoryPath}"))
            MiscUtils.CreateFolder(directoryPath);
        using FileStream fileSaver = File.Create($"{DataPath}/{filePath}");
        fileSaver.Write(newData);
        FileSystem.Commit();
        FileSystem.Flush();
    }

    public static bool IsNewerVersion(int version)
    {
        Logger.Log($"Checking if {GameVersion} is newer than {version}...");
        return version <= int.Parse(GameVersion);
    }
}

public class WeaponInfo // Info is kept for side order and msn randomizers
{
    public List<string> WeaponInfoMain = [];
    public List<string> WeaponInfoSub = [];
    public List<string> WeaponInfoSpecial = [];
}
