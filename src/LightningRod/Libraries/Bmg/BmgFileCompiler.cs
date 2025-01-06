using System;
using System.IO;
using System.Text;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A class for compiling BMG files.
/// </summary>
public class BmgFileCompiler : IFileCompiler<BmgFile>
{
    #region constructor
    static BmgFileCompiler() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    #endregion

    #region IFileCompiler interface
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public void Compile(BmgFile file, Stream fileStream)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(file, nameof(file));
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));
        #else
        if (file is null) throw new ArgumentNullException(nameof(file));
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));
        #endif

        using var writer = new FileWriter(fileStream, true);
        writer.IsBigEndian = file.BigEndian;

        //sort messages by id or label
        if (file.HasMid1)
        {
            file.Messages.Sort((m1, m2) =>
            {
                var cmp = m1.Id.CompareTo(m2.Id);
                if (cmp == 0) throw new InvalidDataException($"BMG message IDs must be unique. A message with the ID {m1.Id} already exists.");
                return cmp;
            });
        }
        if (file.HasStr1)
        {
            file.Messages.Sort((m1, m2) =>
            {
                var cmp = string.CompareOrdinal(m1.Label, m2.Label);
                if (cmp == 0) throw new InvalidDataException($"BMG message labels must be unique. A message with the label {m1.Label} already exists.");
                return cmp;
            });
        }

        WriteHeader(writer, file);
        WriteInf1(writer, file, out var attributeSize);
        WriteDat1(writer, file, out var offsets);
        WriteInf1Data(writer, file, attributeSize, offsets);
        if (file.HasMid1) WriteMid1(writer, file);
        if (file.HasStr1) WriteStr1(writer, file);

        var size = (int) writer.Position; //file size does not count FLW1/FLI1

        if (file is {HasFlw1: true, FlowData: not null})
        {
            WriteFlw1(writer, file);
            WriteFli1(writer, file);
        }

        writer.JumpTo(0x08);
        writer.Write(size);
    }
    #endregion

    #region private methods
    private static void WriteHeader(FileWriter writer, BmgFile file)
    {
        writer.Write(file.BigEndianLabels ? "GSEM1gmb" : "MESGbmg1", Encoding.ASCII);
        writer.Pad(4);

        var sectionCount = 2;
        if (file.HasMid1) ++sectionCount;
        if (file.HasStr1) ++sectionCount;
        if (file.HasFlw1) sectionCount += 2;
        writer.Write(sectionCount);

        if (Equals(file.Encoding, Encoding.GetEncoding(1252))) writer.Write((byte) 0x01);
        else if (Equals(file.Encoding, Encoding.Unicode) || Equals(file.Encoding, Encoding.BigEndianUnicode)) writer.Write((byte) 0x02);
        else if (Equals(file.Encoding, Encoding.GetEncoding("Shift-JIS"))) writer.Write((byte) 0x03);
        else if (Equals(file.Encoding, Encoding.UTF8)) writer.Write((byte) 0x04);
        else throw new InvalidDataException("Invalid text encoding format.");

        writer.Align(16);
    }

    private static void WriteInf1(FileWriter writer, BmgFile file, out int attributeSize)
    {
        var startPos = writer.Position;
        writer.Write(file.BigEndianLabels ? "1FNI" : "INF1", Encoding.ASCII);
        var sizePosition = writer.Position;
        writer.Pad(4);

        attributeSize = 0;
        foreach (var message in file.Messages)
        {
            if (attributeSize < message.Attribute.Length) attributeSize = message.Attribute.Length;
        }

        writer.Write((ushort) file.Messages.Count);
        writer.Write((ushort) (attributeSize + 4));
        writer.Write((ushort) file.FileId);
        writer.Write((byte) file.DefaultColor);
        writer.Pad(1);

        writer.Pad(file.Messages.Count * (attributeSize + 4));

        writer.Align(32); //section size includes padding
        var endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((uint) (endPos - startPos));
        writer.JumpTo(endPos);
    }

    private static void WriteInf1Data(FileWriter writer, BmgFile file, int attributeSize, uint[] offsets)
    {
        var startPos = writer.Position;
        writer.JumpTo(0x30);

        for (var i = 0; i < offsets.Length; ++i)
        {
            var message = file.Messages[i];
            writer.Write(offsets[i]);
            writer.Write(message.Attribute);
            writer.Pad(attributeSize - message.Attribute.Length);
        }

        writer.JumpTo(startPos);
    }

    private static void WriteDat1(FileWriter writer, BmgFile file, out uint[] offsets)
    {
        offsets = new uint[file.Messages.Count];

        var startPos = writer.Position;
        writer.Write(file.BigEndianLabels ? "1TAD" : "DAT1", Encoding.ASCII);
        var sizePosition = writer.Position;
        writer.Pad(4);

        var offsetStart = writer.Position;
        var encodingSize = file.Encoding.GetMinByteCount();
        writer.Pad(encodingSize);

        for (var i = 0; i < file.Messages.Count; ++i)
        {
            offsets[i] = (uint) (writer.Position - offsetStart);

            var message = file.Messages[i];
            var text = message.Text;
            for (var j = 0; j < message.Tags.Count; ++j)
            {
                var split = text.Split("{{" + j + "}}");
                writer.Write(split[0], file.Encoding);

                var function = message.Tags[j];
                if (encodingSize == 1)
                {
                    writer.Write((byte) 0x1A);
                    writer.Write((byte) (function.Args.Length + 5));
                }
                else
                {
                    writer.Write((ushort) 0x1A);
                    writer.Write((byte) (function.Args.Length + 6));
                }

                writer.Write(function.Group);
                writer.Write(function.Type);
                writer.Write(function.Args);

                text = split[1];
            }
            writer.WriteTerminated(text, file.Encoding);
        }

        writer.Align(32); //section size includes padding
        var endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((uint) (endPos - startPos));
        writer.JumpTo(endPos);
    }

    private static void WriteMid1(FileWriter writer, BmgFile file)
    {
        var startPos = writer.Position;
        writer.Write(file.BigEndianLabels ? "1DIM" : "MID1", Encoding.ASCII);
        var sizePosition = writer.Position;
        writer.Pad(4);

        writer.Write((ushort) file.Messages.Count);
        writer.Write(file.Mid1Format);
        writer.Pad(4);

        foreach (var message in file.Messages)
        {
            writer.Write(message.Id);
        }

        writer.Align(32); //section size includes padding
        var endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((uint) (endPos - startPos));
        writer.JumpTo(endPos);
    }

    private static void WriteStr1(FileWriter writer, BmgFile file)
    {
        var startPos = writer.Position;
        writer.Write(file.BigEndianLabels ? "1RTS" : "STR1", Encoding.ASCII);
        var sizePosition = writer.Position;
        writer.Pad(4);

        writer.Write((ushort) file.Messages.Count);
        writer.Pad(1);

        foreach (var message in file.Messages)
        {
            writer.WriteTerminated(message.Label);
        }

        writer.Align(32); //section size includes padding
        var endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((uint) (endPos - startPos));
        writer.JumpTo(endPos);
    }

    private static void WriteFlw1(FileWriter writer, BmgFile file)
    {
        var startPos = writer.Position;
        writer.Write(file.BigEndianLabels ? "1WLF" : "FLW1", Encoding.ASCII);
        var sizePosition = writer.Position;
        writer.Pad(4);

        var nodeCount = file.FlowData!.Nodes.Length + BinaryUtils.GetOffset(file.FlowData.Nodes.Length, 2);
        var labelCount = file.FlowData.Labels.Length + BinaryUtils.GetOffset(file.FlowData.Labels.Length, 8);
        writer.Write((ushort) nodeCount);
        writer.Write((ushort) labelCount);
        writer.Pad(4);

        foreach (var nodeData in file.FlowData.Nodes)
        {
            writer.Write(nodeData);
        }
        writer.Align(16);

        foreach (var labelData in file.FlowData.Labels)
        {
            writer.Write(labelData, 0, 2);
        }
        writer.Align(16);
        foreach (var labelData in file.FlowData.Labels)
        {
            writer.Write(labelData[2]);
        }

        writer.Align(32); //section size includes padding
        var endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((uint) (endPos - startPos));
        writer.JumpTo(endPos);
    }

    private static void WriteFli1(FileWriter writer, BmgFile file)
    {
        var startPos = writer.Position;
        writer.Write(file.BigEndianLabels ? "1ILF" : "FLI1", Encoding.ASCII);
        var sizePosition = writer.Position;
        writer.Pad(4);

        writer.Write((ushort) file.FlowData!.Indices.Length);
        writer.Write((ushort) 8);
        writer.Pad(4);

        foreach (var indexData in file.FlowData.Indices)
        {
            if (indexData.Length > 8) writer.Write(indexData, 0, 8);
            else
            {
                writer.Write(indexData);
                writer.Align(8);
            }
        }

        //FLI1 section is not aligned but header accounts for that
        var endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((uint) (endPos - startPos + BinaryUtils.GetOffset(endPos, 32)));
        writer.JumpTo(endPos);
    }
    #endregion
}