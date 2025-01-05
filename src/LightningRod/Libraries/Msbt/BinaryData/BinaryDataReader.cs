using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using OatmealDome.BinaryData.Extensions;

namespace OatmealDome.BinaryData
{
    /// <summary>
    /// Represents an extended <see cref="BinaryReader"/> supporting special file format data types.
    /// </summary>
    [DebuggerDisplay(
        nameof(BinaryDataReader) + ", " + nameof(Position) + "={" + nameof(Position) + "}"
    )]
    public class BinaryDataReader : BinaryReader
    {
        // ---- CONSTRUCTORS -------------------------------------------------------------------------------------------

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataReader"/> class based on the specified stream and
        /// using UTF-8 encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <exception cref="ArgumentException">The stream does not support reading, is null, or is already closed.
        /// </exception>
        public BinaryDataReader(Stream input)
            : this(input, new UTF8Encoding(), false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataReader"/> class based on the specified stream, UTF-8
        /// encoding and optionally leaves the stream open.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after the <see cref="BinaryDataReader"/> object
        /// is disposed; otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentException">The stream does not support reading, is null, or is already closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">encoding is null.</exception>
        public BinaryDataReader(Stream input, bool leaveOpen)
            : this(input, new UTF8Encoding(), leaveOpen) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataReader"/> class based on the specified stream and
        /// character encoding.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <exception cref="ArgumentException">The stream does not support reading, is null, or is already closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">encoding is null.</exception>
        public BinaryDataReader(Stream input, Encoding encoding)
            : this(input, encoding, false) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryDataReader"/> class based on the specified stream and
        /// character encoding, and optionally leaves the stream open.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <param name="encoding">The character encoding to use.</param>
        /// <param name="leaveOpen"><c>true</c> to leave the stream open after the <see cref="BinaryDataReader"/> object
        /// is disposed; otherwise <c>false</c>.</param>
        /// <exception cref="ArgumentException">The stream does not support reading, is null, or is already closed.
        /// </exception>
        /// <exception cref="ArgumentNullException">encoding is null.</exception>
        public BinaryDataReader(Stream input, Encoding encoding, bool leaveOpen)
            : base(input, encoding, leaveOpen)
        {
            Encoding = encoding;
            ByteOrder = ByteConverter.System.ByteOrder;
        }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets or sets the <see cref="ByteConverter"/> instance used to parse multibyte binary data with.
        /// </summary>
        public ByteConverter ByteConverter { get; set; }

        /// <summary>
        /// Gets or sets the byte order used to parse multibyte binary data with.
        /// </summary>
        public ByteOrder ByteOrder
        {
            get => ByteConverter.ByteOrder;
            set => ByteConverter = ByteConverter.GetConverter(value);
        }

        /// <summary>
        /// Gets the encoding used for string related operations where no other encoding has been provided. Due to the
        /// way the underlying <see cref="BinaryReader"/> is instantiated, it can only be specified at creation time.
        /// </summary>
        public Encoding Encoding { get; }

        /// <summary>
        /// Gets a value indicating whether the end of the stream has been reached and no more data can be read.
        /// </summary>
        public bool EndOfStream => BaseStream.IsEndOfStream();

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        public long Length => BaseStream.Length;

        /// <summary>
        /// Gets or sets the position within the current stream. This is a shortcut to the base stream Position
        /// property.
        /// </summary>
        public long Position
        {
            get => BaseStream.Position;
            set => BaseStream.Position = value;
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Aligns the reader to the next given byte multiple.
        /// </summary>
        /// <param name="alignment">The byte multiple.</param>
        /// <returns>The new position within the current stream.</returns>
        public long Align(int alignment) => BaseStream.Align(alignment);

        /// <summary>
        /// Sets the position within the current stream. This is a shortcut to the base stream Seek method.
        /// </summary>
        /// <param name="offset">A byte offset relative to the current position in the stream.</param>
        /// <returns>The new position within the current stream.</returns>
        public long Seek(long offset) => BaseStream.Seek(offset, SeekOrigin.Current);

        /// <summary>
        /// Sets the position within the current stream. This is a shortcut to the base stream Seek method.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain
        /// the new position.</param>
        /// <returns>The new position within the current stream.</returns>
        public long Seek(long offset, SeekOrigin origin) => BaseStream.Seek(offset, origin);

        /// <summary>
        /// Creates a <see cref="SeekTask"/> to restore the current position after it has been disposed.
        /// </summary>
        /// <returns>The <see cref="SeekTask"/> to be disposed to restore to the current position.</returns>
        public SeekTask TemporarySeek() => BaseStream.TemporarySeek(0, SeekOrigin.Current);

        /// <summary>
        /// Creates a <see cref="SeekTask"/> with the given parameters. As soon as the returned <see cref="SeekTask"/>
        /// is disposed, the previous stream position will be restored.
        /// </summary>
        /// <param name="offset">A byte offset relative to the current position in the stream.</param>
        /// <returns>The <see cref="SeekTask"/> to be disposed to undo the seek.</returns>
        public SeekTask TemporarySeek(long offset) =>
            BaseStream.TemporarySeek(offset, SeekOrigin.Current);

        /// <summary>
        /// Creates a <see cref="SeekTask"/> with the given parameters. As soon as the returned <see cref="SeekTask"/>
        /// is disposed, the previous stream position will be restored.
        /// </summary>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain
        /// the new position.</param>
        /// <returns>The <see cref="SeekTask"/> to be disposed to undo the seek.</returns>
        public SeekTask TemporarySeek(long offset, SeekOrigin origin) =>
            BaseStream.TemporarySeek(offset, origin);

        // ---- Boolean ----

        /// <summary>
        /// Reads a <see cref="Boolean"/> value from the current stream. The <see cref="Boolean"/> is available in the
        /// specified binary format.
        /// </summary>
        /// <param name="format">The binary format, in which the <see cref="Boolean"/> will be read.</param>
        /// <returns>The <see cref="Boolean"/> read from the current stream.</returns>
        public Boolean ReadBoolean(BooleanDataFormat format) => BaseStream.ReadBoolean(format);

        /// <summary>
        /// Reads the specified number of <see cref="Boolean"/> values from the current stream into a
        /// <see cref="Boolean"/> array. The <see cref="Boolean"/> values are available in the specified binary format.
        /// </summary>
        /// <param name="count">The number of <see cref="Boolean"/> values to read.</param>
        /// <param name="format">The binary format, in which the <see cref="Boolean"/> values will be read.</param>
        /// <returns>The <see cref="Boolean"/> array read from the current stream.</returns>
        public Boolean[] ReadBooleans(
            int count,
            BooleanDataFormat format = BooleanDataFormat.Byte
        ) => BaseStream.ReadBooleans(count, format);

        // ---- DateTime ----

        /// <summary>
        /// Reads a <see cref="DateTime"/> from the current stream. The <see cref="DateTime"/> is available in the
        /// specified binary format.
        /// </summary>
        /// <param name="format">The binary format, in which the <see cref="DateTime"/> will be read.</param>
        /// <returns>The <see cref="DateTime"/> read from the current stream.</returns>
        public DateTime ReadDateTime(DateTimeDataFormat format = DateTimeDataFormat.NetTicks) =>
            BaseStream.ReadDateTime(format, ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="DateTime"/> values from the current stream into a
        /// <see cref="DateTime"/> array. The <see cref="DateTime"/> values are available in the specified binary
        /// format.
        /// </summary>
        /// <param name="count">The number of <see cref="DateTime"/> values to read.</param>
        /// <param name="format">The binary format, in which the <see cref="DateTime"/> values will be read.</param>
        /// <returns>The <see cref="DateTime"/> array read from the current stream.</returns>
        public DateTime[] ReadDateTimes(
            int count,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks
        ) => BaseStream.ReadDateTimes(count, format, ByteConverter);

        // ---- Decimal ----

        /// <summary>
        /// Reads an 16-byte floating point value from the current stream and advances the current position of the
        /// stream by sixteen bytes.
        /// </summary>
        /// <returns>The 16-byte floating point value read from the current stream.</returns>
        public override Decimal ReadDecimal() => BaseStream.ReadDecimal(ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="Decimal"/> values from the current stream into a
        /// <see cref="Decimal"/> array and advances the current position by that number of <see cref="Decimal"/> values
        /// multiplied with the size of a single value.
        /// </summary>
        /// <param name="count">The number of <see cref="Decimal"/> values to read.</param>
        /// <returns>The <see cref="Decimal"/> array read from the current stream.</returns>
        public Decimal[] ReadDecimals(int count) => BaseStream.ReadDecimals(count, ByteConverter);

        // ---- Double ----

        /// <summary>
        /// Reads an 8-byte floating point value from the current stream and advances the current position of the stream
        /// by eight bytes.
        /// </summary>
        /// <returns>The 8-byte floating point value read from the current stream.</returns>
        public override Double ReadDouble() => BaseStream.ReadDouble(ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="Double"/> values from the current stream into a
        /// <see cref="Double"/> array and advances the current position by that number of <see cref="Double"/> values
        /// multiplied with the size of a single value.
        /// </summary>
        /// <param name="count">The number of <see cref="Double"/> values to read.</param>
        /// <returns>The <see cref="Double"/> array read from the current stream.</returns>
        public Double[] ReadDoubles(int count) => BaseStream.ReadDoubles(count, ByteConverter);

        // ---- Enum ----

        /// <summary>
        /// Reads the specified enum value from the current stream and advances the current position by the size of the
        /// underlying enum type. Optionally validates the value to be defined in the enum type.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="strict"><c>true</c> to raise an <see cref="ArgumentOutOfRangeException"/> if the value is not
        /// defined in the enum type.</param>
        /// <returns>The enum value read from the current stream.</returns>
        public T ReadEnum<T>(bool strict = false)
            where T : struct, IComparable, IFormattable =>
            BaseStream.ReadEnum<T>(strict, ByteConverter);

        /// <summary>
        /// Reads the specified number of enum values from the current stream into an array of the enum type. Optionally
        /// validates values to be defined in the enum type.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="count">The number of enum values to read.</param>
        /// <param name="strict"><c>true</c> to raise an <see cref="ArgumentOutOfRangeException"/> if a value is not
        /// defined in the enum type.</param>
        /// <returns>The enum values array read from the current stream.</returns>
        public T[] ReadEnums<T>(int count, bool strict = false)
            where T : struct, IComparable, IFormattable =>
            BaseStream.ReadEnums<T>(count, strict, ByteConverter);

        // ---- Int16 ----

        /// <summary>
        /// Reads a 2-byte signed integer from the current stream and advances the current position of the stream by two
        /// bytes.
        /// </summary>
        /// <returns>The 2-byte signed integer read from the current stream.</returns>
        public override Int16 ReadInt16() => BaseStream.ReadInt16(ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="Int16"/> values from the current stream into a <see cref="Int16"/>
        /// array and advances the current position by that number of <see cref="Int16"/> values multiplied with the
        /// size of a single value.
        /// </summary>
        /// <param name="count">The number of <see cref="Int16"/> values to read.</param>
        /// <returns>The <see cref="Int16"/> array read from the current stream.</returns>
        public Int16[] ReadInt16s(int count) => BaseStream.ReadInt16s(count, ByteConverter);

        // ---- Int32 ----

        /// <summary>
        /// Reads a 4-byte signed integer from the current stream and advances the current position of the stream by
        /// four bytes.
        /// </summary>
        /// <returns>The 4-byte signed integer read from the current stream.</returns>
        public override Int32 ReadInt32() => BaseStream.ReadInt32(ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="Int32"/> values from the current stream into a <see cref="Int32"/>
        /// array and advances the current position by that number of <see cref="Int32"/> values multiplied with the
        /// size of a single value.
        /// </summary>
        /// <param name="count">The number of <see cref="Int32"/> values to read.</param>
        /// <returns>The <see cref="Int32"/> array read from the current stream.</returns>
        public Int32[] ReadInt32s(int count) => BaseStream.ReadInt32s(count, ByteConverter);

        // ---- Int64 ----

        /// <summary>
        /// Reads an 8-byte signed integer from the current stream and advances the current position of the stream by
        /// eight bytes.
        /// </summary>
        /// <returns>The 8-byte signed integer read from the current stream.</returns>
        public override Int64 ReadInt64() => BaseStream.ReadInt64(ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="Int64"/> values from the current stream into a <see cref="Int64"/>
        /// array and advances the current position by that number of <see cref="Int64"/> values multiplied with the
        /// size of a single value.
        /// </summary>
        /// <param name="count">The number of <see cref="Int64"/> values to read.</param>
        /// <returns>The <see cref="Int64"/> array read from the current stream.</returns>
        public Int64[] ReadInt64s(int count) => BaseStream.ReadInt64s(count, ByteConverter);

        // ---- Object ----

        /// <summary>
        /// Reads an object of type <typeparamref name="T"/> from the current stream.
        /// </summary>
        /// <typeparam name="T">The type of the object to load.</typeparam>
        /// <returns>The object read from the current stream.</returns>
        public T ReadObject<T>() => BaseStream.ReadObject<T>(ByteConverter);

        /// <summary>
        /// Reads the specified number of objects of type <typeparamref name="T"/> from the current stream.
        /// </summary>
        /// <typeparam name="T">The type of the objects to load.</typeparam>
        /// <param name="count">The number of objects to read.</param>
        /// <returns>The objects array read from the current stream.</returns>
        public T[] ReadObjects<T>(int count) => BaseStream.ReadObjects<T>(count, ByteConverter);

        // ---- SByte ----

        /// <summary>
        /// Reads the specified number of <see cref="SByte"/> values from the current stream into a <see cref="SByte"/>
        /// array and advances the current position by that number of <see cref="SByte"/> values multiplied with the
        /// size of a single value.
        /// </summary>
        /// <param name="count">The number of <see cref="SByte"/> values to read.</param>
        /// <returns>The <see cref="SByte"/> array read from the current stream.</returns>
        public SByte[] ReadSBytes(int count) => BaseStream.ReadSBytes(count);

        // ---- Single ----

        /// <summary>
        /// Reads a 4-byte floating point value from the current stream and advances the current position of the stream
        /// by four bytes.
        /// </summary>
        /// <returns>The 4-byte floating point value read from the current stream.</returns>
        public override Single ReadSingle() => BaseStream.ReadSingle(ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="Single"/> values from the current stream into a
        /// <see cref="Single"/> array and advances the current position by that number of <see cref="Single"/> values
        /// multiplied with the size of a single value.
        /// </summary>
        /// <param name="count">The number of <see cref="Single"/> values to read.</param>
        /// <returns>The <see cref="Single"/> array read from the current stream.</returns>
        public Single[] ReadSingles(int count) => BaseStream.ReadSingles(count, ByteConverter);

        // ---- String ----

        /// <summary>
        /// Reads a string from the current stream. The string is available in the specified binary format and encoding.
        /// </summary>
        /// <param name="format">The binary format, in which the string will be read.</param>
        /// <param name="encoding">The encoding used for converting the string or <c>null</c> to use the encoding
        /// configured for this instance.</param>
        /// <returns>The string read from the current stream.</returns>
        public String ReadString(StringDataFormat format, Encoding encoding = null) =>
            BaseStream.ReadString(format, encoding ?? Encoding, ByteConverter);

        /// <summary>
        /// Reads a string from the current stream. The string has neither a prefix or postfix, the length has to be
        /// specified manually. The string is available in the specified encoding.
        /// </summary>
        /// <param name="length">The length of the string.</param>
        /// <param name="encoding">The encoding used for converting the string or <c>null</c> to use the encoding
        /// configured for this instance.</param>
        /// <returns>The <see cref="String"/> read from the current stream.</returns>
        public String ReadString(int length, Encoding encoding = null) =>
            BaseStream.ReadString(length, encoding ?? Encoding);

        /// <summary>
        /// Reads the specified number of <see cref="String"/> values from the current stream into a
        /// <see cref="String"/> array.
        /// </summary>
        /// <param name="count">The number of <see cref="String"/> values to read.</param>
        /// <returns>The <see cref="String"/> array read from the current stream.</returns>
        public String[] ReadStrings(int count) => BaseStream.ReadStrings(count);

        /// <summary>
        /// Reads the specified number of <see cref="String"/> values from the current stream into a
        /// <see cref="String"/> array. The strings are available in the specified binary format and encoding.
        /// </summary>
        /// <param name="count">The number of <see cref="String"/> values to read.</param>
        /// <param name="format">The binary format, in which the string will be read.</param>
        /// <param name="encoding">The encoding used for converting the string or <c>null</c> to use the encoding
        /// configured for this instance.</param>
        /// <returns>The <see cref="String"/> array read from the current stream.</returns>
        public String[] ReadStrings(int count, StringDataFormat format, Encoding encoding = null) =>
            BaseStream.ReadStrings(count, format, encoding ?? Encoding, ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="String"/> values from the current stream into a
        /// <see cref="String"/> array. The strings have neither a prefix or postfix, the length has to be specified
        /// manually. The strings are available in the specified encoding.
        /// </summary>
        /// <param name="count">The number of <see cref="String"/> values to read.</param>
        /// <param name="length">The length of the string.</param>
        /// <param name="encoding">The encoding used for converting the string or <c>null</c> to use the encoding
        /// configured for this instance.</param>
        /// <returns>The <see cref="String"/> array read from the current stream.</returns>
        public String[] ReadStrings(int count, int length, Encoding encoding = null) =>
            BaseStream.ReadStrings(count, length, encoding ?? Encoding);

        // ---- UInt16 ----

        /// <summary>
        /// Reads a 2-byte unsigned integer from the current stream using little-endian encoding and advances the
        /// position of the stream by two bytes.
        /// </summary>
        /// <returns>The 2-byte unsigned integer read from the current stream.</returns>
        public override UInt16 ReadUInt16() => BaseStream.ReadUInt16(ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="UInt16"/> values from the current stream into a
        /// <see cref="UInt16"/> array and advances the current position by that number of <see cref="UInt16"/> values
        /// multiplied with the size of a single value.
        /// </summary>
        /// <param name="count">The number of <see cref="UInt16"/> values to read.</param>
        /// <returns>The <see cref="UInt16"/> array read from the current stream.</returns>
        public UInt16[] ReadUInt16s(int count) => BaseStream.ReadUInt16s(count, ByteConverter);

        // ---- UInt32 ----

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight
        /// bytes.
        /// </summary>
        /// <returns>The 8-byte unsigned integer read from the current stream.</returns>
        public override UInt32 ReadUInt32() => BaseStream.ReadUInt32(ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="UInt32"/> values from the current stream into a
        /// <see cref="UInt32"/> array and advances the current position by that number of <see cref="UInt32"/> values
        /// multiplied with the size of a single value.
        /// </summary>
        /// <param name="count">The number of <see cref="UInt32"/> values to read.</param>
        /// <returns>The <see cref="UInt32"/> array read from the current stream.</returns>
        public UInt32[] ReadUInt32s(int count) => BaseStream.ReadUInt32s(count, ByteConverter);

        // ---- UInt64 ----

        /// <summary>
        /// Reads an 8-byte unsigned integer from the current stream and advances the position of the stream by eight
        /// bytes.
        /// </summary>
        /// <returns>The 8-byte unsigned integer read from the current stream.</returns>
        public override UInt64 ReadUInt64() => BaseStream.ReadUInt64(ByteConverter);

        /// <summary>
        /// Reads the specified number of <see cref="UInt64"/> values from the current stream into a
        /// <see cref="UInt64"/> array and advances the current position by that number of <see cref="UInt64"/> values
        /// multiplied with the size of a single value.
        /// </summary>
        /// <param name="count">The number of <see cref="UInt64"/> values to read.</param>
        /// <returns>The <see cref="UInt64"/> array read from the current stream.</returns>
        public UInt64[] ReadUInt64s(int count) => BaseStream.ReadUInt64s(count, ByteConverter);
    }
}
