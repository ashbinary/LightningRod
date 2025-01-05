using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NintendoTools.Utils;

internal sealed class FileReader : IDisposable
{
    #region private members
    private static readonly LittleEndianBinaryReader LittleEndianReader = new();
    private static readonly BigEndianBinaryReader BigEndianReader = new();
    private readonly byte[] _buffer;
    private readonly bool _leaveOpen;
    private IBinaryReader _reader;
    private Action<int, int> _readBytes;
    private bool _isBigEndian;
    private bool _disposed;
    #endregion

    #region constructor
    public FileReader(Stream fileStream, bool leaveOpen = false)
    {
        _buffer = new byte[8];
        _leaveOpen = leaveOpen;
        _reader = LittleEndianReader;
        _readBytes = BitConverter.IsLittleEndian ? InternalReadBytes : InternalReadBytesReverse;

        BaseStream = fileStream;
        Position = 0;
    }
    #endregion

    #region public properties
    public bool IsBigEndian
    {
        get => _isBigEndian;
        set
        {
            _isBigEndian = value;
            _reader = value ? BigEndianReader : LittleEndianReader;
            _readBytes = value == BitConverter.IsLittleEndian ? InternalReadBytesReverse : InternalReadBytes;
        }
    }

    public long Position
    {
        get => BaseStream.Position;
        set => BaseStream.Position = value;
    }

    public Stream BaseStream { get; }
    #endregion

    #region public methods
    //skips a number of bytes forward or backwards
    public void Skip(int count) => Position += count;

    //jumps to a certain position in the stream
    public void JumpTo(long position) => Position = position;

    //aligns the stream to the next full given block size
    public void Align(int alignment) => Position += BinaryUtils.GetOffset(Position, alignment);

    //reads an array of raw bytes from the stream
    public byte[] ReadBytes(int length)
    {
        if (length < 1) return [];

        var bytes = new byte[length];
        _ = BaseStream.Read(bytes,  0, length);
        return bytes;
    }
    public byte[] ReadBytesAt(long position, int length)
    {
        Position = position;
        return ReadBytes(length);
    }
    public void ReadBytes(byte[] buffer, int offset, int count) => _ = BaseStream.Read(buffer, offset, count);
    public void ReadBytesAt(long position, byte[] buffer, int offset, int count)
    {
        Position = position;
        ReadBytes(buffer, offset, count);
    }

    //reads a value from stream as sbyte
    public sbyte ReadSByte() => (sbyte) BaseStream.ReadByte();
    public sbyte ReadSByteAt(long position)
    {
        Position = position;
        return ReadSByte();
    }
    public sbyte ReadSByte(int length)
    {
        if (length == 1) return (sbyte) BaseStream.ReadByte();

        _ = BaseStream.Read(_buffer, 0, length);
        return (sbyte) _buffer[IsBigEndian ? 0 : length - 1];
    }
    public sbyte ReadSByteAt(long position, int length)
    {
        Position = position;
        return ReadSByte(length);
    }

    //reads a value from stream as byte
    public byte ReadByte() => (byte) BaseStream.ReadByte();
    public byte ReadByteAt(long position)
    {
        Position = position;
        return ReadByte();
    }
    public byte ReadByte(int length)
    {
        if (length == 1) return (byte) BaseStream.ReadByte();

        _ = BaseStream.Read(_buffer, 0, length);
        return _buffer[IsBigEndian ? 0 : length - 1];
    }
    public byte ReadByteAt(long position, int length)
    {
        Position = position;
        return ReadByte(length);
    }

    //reads a value from stream as short
    public short ReadInt16()
    {
        _ = BaseStream.Read(_buffer, 0, 2);
        return _reader.ReadInt16(_buffer);
    }
    public short ReadInt16At(long position)
    {
        Position = position;
        return ReadInt16();
    }
    public short ReadInt16(int length)
    {
        if (length == 2) return ReadInt16();

        _readBytes(length, 2);
        return _reader.ReadInt16(_buffer);
    }
    public short ReadInt16At(long position, int length)
    {
        Position = position;
        return ReadInt16(length);
    }

    //reads a value from stream as ushort
    public ushort ReadUInt16()
    {
        _ = BaseStream.Read(_buffer, 0, 2);
        return _reader.ReadUInt16(_buffer);
    }
    public ushort ReadUInt16At(long position)
    {
        Position = position;
        return ReadUInt16();
    }
    public ushort ReadUInt16(int length)
    {
        if (length == 2) return ReadUInt16();

        _readBytes(length, 2);
        return _reader.ReadUInt16(_buffer);
    }
    public ushort ReadUInt16At(long position, int length)
    {
        Position = position;
        return ReadUInt16(length);
    }

