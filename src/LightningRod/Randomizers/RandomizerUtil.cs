using System.Text;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LightningRod.Libraries.Byml;
using NintendoTools.FileFormats;
using NintendoTools.FileFormats.Sarc;
using ZstdNet;

namespace LightningRod.Randomizers;

public static class RandomizerUtil
{
    public static byte[] ReadWholeFile(this IFileSystem fs, string filepath)
    {
        UniqueRef<IFile> tempFile = new();
        fs.OpenFile(ref tempFile, filepath.ToU8Span(), OpenMode.Read);

        tempFile.Get.GetSize(out long tempFileSize);
        FileReader fileReader = new(tempFile.Get);

        return fileReader.ReadBytes(0, (int)tempFileSize);
    }

    public static byte[] DecompressZSTDBytes(this byte[] data)
    {
        using Decompressor decompressor = new();
        {
            return decompressor.Unwrap(data);
        }
    }

    public static byte[] CompressZSTDBytes(this byte[] data)
    {
        using Compressor compressor = new(new CompressionOptions(19));
        {
            return compressor.Wrap(data);
        }
    }

    public static byte[] CompileSarc(this SarcFile data)
    {
        var compiler = new SarcFileCompiler();
        return compiler.Compile(data);
    }

    public static void CreateFolder(string folderName)
    {
        Directory.CreateDirectory($"{GameData.DataPath}/{folderName}");
    }

    public static BymlArrayNode randomizeValues(this BymlArrayNode arrayNode, LongRNG rand)
    {
        foreach (BymlNode<float> bymlNode in arrayNode.Array)
        {
            bymlNode.Data = (rand.NextFloat() * 2) * bymlNode.Data;
        }
        return arrayNode;
    }

    // Handler for the versus stage RNG. Held in a utility file cause I hate static classes.
    public static BymlArrayNode VSStageRandomizePositions(
        this BymlArrayNode node,
        (float startPoint, float changePoint) dataPoints,
        float[] randInfo
    )
    {
        float basePoint = dataPoints.startPoint - dataPoints.changePoint;
        float modPoint = dataPoints.changePoint * 2;

        bool isNewNode = false;

        if (node.Length < 1)
            isNewNode = true;

        // RNG is handled as (random * 0.02) + 0.99 (example)
        // provides random of 0.99 - 1.01
        // (1, 0.01) in dataPoints does this
        for (int i = 0; i < randInfo.Length; i++)
        {
            if (isNewNode)
                node.AddNodeToArray(
                    new BymlNode<float>(
                        BymlNodeId.Float,
                        (float)((randInfo[i] * modPoint) + basePoint)
                    )
                );
            else
                (node[i] as BymlNode<float>).Data =
                    (float)((randInfo[i] * modPoint) + basePoint)
                    * (node[i] as BymlNode<float>).Data;
        }
        return node;
    }

    public static byte[] ToBytes(this IBymlNode file)
    {
        using MemoryStream dataStream = new();
        new Byml(file).Save(dataStream);
        return dataStream.ToArray();
    }

    public static byte[] ToBytes(this MemoryStream memoryStream)
    {
        byte[] bytes = new byte[memoryStream.Length];
        memoryStream.Read(bytes, 0, (int)memoryStream.Length);
        return bytes;
    }

    public static bool FileExists(IFileSystem fileSystem, string filePath)
    {
        LibHac.Fs.Path hacPath = new();
        hacPath.InitializeWithNormalization(Encoding.UTF8.GetBytes(filePath));

        Result result = fileSystem.GetEntryType(out DirectoryEntryType entryType, in hacPath);
        return result.IsSuccess() && entryType == DirectoryEntryType.File;
    }

    // --------------------------

    public static BymlArrayNode ReadCompressedByml(this IFileSystem fs, string path)
    {
        Logger.Log($"Opening and reading BYML: {path}");
        var data = fs.ReadWholeFile(path);
        Byml byml = new(new MemoryStream(data.DecompressZSTDBytes()));
        return (BymlArrayNode)byml.Root;
    }

    public static SarcFile ReadCompressedSarc(this IFileSystem fs, string path)
    {
        Logger.Log($"Opening and reading SARC: {path}");
        var data = fs.ReadWholeFile(path);

        SarcFileParser fileParser = new();
        return fileParser.Parse(new MemoryStream(data.DecompressZSTDBytes()));
    }

    public static byte[] GetFileInSarc(this SarcFile sarc, string path)
    {
        foreach (SarcContent sarcData in sarc.Files)
            if (sarcData.Name == path) return sarcData.Data;

        return null;
    }

    public static void SetFileInSarc(this SarcFile sarc, string path, byte[] data)
    {
        foreach (SarcContent sarcData in sarc.Files)
            if (sarcData.Name == path) sarcData.Data = data;
    }

    public static void DebugPrint(string info)
    {
        Logger.Log(info);
    }
}
