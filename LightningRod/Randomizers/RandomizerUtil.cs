using System.Text;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LibHac.Util;
using LightningRod.Libraries.Byml;
using ZstdNet;

namespace LightningRod.Randomizers;
public static class RandomizerUtil {

    public static byte[] ReadWholeFile(this IFileSystem fs, string filepath) {
        UniqueRef<IFile> tempFile = new();
        fs.OpenFile(ref tempFile, filepath.ToU8Span(), OpenMode.Read);

        tempFile.Get.GetSize(out long tempFileSize);
        FileReader fileReader = new(tempFile.Get);

        return fileReader.ReadBytes(0, (int)tempFileSize);  
    }

    public static byte[] DecompressZSTDBytes(this byte[] data) {
        using Decompressor decompressor = new(); {
            return decompressor.Unwrap(data);
        }
    }

    // --------------------------

    public static BymlArrayNode ReadCompressedByml(this IFileSystem fs, string path) {
        DebugPrint($"Loading {path}...");
        var data = fs.ReadWholeFile(path);
        Byml byml = new(new MemoryStream(data.DecompressZSTDBytes()));
        return (BymlArrayNode) byml.Root;
    }

    public static void DebugPrint(string info) {
        Console.WriteLine("[LightningRod] " + info); // it's cute
    }
}