    //reads a value from stream as int
    public int ReadInt32()
    {
        _ = BaseStream.Read(_buffer, 0, 4);
        return _reader.ReadInt32(_buffer);
    }
    public int ReadInt32At(long position)
    {
        Position = position;
        return ReadInt32();
    }
    public int ReadInt32(int length)
    {
        if (length == 4) return ReadInt32();

        _readBytes(length, 4);
        return _reader.ReadInt32(_buffer);
    }
    public int ReadInt32At(long position, int length)
    {
        Position = position;
        return ReadInt32(length);
    }

    //reads a value from stream as uint
    public uint ReadUInt32()
    {
        _ = BaseStream.Read(_buffer, 0, 4);
        return _reader.ReadUInt32(_buffer);
    }
    public uint ReadUInt32At(long position)
    {
        Position = position;
        return ReadUInt32();
    }
    public uint ReadUInt32(int length)
    {
        if (length == 4) return ReadUInt32();

        _readBytes(length, 4);
        return _reader.ReadUInt32(_buffer);
    }
    public uint ReadUInt32At(long position, int length)
    {
        Position = position;
        return ReadUInt32(length);
    }

    //reads a value from stream as long
    public long ReadInt64()
    {
        _ = BaseStream.Read(_buffer, 0, 8);
        return _reader.ReadInt64(_buffer);
    }
    public long ReadInt64At(long position)
    {
        Position = position;
        return ReadInt64();
    }
    public long ReadInt64(int length)
    {
        if (length == 8) return ReadInt64();

        _readBytes(length, 8);
        return _reader.ReadInt64(_buffer);
    }
    public long ReadInt64At(long position, int length)
    {
        Position = position;
        return ReadInt64(length);
    }

    //reads a value from stream as ulong
    public ulong ReadUInt64()
    {
        _ = BaseStream.Read(_buffer, 0, 8);
        return _reader.ReadUInt64(_buffer);
    }
    public ulong ReadUInt64At(long position)
    {
        Position = position;
        return ReadUInt64();
    }
    public ulong ReadUInt64(int length)
    {
        if (length == 8) return ReadUInt64();

        _readBytes(length, 8);
        return _reader.ReadUInt64(_buffer);
    }
    public ulong ReadUInt64At(long position, int length)
    {
        Position = position;
        return ReadUInt64(length);
    }

    //reads a value from stream as float
    public float ReadSingle()
    {
        _ = BaseStream.Read(_buffer, 0, 4);
        return _reader.ReadSingle(_buffer);
    }
    public float ReadSingleAt(long position)
    {
        Position = position;
        return ReadSingle();
    }
    public float ReadSingle(int length)
    {
        if (length == 4) return ReadSingle();

        _readBytes(length, 4);
        return _reader.ReadSingle(_buffer);
    }
    public float ReadSingleAt(long position, int length)
    {
        Position = position;
        return ReadSingle(length);
    }

    //reads a value from stream as double
    public double ReadDouble()
    {
        _ = BaseStream.Read(_buffer, 0, 8);
        return _reader.ReadDouble(_buffer);
    }
    public double ReadDoubleAt(long position)
    {
        Position = position;
        return ReadDouble();
    }
    public double ReadDouble(int length)
    {
        if (length == 8) return ReadDouble();

        _readBytes(length, 8);
        return _reader.ReadDouble(_buffer);
    }
    public double ReadDoubleAt(long position, int length)
    {
        Position = position;
        return ReadDouble(length);
    }

    //reads a value from stream as hex string
    public string ReadHexString(int length)
    {
        #if NET5_0_OR_GREATER
        Span<byte> bytes = stackalloc byte[length];
        _ = BaseStream.Read(bytes);
        if (!IsBigEndian) bytes.Reverse();
        return Convert.ToHexString(bytes);
        #else
        var bytes = new byte[length];
        _ = BaseStream.Read(bytes);
        if (!IsBigEndian) Array.Reverse(bytes);
        return BitConverter.ToString(bytes).Replace("-", string.Empty);
        #endif
    }
    public string ReadHexStringAt(long position, int length)
    {
        Position = position;
        return ReadHexString(length);
    }

    //reads a value from stream as utf8 string
    public string ReadString(int length) => ReadString(length, Encoding.UTF8);
    public string ReadStringAt(long position, int length) => ReadStringAt(position, length, Encoding.UTF8);

    //reads a value from stream as string with a specific encoding
    public string ReadString(int length, Encoding encoding)
    {
        Span<byte> bytes = stackalloc byte[length];
        _ = BaseStream.Read(bytes);
        return encoding.GetString(bytes).TrimEnd('\0');
    }
    public string ReadStringAt(long position, int length, Encoding encoding)
    {
        Position = position;
        return ReadString(length, encoding);
    }

    //reads a value from stream as utf8 string until encountering a null-byte
    public string ReadTerminatedString(int maxLength = -1) => ReadTerminatedString(Encoding.UTF8, maxLength);
    public string ReadTerminatedStringAt(long position, int maxLength = -1) => ReadTerminatedStringAt(position, Encoding.UTF8, maxLength);

