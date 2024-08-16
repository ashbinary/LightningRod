using System.Text;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LibHac.Util;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
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

    public static BymlArrayNode randomizeValues(this BymlArrayNode arrayNode, LongRNG rand) {
        foreach (BymlNode<float> bymlNode in arrayNode.Array) {
            bymlNode.Data = (rand.NextFloat() * 2) * bymlNode.Data;
            DebugPrint($"Returned array node with float unk and RNG unk");
        }
        return arrayNode;
    }

    // --------------------------

    public static BymlArrayNode ReadCompressedByml(this IFileSystem fs, string path) {
        DebugPrint($"Loading {path}...");
        var data = fs.ReadWholeFile(path);
        Byml byml = new(new MemoryStream(data.DecompressZSTDBytes()));
        return (BymlArrayNode) byml.Root;
    }

    public static Sarc ReadCompressedSarc(this IFileSystem fs, string path) {
        DebugPrint($"Loading {path}...");
        var data = fs.ReadWholeFile(path);
        return new Sarc(data.DecompressZSTDBytes());
    }

    public static byte[] GetFileInSarc(this Sarc sarc, string path) {
        var sceneIndex = sarc.GetNodeIndex(path);
        return sarc.OpenFile(sceneIndex).ToArray();
    }

    public static void DebugPrint(string info) {
        Console.WriteLine("[LightningRod] " + info); // it's cute
    }
}