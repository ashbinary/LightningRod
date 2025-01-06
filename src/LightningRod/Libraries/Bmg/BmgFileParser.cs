using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A class for parsing BMG files.
/// </summary>
public class BmgFileParser : IFileParser<BmgFile>
{
    #region constructor
    static BmgFileParser() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    #endregion

    #region public methods
    /// <inheritdoc cref="IFileParser.CanParse"/>
    /// <exception cref="ArgumentNullException"></exception>
    public static bool CanParseStatic(Stream fileStream)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));
        #else
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));
        #endif

        using var reader = new FileReader(fileStream, true);
        return CanParse(reader, out _);
    }
    #endregion

    #region IFileParser interface
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public bool CanParse(Stream fileStream) => CanParseStatic(fileStream);

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidDataException"></exception>
    public BmgFile Parse(Stream fileStream)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fileStream, nameof(fileStream));
        #else
        if (fileStream is null) throw new ArgumentNullException(nameof(fileStream));
        #endif

        using var reader = new FileReader(fileStream);
        if (!CanParse(reader, out var bigEndianLabels)) throw new InvalidDataException("File is not a BMG file.");

        //parse file metadata and header
        GetMetaData(reader, out var sectionCount, out _, out var encoding);

        //parse messages
        var bmgFile = new BmgFile
        {
            BigEndian = reader.IsBigEndian,
            BigEndianLabels = bigEndianLabels,
            Encoding = encoding
        };
        var ids = Array.Empty<uint>();
        var labels = Array.Empty<string>();
        var messageInfo = Array.Empty<(uint, byte[])>();
        var content = Array.Empty<string>();
        var tags = Array.Empty<List<BmgTag>>();

        long sectionOffset = 0x20;
        for (var i = 0; i < sectionCount; ++i)
        {
            reader.JumpTo(sectionOffset);
            reader.Align(32);

            var type = reader.ReadString(4, Encoding.ASCII);
            var sectionSize = reader.ReadUInt32();
            sectionOffset += sectionSize;

            switch (type)
            {
                case "INF1" or "1FNI":
                    ParseInf1(reader, out messageInfo, out var fileId, out var defaultColor);
                    bmgFile.FileId = fileId;
                    bmgFile.DefaultColor = defaultColor;
                    break;
                case "DAT1" or "1TAD":
                    ParseDat1(reader, sectionSize, messageInfo, encoding, out content, out tags);
                    break;
                case "MID1" or "1DIM":
                    ParseMid1(reader, out ids, out var midFormat);
                    bmgFile.HasMid1 = true;
                    bmgFile.Mid1Format = midFormat;
                    break;
                case "STR1" or "1RTS":
                    ParseStr1(reader, sectionSize, out labels);
                    bmgFile.HasStr1 = true;
                    break;
                case "FLW1" or "1WLF":
                    ParseFlw1(reader, out var flowNodes, out var flowLabels);
                    bmgFile.HasFlw1 = true;
                    bmgFile.FlowData ??= new BmgFlowData();
                    bmgFile.FlowData.Nodes = flowNodes;
                    bmgFile.FlowData.Labels = flowLabels;
                    break;
                case "FLI1" or "1ILF":
                    ParseFli1(reader, out var flowIndices);
                    bmgFile.FlowData ??= new BmgFlowData();
                    bmgFile.FlowData.Indices = flowIndices;
                    break;
                default:
                    throw new InvalidDataException($"Unknown section type: {type}");
            }
        }

        //compile messages
        for (var i = 0; i < content.Length; ++i)
        {
            var message = new BmgMessage
            {
                Id = i < ids.Length ? ids[i] : 0,
                Label = i < labels.Length ? labels[i] : string.Empty,
                Attribute = i < messageInfo.Length ? messageInfo[i].Item2 : new byte[messageInfo.Length > 0 ? messageInfo[0].Item2.Length : 0],
                Text = content[i],
                Tags = tags[i]
            };

            bmgFile.Messages.Add(message);
        }

        return bmgFile;
    }
    #endregion

    #region private methods
    //verifies that the file is a BMG file
    private static bool CanParse(FileReader reader, out bool bigEndianLabels)
    {
        bigEndianLabels = false;
        if (reader.BaseStream.Length < 8) return false;

        switch (reader.ReadStringAt(0, 8, Encoding.ASCII))
        {
            case "MESGbmg1":
                return true;
            case "GSEM1gmb":
                reader.IsBigEndian = true;
                bigEndianLabels = true;
                return true;
            default:
                return false;
        }
    }

    //parses meta data
    private static void GetMetaData(FileReader reader, out uint sectionCount, out uint fileSize, out Encoding encoding)
    {
        fileSize = reader.ReadUInt32At(8);
        if (fileSize > reader.BaseStream.Length) //sanity check: if size is invalid -> file uses other endian (FLW1/FLI1 are not counted to the file size!)
        {
            reader.IsBigEndian = !reader.IsBigEndian;
            fileSize = reader.ReadUInt32At(8);
        }

        sectionCount = reader.ReadUInt32At(12);

        encoding = reader.ReadByteAt(16) switch
        {
            0 => Encoding.GetEncoding(1252),
            1 => Encoding.GetEncoding(1252),
            2 => reader.IsBigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode,
            3 => Encoding.GetEncoding("Shift-JIS"),
            4 => Encoding.UTF8,
            _ => reader.IsBigEndian ? Encoding.BigEndianUnicode : Encoding.Unicode
        };
    }

    //parse INF1 type sections (message info)
    private static void ParseInf1(FileReader reader, out (uint, byte[])[] messageInfo, out ushort fileId, out byte defaultColor)
    {
        var entryCount = reader.ReadUInt16();
        var entrySize = reader.ReadUInt16();
        fileId = reader.ReadUInt16();
        defaultColor = reader.ReadByte();
        reader.Skip(1);

        messageInfo = new (uint, byte[])[entryCount];

        for (var i = 0; i < entryCount; ++i)
        {
            var offset = reader.ReadUInt32();
            var data = reader.ReadBytes(entrySize - 4);
            messageInfo[i] = (offset, data);
        }
    }

    //parse DAT1 type sections (message content)
    private static void ParseDat1(FileReader reader, uint sectionSize, (uint, byte[])[] messageInfo, Encoding encoding, out string[] content, out List<BmgTag>[] tags)
    {
        var sectionStart = reader.Position;
        var sectionEnd = reader.Position + sectionSize - 1;

        var encodingWidth = encoding.GetMinByteCount();
        TagCheck isFunctionTag = encodingWidth == 1 ? IsTagSingleByte : reader.IsBigEndian ? IsTagDoubleByteBE : IsTagDoubleByteLE;
        TagCheck isEndTag = encodingWidth == 1 ? IsEndTagSingleByte : IsEndTagDoubleByte;

        content = new string[messageInfo.Length];
        tags = new List<BmgTag>[messageInfo.Length];

        for (var i = 0; i < messageInfo.Length; ++i)
        {
            //Get the start and end position
            var startPos = messageInfo[i].Item1;
            var endPos = i + 1 < messageInfo.Length ? messageInfo[i + 1].Item1 : sectionEnd;

            //parse message text
            reader.JumpTo(sectionStart + startPos);
            var buffer = reader.ReadBytes((int) (endPos - startPos));

            //check bytes for function calls
            var message = new StringBuilder();
            var messageTags = new List<BmgTag>();
            var textIndex = 0;
            for (var j = 0; j < buffer.Length; j += encodingWidth)
            {
                if (isEndTag(buffer, j))
                {
                    if (j > textIndex) message.Append(encoding.GetString(buffer, textIndex, j - textIndex));
                    textIndex = buffer.Length;
                    break;
                }
                if (!isFunctionTag(buffer, j)) continue;

                //append text so far
                if (j > textIndex) message.Append(encoding.GetString(buffer, textIndex, j - textIndex));
                message.Append("{{").Append(messageTags.Count).Append("}}");

                //add function content
                var tagDataOffset = j + encodingWidth;
                var argLength = tagDataOffset < buffer.Length ? buffer[tagDataOffset] - 4 - encodingWidth : 0;
                messageTags.Add(new BmgTag
                {
                    Group = ReadTagGroup(buffer, tagDataOffset + 1),
                    Type = ReadTagType(buffer, tagDataOffset + 2, reader.IsBigEndian),
                    Args = ReadArgArray(buffer, tagDataOffset + 4, argLength)
                });

                j += 4 + argLength;
                textIndex = j + encodingWidth;
            }

            //append remaining text
            if (textIndex < buffer.Length) message.Append(encoding.GetString(buffer, textIndex, buffer.Length - textIndex));

            content[i] = message.ToString().TrimEnd('\0');
            tags[i] = messageTags;
        }
    }

    //check for tags
    private delegate bool TagCheck(byte[] buffer, int index);
    private static bool IsTagSingleByte(byte[] buffer, int index) => buffer[index] == 0x1A;
    private static bool IsTagDoubleByteLE(byte[] buffer, int index) => buffer[index] == 0x1A && buffer[index + 1] == 0x00;
    private static bool IsTagDoubleByteBE(byte[] buffer, int index) => buffer[index] == 0x00 && buffer[index + 1] == 0x1A;
    private static bool IsEndTagSingleByte(byte[] buffer, int index) => buffer[index] == 0x00;
    private static bool IsEndTagDoubleByte(byte[] buffer, int index) => buffer[index] == 0x00 && buffer[index + 1] == 0x00;

    //read tag group (or return max value on error)
    private static byte ReadTagGroup(byte[] buffer, int index) => index < buffer.Length ? buffer[index] : byte.MaxValue;

    //read tag type (or return max value on error)
    private static ushort ReadTagType(byte[] buffer, int index, bool bigEndian)
    {
        if (index + 2 > buffer.Length) return ushort.MaxValue;

        var bytes = buffer[index..(index + 2)];
        if (bigEndian == BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return BitConverter.ToUInt16(bytes);
    }

    //read raw byte array
    private static byte[] ReadArgArray(byte[] buffer, int index, int length)
    {
        if (length == 0) return [];
        if (index + length > buffer.Length) length = buffer.Length - index;

        return buffer[index..(index + length)];
    }

    //parse MID1 type sections (message IDs)
    private static void ParseMid1(FileReader reader, out uint[] ids, out byte[] midFormat)
    {
        var entryCount = reader.ReadUInt16();
        midFormat = reader.ReadBytes(2);
        reader.Skip(4);

        ids = new uint[entryCount];

        for (var i = 0; i < entryCount; i++)
        {
            ids[i] = reader.ReadUInt32();
        }
    }

    //parse STR1 type sections (message labels)
    private static void ParseStr1(FileReader reader, uint sectionSize, out string[] labels)
    {
        var sectionEnd = reader.Position - 8 + sectionSize;
        reader.Skip(1);

        var labelList = new List<string>();

        while (reader.Position < sectionEnd)
        {
            labelList.Add(reader.ReadTerminatedString());
        }

        labels = [..labelList];
    }

    //parse FLW1 type sections
    private static void ParseFlw1(FileReader reader, out byte[][] nodes, out byte[][] labels)
    {
        var nodeCount = reader.ReadUInt16();
        var labelCount = reader.ReadUInt16();
        reader.Skip(4);

        var nodeList = new List<byte[]>();
        for (var i = 0; i < nodeCount; ++i)
        {
            var nodeData = reader.ReadBytes(8);
            if (nodeData[0] > 0) nodeList.Add(nodeData);
            else break; //padding
        }

        var labelOffset = reader.Position;
        var labelIndexOffset = labelOffset + labelCount * 2;
        var labelList = new List<byte[]>();
        for (var i = 0; i < labelCount; ++i)
        {
            var labelData = new byte[3];
            reader.ReadBytesAt(labelOffset + i * 2, labelData, 0, 2);
            labelData[2] = reader.ReadByteAt(labelIndexOffset + i);
            if (labelData is not [0, 0, 0]) labelList.Add(labelData);
            else break; //padding
        }

        nodes = [..nodeList];
        labels = [..labelList];
    }

    //parse FLI1 type sections
    private static void ParseFli1(FileReader reader, out byte[][] entries)
    {
        var entryCount = reader.ReadUInt16();
        var entrySize = reader.ReadUInt16();
        reader.Skip(4);

        entries = new byte[entryCount][];
        for (var i = 0; i < entryCount; ++i)
        {
            entries[i] = reader.ReadBytes(entrySize);
        }
    }
    #endregion
}