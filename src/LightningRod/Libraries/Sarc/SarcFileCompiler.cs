using System;
using System.Globalization;
using System.IO;
using System.Text;
using NintendoTools.Hashing;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Sarc;

/// <summary>
/// A class for compiling SARC archives.
/// </summary>
public class SarcFileCompiler : IFileCompiler<SarcFile>
{
    #region private members
    private static readonly Crc32Hash Hash = new();
    #endregion

    #region public properties
    /// <summary>
    /// Gets or sets the alignment table to use.
    /// </summary>
    public AlignmentTable Alignment { get; set; } = new() { Default = 8 };
    #endregion

    #region IFileCompiler interface
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public void Compile(SarcFile file, Stream fileStream)
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

        //write header
        writer.Write("SARC", Encoding.ASCII);
        writer.Write((ushort)0x14);
        writer.Write((ushort)0xFEFF);
        writer.Pad(8);
        writer.Write((ushort)file.Version);
        writer.Pad(2);

        //write SFAT header
        writer.Write("SFAT", Encoding.ASCII);
        writer.Write((ushort)0x0C);
        writer.Write((ushort)file.Files.Count);
        writer.Write(BitConverter.GetBytes(file.HashKey));

        //build hash map
        var sortedFiles = new (SarcContent, uint, int)[file.Files.Count];
        var maxAlignment = 0;
        for (var i = 0; i < sortedFiles.Length; ++i)
        {
            var content = file.Files[i];
            var alignment = Alignment.Default;

            byte[] nameHash;
            if (file.HasFileNames)
            {
                nameHash = BitConverter.GetBytes(GetNameHash(content.Name, file.HashKey));
                alignment = Alignment.GetFromName(content.Name);
            }
            else if (!TryParseNameHash(content.Name, out nameHash))
            {
                nameHash = Hash.Compute(Encoding.UTF8.GetBytes(Path.GetFileName(content.Name)));
            }

            sortedFiles[i] = (content, BitConverter.ToUInt32(nameHash), alignment);
            if (alignment > maxAlignment)
                maxAlignment = alignment;
        }
        Array.Sort(sortedFiles, (v1, v2) => v1.Item2.CompareTo(v2.Item2));

        //write SFAT nodes
        var currentNameOffset = 0;
        var currentDataOffset = 0;
        var lastHash = 0u;
        var hashCollisionIndex = 1u;
        foreach (var (content, nameHash, alignment) in sortedFiles)
        {
            writer.Write(BitConverter.GetBytes(nameHash));
            if (file.HasFileNames)
            {
                if (lastHash == nameHash)
                    ++hashCollisionIndex;
                else
                    hashCollisionIndex = 1;
                lastHash = nameHash;

                writer.Write((hashCollisionIndex << 24) | ((uint)currentNameOffset / 4));

                currentNameOffset += content.Name.Length + 1;
                currentNameOffset += BinaryUtils.GetOffset(currentNameOffset, 4);
            }
            else
                writer.Pad(4);
            writer.Write(currentDataOffset += BinaryUtils.GetOffset(currentDataOffset, alignment));
            writer.Write(currentDataOffset += content.Data.Length);
        }

        //write SFNT header
        writer.Write("SFNT", Encoding.ASCII);
        writer.Write((ushort)0x08);
        writer.Pad(2);
        if (file.HasFileNames)
        {
            foreach (var (content, _, _) in sortedFiles)
            {
                writer.Write(content.Name);
                writer.Write((byte)0x00);
                writer.Align(4);
            }
        }

        //write data
        writer.Align(maxAlignment);
        var dataOffset = writer.Position;
        for (var i = 0; i < sortedFiles.Length; ++i)
        {
            writer.Align(sortedFiles[i].Item3);
            if (i == 0)
                dataOffset = writer.Position;

            writer.Write(sortedFiles[i].Item1.Data);
        }

        //write file size and data offset
        var fileSize = writer.Position;
        writer.JumpTo(0x08);
        writer.Write((uint)fileSize);
        writer.Write((uint)dataOffset);
    }
    #endregion

    #region private methods
    private static int GetNameHash(string name, int hashKey)
    {
        var hash = 0;
        foreach (var c in name)
            hash = hash * hashKey + c;
        return hash;
    }

    private static bool TryParseNameHash(string name, out byte[] hash)
    {
        hash = [];
        if (!name.StartsWith("0x") || name.Length != 10)
            return false;

        hash = new byte[4];
        for (var i = 0; i < hash.Length; ++i)
        {
            if (
                !byte.TryParse(
                    name.AsSpan(2 + i * 2, 2),
                    NumberStyles.AllowHexSpecifier,
                    CultureInfo.InvariantCulture,
                    out var value
                )
            )
                return false;
            hash[i] = value;
        }

        return true;
    }
    #endregion
}
