using System;
using System.Buffers.Binary;
using System.IO;
using System.Text;

namespace NintendoTools.Utils;

internal sealed class FileWriter : IDisposable
{
    #region private members
    private static readonly LittleEndianBinaryWriter LittleEndianWriter = new();
    private static readonly BigEndianBinaryWriter BigEndianWriter = new();
    private readonly byte[] _buffer;
    private readonly bool _leaveOpen;
    private IBinaryWriter _writer;
    private bool _isBigEndian;
    private bool _disposed;
    #endregion

    #region constructor
    public FileWriter(Stream fileStream, bool leaveOpen = false)
    {
        _buffer = new byte[8];
        _leaveOpen = leaveOpen;
        _writer = LittleEndianWriter;

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
            _writer = value ? BigEndianWriter : LittleEndianWriter;
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

    //appends the given number of null bytes to the stream
    public void Pad(int count) => BaseStream.Write(stackalloc byte[count]);
    public void Pad(int count, byte value)
    {
        Span<byte> buffer = stackalloc byte[count];
        buffer.Fill(value);
        BaseStream.Write(buffer);
    }

    //aligns and pads the stream to the next full given block size
    public void Align(int alignment)
    {
        var offset = BinaryUtils.GetOffset(Position, alignment);
        if (offset > 0) Pad(offset);
    }
    public void Align(int alignment, byte value)
    {
        var offset = BinaryUtils.GetOffset(Position, alignment);
        if (offset > 0) Pad(offset, value);
    }

    //writes an array of raw bytes to the stream
    public void Write(byte[] value)
    {
        if (value.Length == 0) return;
        BaseStream.Write(value);
    }
    public void Write(byte[] value, int offset, int count)
    {
        if (value.Length == 0) return;
        BaseStream.Write(value, offset, count);
    }

    //writes a span of raw bytes to the stream
    public void Write(ReadOnlySpan<byte> value)
    {
        if (value.Length == 0) return;
        BaseStream.Write(value);
    }

    //writes a sbyte value to stream
    public void Write(sbyte value) => BaseStream.WriteByte((byte) value);

    //writes a byte value to stream
    public void Write(byte value) => BaseStream.WriteByte(value);

    //writes a short value to stream
    public void Write(short value)
    {
        _writer.Write(_buffer, value);
        BaseStream.Write(_buffer, 0, 2);
    }

    //writes an ushort value to stream
    public void Write(ushort value)
    {
        _writer.Write(_buffer, value);
        BaseStream.Write(_buffer, 0, 2);
    }

    //writes an int value to stream
    public void Write(int value)
    {
        _writer.Write(_buffer, value);
        BaseStream.Write(_buffer, 0, 4);
    }

    //writes an uint value to stream
    public void Write(uint value)
    {
        _writer.Write(_buffer, value);
        BaseStream.Write(_buffer, 0, 4);
    }

    //writes a long value to stream
    public void Write(long value)
    {
        _writer.Write(_buffer, value);
        BaseStream.Write(_buffer, 0, 8);
    }

    //writes an ulong value to stream
    public void Write(ulong value)
    {
        _writer.Write(_buffer, value);
        BaseStream.Write(_buffer, 0, 8);
    }

    //writes a float value to stream
    public void Write(float value)
    {
        _writer.Write(_buffer, value);
        BaseStream.Write(_buffer, 0, 4);
    }

    //writes a double value to stream
    public void Write(double value)
    {
        _writer.Write(_buffer, value);
        BaseStream.Write(_buffer, 0, 8);
    }

    //writes a string value to stream
    public void Write(string value) => Write(value, Encoding.UTF8);
    public void Write(string value, Encoding encoding) => BaseStream.Write(encoding.GetBytes(value));

    //writes a null-terminated string value to stream
    public void WriteTerminated(string value) => Write(value + "\0", Encoding.UTF8);
    public void WriteTerminated(string value, Encoding encoding) => Write(value + "\0", encoding);
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
    private interface IBinaryWriter
    {
        public void Write(Span<byte> buffer, short value);
        public void Write(Span<byte> buffer, ushort value);
        public void Write(Span<byte> buffer, int value);
        public void Write(Span<byte> buffer, uint value);
        public void Write(Span<byte> buffer, long value);
        public void Write(Span<byte> buffer, ulong value);
        public void Write(Span<byte> buffer, float value);
        public void Write(Span<byte> buffer, double value);
    }

    private class LittleEndianBinaryWriter : IBinaryWriter
    {
        public void Write(Span<byte> buffer, short value) => BinaryPrimitives.WriteInt16LittleEndian(buffer, value);

        public void Write(Span<byte> buffer, ushort value) => BinaryPrimitives.WriteUInt16LittleEndian(buffer, value);

        public void Write(Span<byte> buffer, int value) => BinaryPrimitives.WriteInt32LittleEndian(buffer, value);

        public void Write(Span<byte> buffer, uint value) => BinaryPrimitives.WriteUInt32LittleEndian(buffer, value);

        public void Write(Span<byte> buffer, long value) => BinaryPrimitives.WriteInt64LittleEndian(buffer, value);

        public void Write(Span<byte> buffer, ulong value) => BinaryPrimitives.WriteUInt64LittleEndian(buffer, value);

        public void Write(Span<byte> buffer, float value)
        {
            #if NET5_0_OR_GREATER
            BinaryPrimitives.WriteSingleLittleEndian(buffer, value);
            #else
            var tmpValue = BitConverter.SingleToInt32Bits(value);
            BinaryPrimitives.WriteInt64LittleEndian(buffer, tmpValue);
            #endif
        }

        public void Write(Span<byte> buffer, double value)
        {
            #if NET5_0_OR_GREATER
            BinaryPrimitives.WriteDoubleLittleEndian(buffer, value);
            #else
            var tmpValue = BitConverter.DoubleToInt64Bits(value);
            BinaryPrimitives.WriteInt64LittleEndian(buffer, tmpValue);
            #endif
        }
    }

    private class BigEndianBinaryWriter : IBinaryWriter
    {
        public void Write(Span<byte> buffer, short value) => BinaryPrimitives.WriteInt16BigEndian(buffer, value);

        public void Write(Span<byte> buffer, ushort value) => BinaryPrimitives.WriteUInt16BigEndian(buffer, value);

        public void Write(Span<byte> buffer, int value) => BinaryPrimitives.WriteInt32BigEndian(buffer, value);

        public void Write(Span<byte> buffer, uint value) => BinaryPrimitives.WriteUInt32BigEndian(buffer, value);

        public void Write(Span<byte> buffer, long value) => BinaryPrimitives.WriteInt64BigEndian(buffer, value);

        public void Write(Span<byte> buffer, ulong value) => BinaryPrimitives.WriteUInt64BigEndian(buffer, value);

        public void Write(Span<byte> buffer, float value)
        {
            #if NET5_0_OR_GREATER
            BinaryPrimitives.WriteSingleBigEndian(buffer, value);
            #else
            var tmpValue = BitConverter.SingleToInt32Bits(value);
            BinaryPrimitives.WriteInt32BigEndian(buffer, tmpValue);
            #endif
        }

        public void Write(Span<byte> buffer, double value)
        {
            #if NET5_0_OR_GREATER
            BinaryPrimitives.WriteDoubleBigEndian(buffer, value);
            #else
            var tmpValue = BitConverter.DoubleToInt64Bits(value);
            BinaryPrimitives.WriteInt64BigEndian(buffer, tmpValue);
            #endif
        }
    }
    #endregion
}