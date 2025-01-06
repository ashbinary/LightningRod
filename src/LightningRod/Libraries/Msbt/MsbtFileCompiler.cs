using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class for compiling MSBT files.
/// </summary>
public class MsbtFileCompiler : IFileCompiler<MsbtFile>
{
    #region private members
    //primes used for generating label groups; hard-capped at 101 for some reason
    private static readonly int[] LabelPrimes =
    [
        2,
        3,
        5,
        7,
        11,
        13,
        17,
        19,
        23,
        29,
        31,
        37,
        41,
        43,
        47,
        53,
        59,
        61,
        67,
        71,
        73,
        79,
        83,
        89,
        97,
        101,
    ];
    #endregion

    #region IFileCompiler interface
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public void Compile(MsbtFile file, Stream fileStream)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(file, nameof(file));
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));
#else
        if (file is null)
            throw new ArgumentNullException(nameof(file));
        if (fileStream is null)
            throw new ArgumentNullException(nameof(fileStream));
#endif

        using var writer = new FileWriter(fileStream, true);
        writer.IsBigEndian = file.BigEndian;

        //check message ids/labels
        if (file.HasNli1)
        {
            var ids = new HashSet<uint>();
            foreach (var message in file.Messages)
            {
                if (!ids.Add(message.Id))
                    throw new InvalidDataException(
                        $"MSBT message IDs must be unique. A message with the ID {message.Id} already exists."
                    );
            }
        }
        if (file.HasLbl1)
        {
            var labels = new HashSet<string>();
            foreach (var message in file.Messages)
            {
                if (!labels.Add(message.Label))
                    throw new InvalidDataException(
                        $"MSBT message labels must be unique. A message with the label {message.Label} already exists."
                    );
            }
        }

        WriteHeader(writer, file);
        if (file.HasNli1)
            WriteSection("NLI1", writer, file, WriteNli1);
        if (file.HasLbl1)
            WriteSection("LBL1", writer, file, WriteLbl1);
        if (file.HasAto1)
            WriteSection("ATO1", writer, file, WriteAto1);
        if (file.HasAtr1)
            WriteSection("ATR1", writer, file, WriteAtr1);
        if (file.HasTsy1)
            WriteSection("TSY1", writer, file, WriteTsy1);
        WriteSection("TXT2", writer, file, WriteTxt2);

        var size = (uint)writer.Position;
        writer.JumpTo(0x12);
        writer.Write(size);
    }
    #endregion

    #region private methods
    private static void WriteHeader(FileWriter writer, MsbtFile file)
    {
        writer.Write("MsgStdBn", Encoding.ASCII);
        writer.Write((ushort)0xFEFF);
        writer.Pad(2);

        if (Equals(file.Encoding, Encoding.UTF8))
            writer.Write((byte)0x00);
        else if (
            Equals(file.Encoding, Encoding.Unicode)
            || Equals(file.Encoding, Encoding.BigEndianUnicode)
        )
            writer.Write((byte)0x01);
        else if (Equals(file.Encoding, Encoding.UTF32))
            writer.Write((byte)0x02);
        else
            throw new InvalidDataException("Invalid text encoding format.");

        writer.Write((byte)file.Version);

        var sections = 1;
        if (file.HasNli1)
            ++sections;
        if (file.HasLbl1)
            ++sections;
        if (file.HasAtr1)
            ++sections;
        if (file.HasAto1)
            ++sections;
        if (file.HasTsy1)
            ++sections;
        writer.Write((ushort)sections);

        writer.Pad(16);
    }

    private static void WriteSection(
        string sectionName,
        FileWriter writer,
        MsbtFile file,
        Action<FileWriter, MsbtFile> sectionWriter
    )
    {
        writer.Write(sectionName, Encoding.ASCII);
        var sizePosition = writer.Position;
        writer.Pad(12);

        sectionWriter(writer, file);

        var endPos = writer.Position;
        writer.JumpTo(sizePosition);
        writer.Write((int)(endPos - sizePosition - 12));
        writer.JumpTo(endPos);
        writer.Align(16, 0xAB);
    }

    private static void WriteNli1(FileWriter writer, MsbtFile file)
    {
        writer.Write(file.Messages.Count);

        //indices are usually sorted
        var indices = new (uint, int)[file.Messages.Count];
        for (var i = 0; i < file.Messages.Count; ++i)
        {
            indices[i] = (file.Messages[i].Id, i);
        }
        Array.Sort(indices, (i1, i2) => i1.Item1.CompareTo(i2.Item1));

        foreach (var (messageIndex, tableIndex) in indices)
        {
            writer.Write(messageIndex);
            writer.Write(tableIndex);
        }
    }

    private static void WriteLbl1(FileWriter writer, MsbtFile file)
    {
        var groupCount = GetLabelGroups(file.Messages.Count);
        var table = new List<(string, int)>[groupCount];
        for (var i = 0; i < table.Length; ++i)
            table[i] = [];
        for (var i = 0; i < file.Messages.Count; ++i)
        {
            var message = file.Messages[i];
            var hash = GetLabelHash(message.Label, groupCount);
            table[hash].Add((message.Label, i));
        }

        var labelOffset = 4 + groupCount * 8;
        writer.Write(groupCount);
        foreach (var list in table)
        {
            writer.Write(list.Count);
            writer.Write(labelOffset);
            foreach (var label in list)
                labelOffset += 5 + label.Item1.Length;
        }

        foreach (var list in table)
        {
            foreach (var label in list)
            {
                writer.Write((byte)label.Item1.Length);
                writer.Write(label.Item1, Encoding.ASCII);
                writer.Write(label.Item2);
            }
        }
    }

    private static int GetLabelGroups(int labelCount)
    {
        foreach (var prime in LabelPrimes)
        {
            if (labelCount / 2 < prime)
                return prime;
        }
        return LabelPrimes[^1];
    }

    private static int GetLabelHash(string label, int groups)
    {
        var hash = 0;
        foreach (var c in label)
            hash = hash * 0x492 + c;
        return (int)((hash & 0xFFFFFFFF) % groups);
    }

    private static void WriteAtr1(FileWriter writer, MsbtFile file)
    {
        writer.Write(file.Messages.Count);
        var attributeSize = 0;
        var hasAttributeText = false;
        foreach (var message in file.Messages)
        {
            if (message.AttributeText is not null)
            {
                attributeSize = 4;
                hasAttributeText = true;
                break;
            }

            if (attributeSize < message.Attribute.Length)
                attributeSize = message.Attribute.Length;
        }
        writer.Write(attributeSize);

        if (hasAttributeText)
        {
            var offsetPosition = writer.Position;
            writer.Pad(file.Messages.Count * 4);
            for (var i = 0; i < file.Messages.Count; ++i)
            {
                var startPos = writer.Position;
                writer.JumpTo(offsetPosition + i * 4);
                writer.Write((int)(startPos - offsetPosition + 8));
                writer.JumpTo(startPos);

                if (string.IsNullOrEmpty(file.Messages[i].AttributeText))
                    writer.Write("\0", file.Encoding);
                else
                    writer.Write(file.Messages[i].AttributeText + '\0', file.Encoding);
            }
        }
        else
        {
            foreach (var message in file.Messages)
            {
                writer.Write(message.Attribute);
                writer.Pad(attributeSize - message.Attribute.Length);
            }
        }

        writer.Write(file.AdditionalAttributeData);
    }

    private static void WriteAto1(FileWriter writer, MsbtFile file) => writer.Write(file.Ato1Data);

    private static void WriteTsy1(FileWriter writer, MsbtFile file)
    {
        foreach (var message in file.Messages)
        {
            writer.Write(message.StyleIndex);
        }
    }

    private static void WriteTxt2(FileWriter writer, MsbtFile file)
    {
        writer.Write(file.Messages.Count);
        var offsetPosition = writer.Position;
        writer.Pad(file.Messages.Count * 4);

        var isSingleByteEncoding = file.Encoding.GetMinByteCount() == 1;

        for (var i = 0; i < file.Messages.Count; ++i)
        {
            var startPos = writer.Position;
            writer.JumpTo(offsetPosition + i * 4);
            writer.Write((int)(startPos - offsetPosition + 4));
            writer.JumpTo(startPos);

            var message = file.Messages[i];
            var text = message.Text;
            for (var j = 0; j < message.Tags.Count; ++j)
            {
                var split = text.Split("{{" + j + "}}");
                writer.Write(split[0], file.Encoding);

                var function = message.Tags[j];
                if (function.Group == 0x0F)
                {
                    if (isSingleByteEncoding)
                        writer.Write((byte)0x0F);
                    else
                        writer.Write((ushort)0x0F);
                    writer.Write(function.Type);
                    writer.Write(function.Args);
                }
                else
                {
                    if (isSingleByteEncoding)
                        writer.Write((byte)0x0E);
                    else
                        writer.Write((ushort)0x0E);
                    writer.Write(function.Group);
                    writer.Write(function.Type);
                    writer.Write((ushort)function.Args.Length);
                    writer.Write(function.Args);
                }

                text = split[1];
            }
            writer.WriteTerminated(text, file.Encoding);
        }
    }
    #endregion
}
