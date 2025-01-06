using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries;
using LightningRod.Libraries.Sarc;
using ZstdNet;
using LightningRod.Libraries.Msbt;

namespace LightningRod.Utilities;

public static class FileUtils
{
    public static byte[] GetFile(this IFileSystem fs, string filepath)
    {
        UniqueRef<IFile> tempFile = new();
        fs.OpenFile(ref tempFile, filepath.ToU8Span(), OpenMode.Read);

        tempFile.Get.GetSize(out long tempFileSize);
        FileReader fileReader = new(tempFile.Get);

        return fileReader.ReadBytes(0, (int)tempFileSize);
    }

    // ----------------------------------------------------------------------------

    public static byte[] CompressZSTD(this byte[] data)
    {
        using Compressor compressor = new(new CompressionOptions(19));
        return compressor.Wrap(data);
    }

    public static byte[] DecompressZSTD(this byte[] data)
    {
        using Decompressor decompressor = new();
        return decompressor.Unwrap(data);
    }

    // ----------------------------------------------------------------------------

    public static BymlArrayNode ParseByml(this IFileSystem fs, string path)
    {
        Logger.Log($"Opening and reading BYML: {path}");
        var data = fs.GetFile(path);
        Byml byml = new(new MemoryStream(data.DecompressZSTD()));
        return (BymlArrayNode)byml.Root;
    }

    public static byte[] SaveByml(IBymlNode file)
    {
        using MemoryStream dataStream = new();
        new Byml(file).Save(dataStream);
        return dataStream.ToArray();
    }

    public static SarcFile ParseSarc(this IFileSystem fs, string path)
    {
        Logger.Log($"Opening and reading SARC: {path}");
        var data = fs.GetFile(path);

        SarcFileParser fileParser = new();
        return fileParser.Parse(new MemoryStream(data.DecompressZSTD()));
    }

    public static byte[] SaveSarc(SarcFile data)
    {
        var compiler = new SarcFileCompiler();
        return compiler.Compile(data);
    }

    public static MsbtFile ParseMsbt(this byte[] data)
    {
        MsbtFileParser fileParser = new();
        return fileParser.Parse(new MemoryStream(data));
    }

    public static byte[] SaveMsbt(MsbtFile data)
    {
        var compiler = new MsbtFileCompiler();
        return compiler.Compile(data);
    }

    // ----------------------------------------------------------------------------

    public static byte[] GetSarcFileData(this SarcFile sarc, string path)
    {
        foreach (SarcContent sarcData in sarc.Files)
            if (sarcData.Name == path)
                return sarcData.Data;

        return null; // Unable to find file in SARC.
    }

    public static int GetSarcFileIndex(this SarcFile sarc, string path)
    {
        for (int i = 0; i < sarc.Files.Count; i++)
        {
            Console.WriteLine($"Finding {sarc.Files[i].Name}");
            if (sarc.Files[i].Name == path)
                return i;
        }

        return -1; // Unable to find file in SARC.
    }
}
