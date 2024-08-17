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

    public static byte[] CompressZSTDBytes(this byte[] data) {
        using Compressor compressor = new(new CompressionOptions(19)); {
            return compressor.Wrap(data);
        }
    }

    public static BymlArrayNode randomizeValues(this BymlArrayNode arrayNode, LongRNG rand) {
        foreach (BymlNode<float> bymlNode in arrayNode.Array) {
            bymlNode.Data = (rand.NextFloat() * 2) * bymlNode.Data;
            DebugPrint($"Returned array node with float unk and RNG unk");
        }
        return arrayNode;
    }

    // Handler for the versus stage RNG. Held in a utility file cause I hate static classes.
    public static BymlArrayNode VSStageRandomizePositions(this BymlArrayNode node, (float startPoint, float changePoint) dataPoints, float[] randInfo) {
        float basePoint = dataPoints.startPoint - dataPoints.changePoint;
        float modPoint = dataPoints.changePoint * 2;

        bool isNewNode = false;

        if (node.Length < 1) isNewNode = true;

        // RNG is handled as (random * 0.02) + 0.99 (example) 
        // provides random of 0.99 - 1.01
        // (1, 0.01) in dataPoints does this
        for (int i = 0; i < randInfo.Length; i++)
        {
            if (isNewNode)
                node.AddNodeToArray(new BymlNode<float>(BymlNodeId.Float, (float)((randInfo[i] * modPoint) + basePoint)));
            else
                (node[i] as BymlNode<float>).Data = (float)((randInfo[i] * modPoint) + basePoint) * (node[i] as BymlNode<float>).Data;
        }

        DebugPrint($"{isNewNode} Node Log: ArrayNode with Value 1 {(node[0] as BymlNode<float>).Data} from RNG set {randInfo[0]} and Datapoints {basePoint} and {modPoint}");
        return node;
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