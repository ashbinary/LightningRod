using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class for parsing MSBT files.
/// </summary>
public class MsbtFileParser : IFileParser<MsbtFile>
{
    #region public methods
    /// <inheritdoc cref="IFileParser.CanParse"/>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool CanParseStatic(Stream fileStream)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));
#else
        if (fileStream is null)
            throw new ArgumentNullException(nameof(fileStream));
#endif

        using var reader = new FileReader(fileStream, true);
        return CanParse(reader);
    }
    #endregion

    #region IFileParser interface
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public bool CanParse(Stream fileStream) => CanParseStatic(fileStream);

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public MsbtFile Parse(Stream fileStream)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));
#else
        if (fileStream is null)
            throw new ArgumentNullException(nameof(fileStream));
#endif

        using var reader = new FileReader(fileStream);
        if (!CanParse(reader))
            throw new InvalidDataException("File is not a MSBT file.");

        //parse file metadata and header
        GetMetaData(reader, out var sectionCount, out _, out var version, out var encoding);

        //parse messages
        var msbtFile = new MsbtFile
        {
            BigEndian = reader.IsBigEndian,
            Version = version,
            Encoding = encoding,
        };

        var ids = Array.Empty<uint>();
        var labels = Array.Empty<string>();
        var attributes = Array.Empty<byte[]>();
        var attributeTexts = Array.Empty<string>();
        var styles = Array.Empty<uint>();
        var content = Array.Empty<string>();
        var tags = Array.Empty<List<MsbtTag>>();

        long sectionOffset = 0x20;
        for (var i = 0; i < sectionCount; ++i)
        {
            reader.JumpTo(sectionOffset);
            reader.Align(16);

            var type = reader.ReadString(4, Encoding.ASCII);
            var sectionSize = reader.ReadUInt32();
            sectionOffset += 0x10 + (sectionSize + 0xF & ~0xF);

            switch (type)
            {
                case "NLI1":
                    ParseNli1(reader, out ids);
                    msbtFile.HasNli1 = true;
                    break;
                case "LBL1":
                    ParseLbl1(reader, out labels);
                    msbtFile.HasLbl1 = true;
                    break;
                case "ATR1":
                    ParseAtr1(
                        reader,
                        encoding,
                        out attributes,
                        out attributeTexts,
                        out var additionalAttributeData
                    );
                    msbtFile.HasAtr1 = true;
                    msbtFile.AdditionalAttributeData = additionalAttributeData;
                    break;
                case "ATO1":
                    ParseAto1(reader, out var atoData);
                    msbtFile.HasAto1 = true;
                    msbtFile.Ato1Data = atoData;
                    break;
                case "TSY1":
                    ParseTsy1(reader, out styles);
                    msbtFile.HasTsy1 = true;
                    break;
                case "TXT2":
                    ParseTxt2(reader, encoding, sectionSize, out content, out tags);
                    break;
                default:
                    throw new InvalidDataException($"Unknown section type: {type}");
            }
        }

        //compile messages
        for (var i = 0; i < content.Length; ++i)
        {
            var message = new MsbtMessage
            {
                Id = i < ids.Length ? ids[i] : 0,
                Label = i < labels.Length ? labels[i] : string.Empty,
                Attribute =
                    i < attributes.Length
                        ? attributes[i]
                        : new byte[attributes.Length > 0 ? attributes[0].Length : 0],
                AttributeText =
                    i < attributeTexts.Length ? attributeTexts[i]
                    : attributeTexts.Length > 0 ? string.Empty
                    : null,
                StyleIndex = i < styles.Length ? styles[i] : 0,
                Text = content[i],
                Tags = tags[i],
            };

            msbtFile.Messages.Add(message);
        }

        return msbtFile;
    }
    #endregion

    #region private methods
    //verifies that the file is a MSBT file
    private static bool CanParse(FileReader reader) =>
        reader.BaseStream.Length > 8 && reader.ReadStringAt(0, 8, Encoding.ASCII) == "MsgStdBn";

    //parses meta data
    private static void GetMetaData(
        FileReader reader,
        out int sectionCount,
        out uint fileSize,
        out int version,
        out Encoding encoding
    )
    {
        var byteOrder = reader.ReadByteAt(8);
        if (byteOrder == 0xFE)
            reader.IsBigEndian = true;

        encoding = reader.ReadByteAt(12) switch
        {
            0 => Encoding.UTF8,
            1 => reader.IsBigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode,
            2 => Encoding.UTF32,
            _ => reader.IsBigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode,
        };

        version = reader.ReadByte();
        sectionCount = reader.ReadUInt16();
        fileSize = reader.ReadUInt32At(18);
    }

    //parse NLI1 type sections (message IDs)
    private static void ParseNli1(FileReader reader, out uint[] ids)
    {
        reader.Skip(8);
        var entryCount = reader.ReadUInt32();

        ids = new uint[entryCount];
        for (var i = 0; i < entryCount; ++i)
        {
            var id = reader.ReadUInt32();
            var tableIndex = reader.ReadUInt32();
            ids[tableIndex] = id;
        }
    }

    //parse LBL1 type sections (message labels)
    private static void ParseLbl1(FileReader reader, out string[] labels)
    {
        reader.Skip(8);
        var position = reader.Position;
        var entryCount = reader.ReadUInt32();

        var labelValues = new List<string>();
        var indices = new List<uint>();

        for (var i = 0; i < entryCount; ++i)
        {
            //group header
            reader.JumpTo(position + 4 + i * 8);
            var labelCount = reader.ReadUInt32();
            var offset = reader.ReadUInt32();

            //labels
            reader.JumpTo(position + offset);
            for (var j = 0; j < labelCount; ++j)
            {
                var length = reader.ReadByte();
                labelValues.Add(reader.ReadString(length));
                indices.Add(reader.ReadUInt32());
            }
        }

        labels = new string[indices.Count];
        for (var i = 0; i < indices.Count; ++i)
            labels[indices[i]] = labelValues[i];
    }

    //parse ATR1 type sections (message attributes)
    private static void ParseAtr1(
        FileReader reader,
        Encoding encoding,
        out byte[][] attributes,
        out string[] attributeTexts,
        out byte[] additionalData
    )
    {
        reader.Skip(-4);
        var sectionSize = reader.ReadUInt32();
        reader.Skip(8);
        var startPos = reader.Position;
        var entryCount = reader.ReadUInt32();
        var attributeSize = reader.ReadUInt32();

        var attributeLength = entryCount * attributeSize + 8;
        var hasText =
            attributeSize == 4
            && sectionSize >= attributeLength + entryCount * encoding.GetMinByteCount();

        attributes = new byte[entryCount][];
        attributeTexts = new string[hasText ? entryCount : 0];

        for (var i = 0; i < entryCount; ++i)
        {
            attributes[i] = reader.ReadBytes((int)attributeSize);

            if (hasText)
            {
                reader.Skip(-4);
                var offset = reader.ReadUInt32();
                if (offset > sectionSize || offset < attributeLength)
                {
                    hasText = false;
                    continue;
                }

                var pos = reader.Position;
                attributeTexts[i] = reader.ReadTerminatedStringAt(startPos + offset, encoding);
                reader.JumpTo(pos);
            }
        }

        if (!hasText)
            attributeTexts = [];

        var sectionDiff = startPos + sectionSize - reader.Position;
        additionalData = !hasText && sectionDiff > 0 ? reader.ReadBytes((int)sectionDiff) : [];
    }

    //parse ATO1 type sections (additional data)
    private static void ParseAto1(FileReader reader, out byte[] data)
    {
        reader.Skip(-4);
        var sectionSize = reader.ReadUInt32();
        reader.Skip(8);

        data = reader.ReadBytes((int)sectionSize);
    }

    //parse TSY1 type sections (text style)
    private static void ParseTsy1(FileReader reader, out uint[] styleIndices)
    {
        reader.Skip(-4);
        var sectionSize = reader.ReadUInt32();
        reader.Skip(8);

        styleIndices = new uint[sectionSize / 4];
        for (var i = 0; i < styleIndices.Length; ++i)
        {
            styleIndices[i] = reader.ReadUInt32();
        }
    }

    //parse TXT2 type sections (message content)
    private static void ParseTxt2(
        FileReader reader,
        Encoding encoding,
        long sectionSize,
        out string[] content,
        out List<MsbtTag>[] tags
    )
    {
        reader.Skip(8);
        var position = reader.Position;
        var entryCount = reader.ReadUInt32();

        var encodingWidth = encoding.GetMinByteCount();
        TagCheck isFunctionTag =
            encodingWidth == 1 ? IsFunctionTagSingle
            : reader.IsBigEndian ? IsTagDoubleByteBE
            : IsTagDoubleByteLE;
        TagCheck isEndTag =
            encodingWidth == 1 ? IsEndTagSingleByte
            : reader.IsBigEndian ? IsEndTagDoubleByteBE
            : IsEndTagDoubleByteLE;

        var offsets = ReadArray(reader, (int)entryCount);
        content = new string[entryCount];
        tags = new List<MsbtTag>[entryCount];

        for (var i = 0; i < entryCount; ++i)
        {
            //Get the start and end position
            var startPos = offsets[i] + position;
            var endPos = i + 1 < entryCount ? position + offsets[i + 1] : position + sectionSize;

            //parse message text
            reader.JumpTo(startPos);
            var buffer = reader.ReadBytes((int)(endPos - startPos));

            //check bytes for function calls
            var message = new StringBuilder();
            var messageTags = new List<MsbtTag>();
            var textIndex = 0;
            for (var j = 0; j < buffer.Length; j += encodingWidth)
            {
                if (isFunctionTag(buffer, j))
                {
                    //append text so far
                    if (j > textIndex)
                        message.Append(encoding.GetString(buffer, textIndex, j - textIndex));
                    message.Append("{{").Append(messageTags.Count).Append("}}");

                    //add function content
                    var tagDataOffset = j + encodingWidth;
                    var argLength =
                        tagDataOffset + 6 < buffer.Length
                            ? ReadTagValue(buffer, tagDataOffset + 4, reader.IsBigEndian)
                            : 0;
                    messageTags.Add(
                        new MsbtTag
                        {
                            Group = ReadTagValue(buffer, tagDataOffset, reader.IsBigEndian),
                            Type = ReadTagValue(buffer, tagDataOffset + 2, reader.IsBigEndian),
                            Args = ReadArgArray(buffer, tagDataOffset + 6, argLength),
                        }
                    );

                    j += 6 + argLength;
                    textIndex = j + encodingWidth;
                }
                else if (isEndTag(buffer, j))
                {
                    //append text so far
                    if (j > textIndex)
                        message.Append(encoding.GetString(buffer, textIndex, j - textIndex));
                    message.Append("{{").Append(messageTags.Count).Append("}}");

                    //add function content
                    var tagDataOffset = j + encodingWidth;
                    messageTags.Add(
                        new MsbtTag
                        {
                            Group = 0x0F,
                            Type = ReadTagValue(buffer, tagDataOffset, reader.IsBigEndian),
                            Args = ReadArgArray(buffer, tagDataOffset + 2, 2), //always 2 bytes long?
                        }
                    );

                    j += 4;
                    textIndex = j + encodingWidth;
                }
            }

            //append remaining text
            if (textIndex < buffer.Length)
                message.Append(encoding.GetString(buffer, textIndex, buffer.Length - textIndex));

            content[i] = message.ToString().TrimEnd('\0');
            tags[i] = messageTags;
        }
    }

    //read a uint array
    private static uint[] ReadArray(FileReader reader, int count)
    {
        var result = new uint[count];
        for (var i = 0; i < result.Length; ++i)
            result[i] = reader.ReadUInt32();
        return result;
    }

    //check for tags
    private delegate bool TagCheck(byte[] buffer, int index);

    private static bool IsFunctionTagSingle(byte[] buffer, int index) => buffer[index] == 0x0E;

    private static bool IsTagDoubleByteLE(byte[] buffer, int index) =>
        buffer[index] == 0x0E && buffer[index + 1] == 0x00;

    private static bool IsTagDoubleByteBE(byte[] buffer, int index) =>
        buffer[index] == 0x00 && buffer[index + 1] == 0x0E;

    private static bool IsEndTagSingleByte(byte[] buffer, int index) => buffer[index] == 0x0F;

    private static bool IsEndTagDoubleByteLE(byte[] buffer, int index) =>
        buffer[index] == 0x0F && buffer[index + 1] == 0x00;

    private static bool IsEndTagDoubleByteBE(byte[] buffer, int index) =>
        buffer[index] == 0x00 && buffer[index + 1] == 0x0F;

    //read tag value (or return max value on error)
    private static ushort ReadTagValue(byte[] buffer, int index, bool bigEndian)
    {
        if (index + 2 > buffer.Length)
            return ushort.MaxValue;

        var bytes = buffer[index..(index + 2)];
        if (bigEndian == BitConverter.IsLittleEndian)
            Array.Reverse(bytes);
        return BitConverter.ToUInt16(bytes);
    }

    //read raw byte array
    private static byte[] ReadArgArray(byte[] buffer, int index, int length)
    {
        if (length == 0)
            return [];
        if (index + length > buffer.Length)
            length = buffer.Length - index;

        return buffer[index..(index + length)];
    }
    #endregion
}
