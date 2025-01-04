using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace LightningRod.Libraries.Sarc
{
    public ref struct Sarc
    {
        [StructLayout(LayoutKind.Sequential, Size = 0x14)]
        public struct Header
        {
            public static uint ExpectedMagic = 0x43524153;

            public uint Magic;
            public ushort HeaderSize;
            public ushort Bom;
            public uint FileSize;
            public uint DataStart;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0xC)]
        public struct SfatHeader
        {
            public static uint ExpectedMagic = 0x54414653;

            public uint Magic;
            public ushort HeaderSize;
            public ushort NodeCount;
            public uint HashKey;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x8)]
        public struct SfntHeader
        {
            public static uint ExpectedMagic = 0x544E4653;

            public uint Magic;
            public uint HeaderSize;
        }

        [StructLayout(LayoutKind.Sequential, Size = 0x10)]
        public struct FileNode : IComparable<uint>, IComparable<FileNode>
        {
            /* Flags are the upper byte of the uint, so 24 bits up. */
            private const int FlagsShift = 24;
            private const uint NameOffsetMask = (1 << FlagsShift) - 1;
            private const uint FlagsMask = ~NameOffsetMask;

            public uint NameHash;
            public uint FlagsAndNameOffset;
            public uint FileDataBegin;
            public uint FileDataEnd;

            public uint NameOffset
            {
                get => FlagsAndNameOffset & NameOffsetMask;
                set =>
                    FlagsAndNameOffset =
                        (FlagsAndNameOffset & FlagsMask) | (value & NameOffsetMask);
            }

            public uint Flags
            {
                get => FlagsAndNameOffset >> 24;
                set =>
                    FlagsAndNameOffset =
                        (value << FlagsShift) | (FlagsAndNameOffset & NameOffsetMask);
            }
            public uint FileDataLength => FileDataEnd - FileDataBegin;

            public int CompareTo(uint other)
            {
                return NameHash.CompareTo(other);
            }

            public int CompareTo(FileNode other)
            {
                return CompareTo(other.NameHash);
            }
        }

        private readonly int SfatOffset => Unsafe.SizeOf<Header>();
        private readonly int SfatNodesOffset => SfatOffset + Unsafe.SizeOf<SfatHeader>();
        private readonly int SfatNodesEnd =>
            SfatNodesOffset + (Unsafe.SizeOf<FileNode>() * Sfat.NodeCount);
        private readonly int SfntStart => SfatNodesEnd;
        private readonly int SfntNameTableStart => SfntStart + Unsafe.SizeOf<SfntHeader>();

        private readonly Span<byte> Data;
        public readonly ref Header SarcHeader;

        private readonly ref SfatHeader Sfat;
        public Span<FileNode> FileNodes;

        private ref SfntHeader Sfnt;
        private Span<byte> NameTable;

        private Span<byte> FileData;

        public Sarc(Span<byte> data)
        {
            Data = data;
            SarcHeader = ref MemoryMarshal.AsRef<Header>(Data);

            /* Validate SARC header. */
            if (SarcHeader.Bom != 0xFEFF)
                throw new NotImplementedException("Big endian SARCs are not supported!");
            if (SarcHeader.Magic != 0x43524153)
                throw new System.IO.InvalidDataException("Invalid SARC magic!");
            if (SarcHeader.HeaderSize != Unsafe.SizeOf<Header>())
                throw new System.IO.InvalidDataException("Invalid SARC header size!");

            /* Validate SFAT. */
            Sfat = ref MemoryMarshal.AsRef<SfatHeader>(Data[SfatOffset..]);
            if (Sfat.Magic != 0x54414653)
                throw new System.IO.InvalidDataException("Invalid SFAT magic!");
            if (Sfat.HeaderSize != Unsafe.SizeOf<SfatHeader>())
                throw new System.IO.InvalidDataException("Invalid SFAT header size!");
            if (Sfat.NodeCount >> 0xE != 0)
                throw new System.IO.InvalidDataException("Invalid SFAT node count!");
            FileNodes = MemoryMarshal.Cast<byte, FileNode>(Data[SfatNodesOffset..SfatNodesEnd]);

            /* Validate SFNT. */
            Sfnt = ref MemoryMarshal.AsRef<SfntHeader>(Data[SfntStart..]);
            if (Sfnt.Magic != 0x544E4653)
                throw new System.IO.InvalidDataException("Invalid SNFT magic!");
            if (Sfnt.HeaderSize != Unsafe.SizeOf<SfntHeader>())
                throw new System.IO.InvalidDataException("Invalid SNFT header size!");
            NameTable = Data.Slice(SfntNameTableStart, (int)(Data.Length - SarcHeader.DataStart));

            FileData = Data[(int)SarcHeader.DataStart..];
        }

        public string GetNodeFilename(in FileNode node)
        {
            var idx = (int)(node.NameOffset * 4);

            var slice = NameTable[idx..];
            int length = slice.IndexOf(byte.MinValue);
            if (length < 0)
                length = slice.Length;

            return Encoding.UTF8.GetString(slice[..length]);
        }

        public uint Hash(string str)
        {
            uint hash = 0;

            for (int i = 0; i < str.Length; i++)
                hash = hash * Sfat.HashKey + str[i];

            return hash;
        }

        public static uint Hash(string str, uint key)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            uint hash = 0;
            foreach (var b in bytes)
            {
                hash = hash * key + b;
            }
            return hash;
        }

        public int GetNodeIndex(string path)
        {
            var hash = Hash(path);

            return Utils.BinarySearch<FileNode, uint>(FileNodes, hash);
        }

        public Span<byte> OpenFile(int idx) => OpenFile(in FileNodes[idx]);

        public Span<byte> OpenFile(in FileNode node) =>
            FileData.Slice((int)node.FileDataBegin, (int)node.FileDataLength);
    }
}
