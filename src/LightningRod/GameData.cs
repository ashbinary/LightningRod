using System.Text;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LightningRod.Randomizers;

namespace LightningRod;

public static class GameData
{
    public static LayeredFileSystem FileSystem { get; set; }
    public static string DataPath { get; set; }

    public static string GameVersion { get; set; }
    public static LongRNG Random { get; set; }

    public static void CommitToFileSystem(string filePath, byte[] newData)
    {
        using FileStream fileSaver = File.Create($"{DataPath}/{filePath}");
        fileSaver.Write(newData);
        FileSystem.Commit();
    }
}