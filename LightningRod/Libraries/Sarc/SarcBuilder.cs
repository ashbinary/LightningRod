using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Path = System.IO.Path;

namespace LightningRod.Libraries.Sarc
{
    public class SarcBuilder
    {
        private static readonly Dictionary<string, int> AlignmentsForExtensions = new()
        {
            { "bgyml", 0x40 },
            { "byml", 0x40 },
        };

        private const int DefaultAlignment = 0x40;

        private const int DefaultHashKey = 0x65;

        private const int NameTableAlignment = 4;

        public static byte[] Build(List<(string, Memory<byte>)> files)
        {
            var hashKey = DefaultHashKey;

            files.Sort((left, right) =>
            {
                var (leftPath, _) = left;
                var (rightPath, _) = right;

                var leftAlignment = Sarc.Hash(leftPath, (uint)hashKey);
                var rightAlignment = Sarc.Hash(rightPath, (uint)hashKey);

                return leftAlignment.CompareTo(rightAlignment);
            });

            /* Collect up all the sizes of files, rounded up by their respective alignment. */
            var alignedFileSizes = files
                .Select(x => Utils.RoundUp(x.Item2.Length, GetAlignmentForName(x.Item1)))
                .ToList();

            /* Total amount of every file after alignment. */
            var totalFileData = alignedFileSizes.Sum();

            /* Collect up the size of the name table. */
            var nameTableSize = files
                .Select(x => x.Item1)
                .Sum(LengthOfStringForNameTable);

            int firstFileAlignment;
            if (files.Count > 0)
            {
                firstFileAlignment = GetAlignmentForName(files[0].Item1);
            }
            else
            {
                firstFileAlignment = 0;
            }

            var sfatOffset = Unsafe.SizeOf<Sarc.SarcHeader>();
            var fileNodesOffset = sfatOffset + Unsafe.SizeOf<Sarc.SfatHeader>();
            var sfntOffset = fileNodesOffset + (Unsafe.SizeOf<Sarc.FileNode>() * files.Count);
            var nameTableOffset = sfntOffset + Unsafe.SizeOf<Sarc.SfntHeader>();
            var dataStart = Utils.RoundUp(nameTableOffset + nameTableSize, firstFileAlignment);
            var totalData = dataStart + totalFileData;

            var data = new byte[totalData];
            var span = data.AsSpan();

            ref var header = ref span.AsStruct<Sarc.SarcHeader>();
            header.Magic = Sarc.SarcHeader.ExpectedMagic;
            header.HeaderSize = (ushort)Unsafe.SizeOf<Sarc.SarcHeader>();
            header.Bom = 0xFEFF;
            header.FileSize = (uint)totalData;
            header.DataStart = (uint)dataStart;

            ref var sfatHeader = ref span[sfatOffset..].AsStruct<Sarc.SfatHeader>();
            sfatHeader.Magic = Sarc.SfatHeader.ExpectedMagic;
            sfatHeader.HeaderSize = (ushort)Unsafe.SizeOf<Sarc.SfatHeader>();
            sfatHeader.NodeCount = (ushort)files.Count;
            sfatHeader.HashKey = (uint)hashKey;

            var nodes = span[fileNodesOffset..].AsStructSpan<Sarc.FileNode>(files.Count);

            /* Build file nodes. */
            for(var i = 0; i < files.Count; i++)
            {
                var (fileName, _) = files[i];
                ref var node = ref nodes[i];
                node.NameHash = Sarc.Hash(fileName, (uint)hashKey);
                /* NameOffset is done later. */
                node.Flags = 1; /* TODO: why? */
                /* FileData Start/End is done later. */
            }

            ref var sfntHeader = ref span[sfntOffset..].AsStruct<Sarc.SfntHeader>();
            sfntHeader.Magic = Sarc.SfntHeader.ExpectedMagic;
            sfntHeader.HeaderSize = (uint)Unsafe.SizeOf<Sarc.SfntHeader>();

            /* Write out the strings. */
            var currentNameTableOffset = 0;
            var nametable = span.Slice(nameTableOffset, nameTableSize);
            for (var i = 0; i < files.Count; i++)
            {
                var (name, _) = files[i];
                ref var node = ref nodes[i];

                var bytes = Encoding.UTF8.GetBytes(name);
                bytes.CopyTo(nametable[currentNameTableOffset..]);

                node.NameOffset = (uint)currentNameTableOffset / 4;

                currentNameTableOffset += LengthOfStringForNameTable(name);
            }

            /* Write out the file data. */
            var currentDataOffset = 0;
            var dataarea = span.Slice(dataStart, totalData - dataStart);
            for(var i = 0; i < files.Count; i++)
            {
                ref var node = ref nodes[i];
                var (_, fileData) = files[i];

                var alignedSize = alignedFileSizes[i];
                fileData.Span.CopyTo(dataarea.Slice(currentDataOffset, alignedSize));

                node.FileDataBegin = (uint)currentDataOffset;
                node.FileDataEnd = (uint)(currentDataOffset + fileData.Length);

                currentDataOffset += alignedSize;
            }

            return data;
        }

        /* The length of the string, +1 for the null terminator, then aligned. */
        private static int LengthOfStringForNameTable(string name) => Utils.RoundUp(name.Length + 1, NameTableAlignment);

        private static int GetAlignmentForName(string name) => GetAlignmentForExtension(Path.GetExtension(name));

        private static int GetAlignmentForExtension(string extension)
        {
            if (AlignmentsForExtensions.TryGetValue(extension, out var alignment))
            {
                return alignment;
            }

            return DefaultAlignment;
        }
    }
}