    //reads a value from stream as string with a specific encoding until encountering a null-byte
    public string ReadTerminatedString(Encoding encoding, int maxLength = -1)
    {
        var bytes = new List<byte>(maxLength > 0 ? maxLength : 256);
        var nullByteLength = encoding.GetMinByteCount();

        var nullCount = 0;
        do
        {
            var value = (byte) BaseStream.ReadByte();
            nullCount = value == 0x00 ? nullCount + 1 : 0;
            bytes.Add(value);
        } while (bytes.Count != maxLength && nullCount < nullByteLength);

        //return whatever we have
        if (bytes.Count == maxLength) return encoding.GetString([..bytes]).TrimEnd('\0');

        //append enough null bytes to ensure we have a full null-byte to trim
        for (var i = 0; i < nullByteLength - 1; ++i) bytes.Add(0x00);
        return encoding.GetString([..bytes])[..^1].TrimEnd('\0');
    }
    public string ReadTerminatedStringAt(long position, Encoding encoding, int maxLength = -1)
    {
        Position = position;
        return ReadTerminatedString(encoding, maxLength);
    }
    #endregion

    #region private methods
    //reads an array of raw bytes from the stream
    private void InternalReadBytes(int length, int padding)
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _ = BaseStream.Read(_buffer, 0, length);
    }
    private void InternalReadBytesReverse(int length, int padding)
    {
        Array.Clear(_buffer, 0, _buffer.Length);
        _ = BaseStream.Read(_buffer, padding - length, length);
    }
    #endregion

    #region IDisposable interface
    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed) return;

        if (!_leaveOpen) BaseStream.Dispose();
        _disposed = true;
    }
    #endregion

    #region helper classes
    private interface IBinaryReader
    {
        public short ReadInt16(ReadOnlySpan<byte> buffer);
        public ushort ReadUInt16(ReadOnlySpan<byte> buffer);
        public int ReadInt32(ReadOnlySpan<byte> buffer);
        public uint ReadUInt32(ReadOnlySpan<byte> buffer);
        public long ReadInt64(ReadOnlySpan<byte> buffer);
        public ulong ReadUInt64(ReadOnlySpan<byte> buffer);
        public float ReadSingle(ReadOnlySpan<byte> buffer);
        public double ReadDouble(ReadOnlySpan<byte> buffer);
    }

    private class LittleEndianBinaryReader : IBinaryReader
    {
        public short ReadInt16(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt16LittleEndian(buffer);

        public ushort ReadUInt16(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt16LittleEndian(buffer);

        public int ReadInt32(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt32LittleEndian(buffer);

        public uint ReadUInt32(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt32LittleEndian(buffer);

        public long ReadInt64(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt64LittleEndian(buffer);

        public ulong ReadUInt64(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt64LittleEndian(buffer);

        public float ReadSingle(ReadOnlySpan<byte> buffer)
        {
            #if NET5_0_OR_GREATER
            return BinaryPrimitives.ReadSingleLittleEndian(buffer);
            #else
            var tmpValue = BinaryPrimitives.ReadInt32LittleEndian(buffer);
            return BitConverter.Int32BitsToSingle(tmpValue);
            #endif
        }

        public double ReadDouble(ReadOnlySpan<byte> buffer)
        {
            #if NET5_0_OR_GREATER
            return BinaryPrimitives.ReadDoubleLittleEndian(buffer);
            #else
            var tmpValue = BinaryPrimitives.ReadInt64LittleEndian(buffer);
            return BitConverter.Int64BitsToDouble(tmpValue);
            #endif
        }
    }

    private class BigEndianBinaryReader : IBinaryReader
    {
        public short ReadInt16(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt16BigEndian(buffer);

        public ushort ReadUInt16(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt16BigEndian(buffer);

        public int ReadInt32(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt32BigEndian(buffer);

        public uint ReadUInt32(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt32BigEndian(buffer);

        public long ReadInt64(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadInt64BigEndian(buffer);

        public ulong ReadUInt64(ReadOnlySpan<byte> buffer) => BinaryPrimitives.ReadUInt64BigEndian(buffer);

        public float ReadSingle(ReadOnlySpan<byte> buffer)
        {
            #if NET5_0_OR_GREATER
            return BinaryPrimitives.ReadSingleBigEndian(buffer);
            #else
            var tmpValue = BinaryPrimitives.ReadInt32BigEndian(buffer);
            return BitConverter.Int32BitsToSingle(tmpValue);
            #endif
        }

        public double ReadDouble(ReadOnlySpan<byte> buffer)
        {
            #if NET5_0_OR_GREATER
            return BinaryPrimitives.ReadDoubleBigEndian(buffer);
            #else
            var tmpValue = BinaryPrimitives.ReadInt64BigEndian(buffer);
            return BitConverter.Int64BitsToDouble(tmpValue);
            #endif
        }
    }
    #endregion
}