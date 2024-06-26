using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using OatmealDome.BinaryData.Core;

namespace OatmealDome.BinaryData.Extensions
{  
    public static partial class StreamExtensions
    {
        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        // ---- Boolean ----

        /// <summary>
        /// Returns a <see cref="Boolean"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="format">The <see cref="BooleanDataFormat"/> format in which the data is stored.</param>
        /// <returns>The value read from the current stream.</returns>
        public static Boolean ReadBoolean(this Stream stream,
            BooleanDataFormat format = BooleanDataFormat.Byte)
        {
            switch (format)
            {
                case BooleanDataFormat.Byte:
                    return stream.ReadByte() != 0;
                case BooleanDataFormat.Word:
                    return ReadInt16(stream) != 0;
                case BooleanDataFormat.Dword:
                    return ReadInt32(stream) != 0;
                default:
                    throw new ArgumentException($"Invalid {nameof(BooleanDataFormat)}.", nameof(format));
            }
        }

        /// <summary>
        /// Returns an array of <see cref="Boolean"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="format">The <see cref="BooleanDataFormat"/> format in which the data is stored.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static Boolean[] ReadBooleans(this Stream stream, int count,
            BooleanDataFormat format = BooleanDataFormat.Byte)
        {
            var values = new Boolean[count];
            lock (stream)
            {
                switch (format)
                {
                    case BooleanDataFormat.Byte:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = stream.ReadByte() != 0;
                        }
                        break;
                    case BooleanDataFormat.Word:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = ReadInt16(stream) != 0;
                        }
                        break;
                    case BooleanDataFormat.Dword:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = ReadInt32(stream) != 0;
                        }
                        break;
                    default:
                        throw new ArgumentException($"Invalid {nameof(BooleanDataFormat)}.", nameof(format));
                }
            }
            return values;
        }

        // ---- Byte ----

        /// <summary>
        /// Returns a <see cref="Byte"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <returns>The value read from the current stream.</returns>
        public static Byte Read1Byte(this Stream stream)
        {
            return (Byte)stream.ReadByte();
        }

        /// <summary>
        /// Returns an array of <see cref="Byte"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static Byte[] ReadBytes(this Stream stream, int count)
        {
            byte[] buffer = new byte[count];
            stream.Read(buffer, 0, count);
            return buffer;
        }

        // ---- DateTime ----

        /// <summary>
        /// Returns a <see cref="DateTime"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="format">The <see cref="DateTimeDataFormat"/> format in which the data is stored.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static DateTime ReadDateTime(this Stream stream,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks, ByteConverter converter = null)
        {
            switch (format)
            {
                case DateTimeDataFormat.NetTicks:
                    return new DateTime(ReadInt64(stream, converter));
                case DateTimeDataFormat.CTime:
                    return _cTimeBase.AddSeconds(ReadUInt32(stream, converter));
                case DateTimeDataFormat.CTime64:
                    return _cTimeBase.AddSeconds(ReadInt64(stream, converter));
                default:
                    throw new ArgumentException($"Invalid {nameof(DateTimeDataFormat)}.", nameof(format));
            }
        }

        /// <summary>
        /// Returns an array of <see cref="DateTime"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="format">The <see cref="DateTimeDataFormat"/> format in which the data is stored.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static DateTime[] ReadDateTimes(this Stream stream, int count,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks, ByteConverter converter = null)
        {
            var values = new DateTime[count];
            lock (stream)
            {
                switch (format)
                {
                    case DateTimeDataFormat.NetTicks:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = new DateTime(ReadInt64(stream, converter));
                        }
                        break;
                    case DateTimeDataFormat.CTime:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = _cTimeBase.AddSeconds(ReadUInt32(stream, converter));
                        }
                        break;
                    case DateTimeDataFormat.CTime64:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = _cTimeBase.AddSeconds(ReadInt64(stream, converter));
                        }
                        break;
                    default:
                        throw new ArgumentException($"Invalid {nameof(BooleanDataFormat)}.", nameof(format));
                }
            }
            return values;
        }

        // ---- Decimal ----

        /// <summary>
        /// Returns a <see cref="Decimal"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static Decimal ReadDecimal(this Stream stream, ByteConverter converter = null)
        {
            FillBuffer(stream, sizeof(Decimal));
            return (converter ?? ByteConverter.System).ToDecimal(Buffer);
        }

        /// <summary>
        /// Returns an array of <see cref="Decimal"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static Decimal[] ReadDecimals(this Stream stream, int count, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            var values = new Decimal[count];
            lock (stream)
            {
                byte[] buffer = Buffer;
                for (int i = 0; i < count; i++)
                {
                    FillBuffer(stream, sizeof(Decimal));
                    values[i] = converter.ToDecimal(buffer);
                }
            }
            return values;
        }

        // ---- Double ----

        /// <summary>
        /// Returns a <see cref="Double"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static Double ReadDouble(this Stream stream, ByteConverter converter = null)
        {
            FillBuffer(stream, sizeof(Double));
            return (converter ?? ByteConverter.System).ToDouble(Buffer);
        }

        /// <summary>
        /// Returns an array of <see cref="Double"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static Double[] ReadDoubles(this Stream stream, int count, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            var values = new Double[count];
            lock (stream)
            {
                byte[] buffer = Buffer;
                for (int i = 0; i < count; i++)
                {
                    FillBuffer(stream, sizeof(Double));
                    values[i] = converter.ToDouble(buffer);
                }
            }
            return values;
        }

        // ---- Enum ----

        /// <summary>
        /// Returns an <see cref="Enum"/> instance of type <typeparamref name="T"/> from the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="strict"><c>true</c> to raise an <see cref="ArgumentOutOfRangeException"/> if a value is not
        /// defined in the enum type.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static T ReadEnum<T>(this Stream stream, bool strict = false, ByteConverter converter = null)
            where T : struct, IComparable, IFormattable
            => (T)ReadEnum(stream, typeof(T), strict, converter);

        /// <summary>
        /// Returns an array of <see cref="Enum"/> instances of type <typeparamref name="T"/> read from the
        /// <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="strict"><c>true</c> to raise an <see cref="ArgumentOutOfRangeException"/> if a value is not
        /// defined in the enum type.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static T[] ReadEnums<T>(this Stream stream, int count, bool strict = false,
            ByteConverter converter = null)
            where T : struct, IComparable, IFormattable
        {
            converter = converter ?? ByteConverter.System;
            var values = new T[count];
            Type enumType = typeof(T);
            lock (stream)
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = (T)ReadEnum(stream, enumType, strict, converter);
                }
            }
            return values;
        }

        // ---- Int16 ----

        /// <summary>
        /// Returns an <see cref="Int16"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static Int16 ReadInt16(this Stream stream, ByteConverter converter = null)
        {
            FillBuffer(stream, sizeof(Int16));
            return (converter ?? ByteConverter.System).ToInt16(Buffer);
        }

        /// <summary>
        /// Returns an array of <see cref="Int16"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static Int16[] ReadInt16s(this Stream stream, int count, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            var values = new Int16[count];
            lock (stream)
            {
                byte[] buffer = Buffer;
                for (int i = 0; i < count; i++)
                {
                    FillBuffer(stream, sizeof(Int16));
                    values[i] = converter.ToInt16(buffer);
                }
            }
            return values;
        }

        // ---- Int32 ----

        /// <summary>
        /// Returns an <see cref="Int32"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static Int32 ReadInt32(this Stream stream, ByteConverter converter = null)
        {
            FillBuffer(stream, sizeof(Int32));
            return (converter ?? ByteConverter.System).ToInt32(Buffer);
        }

        /// <summary>
        /// Returns an array of <see cref="Int32"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static Int32[] ReadInt32s(this Stream stream, int count, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            var values = new Int32[count];
            lock (stream)
            {
                byte[] buffer = Buffer;
                for (int i = 0; i < count; i++)
                {
                    FillBuffer(stream, sizeof(Int32));
                    values[i] = converter.ToInt32(buffer);
                }
            }
            return values;
        }

        // ---- Int64 ----

        /// <summary>
        /// Returns an <see cref="Int64"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static Int64 ReadInt64(this Stream stream, ByteConverter converter = null)
        {
            FillBuffer(stream, sizeof(Int64));
            return (converter ?? ByteConverter.System).ToInt64(Buffer);
        }

        /// <summary>
        /// Returns an array of <see cref="Int64"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static Int64[] ReadInt64s(this Stream stream, int count, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            var values = new Int64[count];
            lock (stream)
            {
                byte[] buffer = Buffer;
                for (int i = 0; i < count; i++)
                {
                    FillBuffer(stream, sizeof(Int64));
                    values[i] = converter.ToInt64(buffer);
                }
            }
            return values;
        }

        // ---- Object ----

        /// <summary>
        /// Returns an object of type <typeparamref name="T"/> read from the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to read.</typeparam>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static T ReadObject<T>(this Stream stream, ByteConverter converter = null)
            => (T)ReadObject(stream, null, BinaryMemberAttribute.Default, typeof(T), converter);

        /// <summary>
        /// Returns an array of objects of type <typeparamref name="T"/> read from the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of the object to load.</typeparam>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static T[] ReadObjects<T>(this Stream stream, int count, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            var values = new T[count];
            lock (stream)
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = stream.ReadObject<T>(converter);
                }
            }
            return values;
        }

        // ---- SByte ----

        /// <summary>
        /// Returns an <see cref="SByte"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <returns>The value read from the current stream.</returns>
        public static SByte ReadSByte(this Stream stream)
        {
            return (SByte)stream.ReadByte();
        }

        /// <summary>
        /// Returns an array of <see cref="SByte"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static SByte[] ReadSBytes(this Stream stream, int count)
        {
            var values = new SByte[count];
            lock (stream)
            {
                byte[] buffer = Buffer;
                for (int i = 0; i < count; i++)
                {
                    FillBuffer(stream, sizeof(SByte));
                    values[i] = (SByte)buffer[0];
                }
            }
            return values;
        }

        // ---- Single ----

        /// <summary>
        /// Returns a <see cref="Single"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static Single ReadSingle(this Stream stream, ByteConverter converter = null)
        {
            FillBuffer(stream, sizeof(Single));
            return (converter ?? ByteConverter.System).ToSingle(Buffer);
        }

        /// <summary>
        /// Returns an array of <see cref="Single"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static Single[] ReadSingles(this Stream stream, int count, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            var values = new Single[count];
            lock (stream)
            {
                byte[] buffer = Buffer;
                for (int i = 0; i < count; i++)
                {
                    FillBuffer(stream, sizeof(Single));
                    values[i] = converter.ToSingle(buffer);
                }
            }
            return values;
        }

        // ---- String ----

        /// <summary>
        /// Returns a <see cref="String"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="format">The <see cref="StringDataFormat"/> format determining how the length of the string is
        /// stored.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to parse the bytes with, or <c>null</c> to use
        /// <see cref="Encoding.UTF8"/>.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static String ReadString(this Stream stream,
            StringDataFormat format = StringDataFormat.DynamicByteCount, Encoding encoding = null,
            ByteConverter converter = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            converter = converter ?? ByteConverter.System;
            switch (format)
            {
                case StringDataFormat.DynamicByteCount:
                    return ReadStringWithLength(stream, Read7BitEncodedInt32(stream), false, encoding);
                case StringDataFormat.ByteCharCount:
                    return ReadStringWithLength(stream, stream.ReadByte(), true, encoding);
                case StringDataFormat.Int16CharCount:
                    return ReadStringWithLength(stream, ReadInt16(stream, converter), true, encoding);
                case StringDataFormat.Int32CharCount:
                    return ReadStringWithLength(stream, ReadInt32(stream, converter), true, encoding);
                case StringDataFormat.ZeroTerminated:
                    return ReadStringZeroPostfix(stream, encoding);
                default:
                    throw new ArgumentException($"Invalid {nameof(StringDataFormat)}.", nameof(format));
            }
        }

        /// <summary>
        /// Returns an array of <see cref="String"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="format">The <see cref="StringDataFormat"/> format determining how the length of the strings is
        /// stored.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to parse the bytes with, or <c>null</c> to use
        /// <see cref="Encoding.UTF8"/>.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static String[] ReadStrings(this Stream stream, int count,
            StringDataFormat format = StringDataFormat.DynamicByteCount, Encoding encoding = null,
            ByteConverter converter = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            converter = converter ?? ByteConverter.System;
            var values = new String[count];
            lock (stream)
            {
                switch (format)
                {
                    case StringDataFormat.DynamicByteCount:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = ReadStringWithLength(stream, Read7BitEncodedInt32(stream), false, encoding);
                        }
                        break;
                    case StringDataFormat.ByteCharCount:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = ReadStringWithLength(stream, stream.ReadByte(), true, encoding);
                        }
                        break;
                    case StringDataFormat.Int16CharCount:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = ReadStringWithLength(stream, ReadInt16(stream, converter), true, encoding);
                        }
                        break;
                    case StringDataFormat.Int32CharCount:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = ReadStringWithLength(stream, ReadInt32(stream, converter), true, encoding);
                        }
                        break;
                    case StringDataFormat.ZeroTerminated:
                        for (int i = 0; i < count; i++)
                        {
                            values[i] = ReadStringZeroPostfix(stream, encoding);
                        }
                        break;
                    default:
                        throw new ArgumentException($"Invalid {nameof(StringDataFormat)}.", nameof(format));
                }
            }
            return values;
        }

        /// <summary>
        /// Returns a <see cref="String"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="length">The length of the string.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to parse the decode the chars with, or <c>null</c> to use
        /// <see cref="Encoding.UTF8"/>.</param>
        /// <returns>The value read from the current stream.</returns>
        public static String ReadString(this Stream stream, int length, Encoding encoding = null)
        {
            return ReadStringWithLength(stream, length, true, encoding ?? Encoding.UTF8);
        }

        /// <summary>
        /// Returns an array of <see cref="String"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="length">The length of the string.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to parse the bytes with, or <c>null</c> to use
        /// <see cref="Encoding.UTF8"/>.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static String[] ReadStrings(this Stream stream, int count, int length, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var values = new String[count];
            lock (stream)
            {
                for (int i = 0; i < count; i++)
                {
                    values[i] = ReadStringWithLength(stream, length, true, encoding);
                }
            }
            return values;
        }

        // ---- UInt16 ----

        /// <summary>
        /// Returns a <see cref="UInt16"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static UInt16 ReadUInt16(this Stream stream, ByteConverter converter = null)
        {
            FillBuffer(stream, sizeof(UInt16));
            return (converter ?? ByteConverter.System).ToUInt16(Buffer);
        }

        /// <summary>
        /// Returns an array of <see cref="UInt16"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static UInt16[] ReadUInt16s(this Stream stream, int count, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            var values = new UInt16[count];
            lock (stream)
            {
                byte[] buffer = Buffer;
                for (int i = 0; i < count; i++)
                {
                    FillBuffer(stream, sizeof(UInt16));
                    values[i] = converter.ToUInt16(buffer);
                }
            }
            return values;
        }

        // ---- UInt32 ----

        /// <summary>
        /// Returns a <see cref="UInt32"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static UInt32 ReadUInt32(this Stream stream, ByteConverter converter = null)
        {
            FillBuffer(stream, sizeof(UInt32));
            return (converter ?? ByteConverter.System).ToUInt32(Buffer);
        }

        /// <summary>
        /// Returns an array of <see cref="UInt32"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static UInt32[] ReadUInt32s(this Stream stream, int count, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            var values = new UInt32[count];
            lock (stream)
            {
                byte[] buffer = Buffer;
                for (int i = 0; i < count; i++)
                {
                    FillBuffer(stream, sizeof(UInt32));
                    values[i] = converter.ToUInt32(buffer);
                }
            }
            return values;
        }

        // ---- UInt64 ----

        /// <summary>
        /// Returns a <see cref="UInt64"/> instance read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The value read from the current stream.</returns>
        public static UInt64 ReadUInt64(this Stream stream, ByteConverter converter = null)
        {
            FillBuffer(stream, sizeof(UInt64));
            return (converter ?? ByteConverter.System).ToUInt64(Buffer);
        }

        /// <summary>
        /// Returns an array of <see cref="UInt64"/> instances read from the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="count">The number of values to read.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        /// <returns>The array of values read from the current stream.</returns>
        public static UInt64[] ReadUInt64s(this Stream stream, int count, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            var values = new UInt64[count];
            lock (stream)
            {
                byte[] buffer = Buffer;
                for (int i = 0; i < count; i++)
                {
                    FillBuffer(stream, sizeof(UInt64));
                    values[i] = converter.ToUInt64(buffer);
                }
            }
            return values;
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private static void FillBuffer(Stream stream, int length)
        {
            if (stream.Read(Buffer, 0, length) < length)
                throw new EndOfStreamException($"Could not read {length} bytes.");
        }

        private static Int32 Read7BitEncodedInt32(Stream stream)
        {
            // Endianness should not matter, as this value is stored byte by byte.
            // While the highest bit is set, the integer requires another of a maximum of 5 bytes.
            int value = 0;
            for (int i = 0; i < sizeof(Int32) + 1; i++)
            {
                int readByte = stream.ReadByte();
                if (readByte == -1)
                    throw new EndOfStreamException("Incomplete 7-bit encoded integer.");
                value |= (readByte & 0b01111111) << i * 7;
                if ((readByte & 0b10000000) == 0)
                {
                    return value;
                }
            }
            throw new InvalidDataException("Invalid 7-bit encoded integer.");
        }

        private static object ReadEnum(Stream stream, Type enumType, bool strict, ByteConverter converter)
        {
            converter = converter ?? ByteConverter.System;

            Type valueType = Enum.GetUnderlyingType(enumType);
            int valueSize = Marshal.SizeOf(valueType);
            object value;

            // Read enough bytes to form an enum value.
            FillBuffer(stream, valueSize);
            if (valueType == typeof(Byte))
            {
                value = Buffer[0];
            }
            else if (valueType == typeof(SByte))
            {
                value = (sbyte)Buffer[0];
            }
            else if (valueType == typeof(Int16))
            {
                value = converter.ToInt16(Buffer);
            }
            else if (valueType == typeof(Int32))
            {
                value = converter.ToInt32(Buffer);
            }
            else if (valueType == typeof(Int64))
            {
                value = converter.ToInt64(Buffer);
            }
            else if (valueType == typeof(UInt16))
            {
                value = converter.ToUInt16(Buffer);
            }
            else if (valueType == typeof(UInt32))
            {
                value = converter.ToUInt32(Buffer);
            }
            else if (valueType == typeof(UInt64))
            {
                value = converter.ToUInt64(Buffer);
            }
            else
            {
                throw new NotImplementedException($"Unsupported enum type {valueType}.");
            }
            
            // Check if the value is defined in the enumeration, if requested.
            if (strict)
            {
                ValidateEnumValue(enumType, value);
            }
            return value;
        }

        private static object ReadObject(Stream stream, object instance, BinaryMemberAttribute attribute,
            Type type, ByteConverter converter)
        {
            if (attribute.Converter == null)
            {
                if (type == typeof(String))
                {
                    if (attribute.StringFormat == StringDataFormat.Raw)
                        return stream.ReadString(attribute.Length);
                    else
                        return stream.ReadString(attribute.StringFormat, converter: converter);
                }
                else if (type.IsEnumerable())
                {
                    throw new InvalidOperationException("Multidimensional arrays cannot be read directly.");
                }
                else if (type == typeof(Boolean))
                {
                    return stream.ReadBoolean(attribute.BooleanFormat);
                }
                else if (type == typeof(Byte))
                {
                    return stream.Read1Byte();
                }
                else if (type == typeof(DateTime))
                {
                    return stream.ReadDateTime(attribute.DateTimeFormat, converter);
                }
                else if (type == typeof(Decimal))
                {
                    return stream.ReadDecimal(converter);
                }
                else if (type == typeof(Double))
                {
                    return stream.ReadDouble(converter);
                }
                else if (type == typeof(Int16))
                {
                    return stream.ReadInt16(converter);
                }
                else if (type == typeof(Int32))
                {
                    return stream.ReadInt32(converter);
                }
                else if (type == typeof(Int64))
                {
                    return stream.ReadInt64(converter);
                }
                else if (type == typeof(SByte))
                {
                    return stream.ReadSByte();
                }
                else if (type == typeof(Single))
                {
                    return stream.ReadSingle(converter);
                }
                else if (type == typeof(UInt16))
                {
                    return stream.ReadUInt16(converter);
                }
                else if (type == typeof(UInt32))
                {
                    return stream.ReadUInt32(converter);
                }
                else if (type == typeof(UInt64))
                {
                    return stream.ReadUInt64(converter);
                }
                else if (type.IsEnum)
                {
                    return ReadEnum(stream, type, attribute.Strict, converter);
                }
                else
                {
                    if (stream.CanSeek)
                        return ReadCustomObject(stream, type, null, stream.Position, converter);
                    else
                        return ReadCustomObject(stream, type, null, -1, converter);
                }
            }
            else
            {
                // Let a binary converter do all the work.
                IBinaryConverter binaryConverter = BinaryConverterCache.GetConverter(attribute.Converter);
                return binaryConverter.Read(stream, instance, attribute, converter);
            }
        }

        private static object ReadCustomObject(Stream stream, Type type, object instance, long startOffset,
            ByteConverter converter)
        {
            TypeData typeData = TypeData.GetTypeData(type);
            instance = instance ?? typeData.GetInstance();

            // Read inherited members first if required.
            if (typeData.Attribute.Inherit && typeData.Type.BaseType != null)
            {
                ReadCustomObject(stream, typeData.Type.BaseType, instance, startOffset, converter);
            }

            // Read members.
            foreach (KeyValuePair<int, MemberData> orderedMember in typeData.OrderedMembers)
            {
                ReadMember(stream, instance, startOffset, converter, orderedMember.Value);
            }
            foreach (KeyValuePair<string, MemberData> unorderedMember in typeData.UnorderedMembers)
            {
                ReadMember(stream, instance, startOffset, converter, unorderedMember.Value);
            }

            return instance;
        }

        private static void ReadMember(Stream stream, object instance, long startOffset, ByteConverter converter,
            MemberData member)
        {
            // If possible, reposition the stream according to offset.
            if (stream.CanSeek)
            {
                if (member.Attribute.OffsetOrigin == OffsetOrigin.Begin)
                    stream.Position = startOffset + member.Attribute.Offset;
                else if (member.Attribute.Offset != 0)
                    stream.Position += member.Attribute.Offset;
            }
            else
            {
                if (member.Attribute.OffsetOrigin == OffsetOrigin.Begin || member.Attribute.Offset < 0)
                    throw new NotSupportedException("Cannot reposition the stream as it is not seekable.");
                else if (member.Attribute.Offset > 0) // Simulate moving forward by reading bytes.
                    stream.ReadBytes(member.Attribute.Offset);
            }

            // Read the value and respect settings stored in the member attribute.
            object value;
            Type elementType = member.Type.GetEnumerableElementType();
            if (elementType == null)
            {
                value = ReadObject(stream, instance, member.Attribute, member.Type, converter);
            }
            else
            {
                Array values = Array.CreateInstance(elementType, member.Attribute.Length);
                for (int i = 0; i < values.Length; i++)
                {
                    values.SetValue(ReadObject(stream, instance, member.Attribute, elementType, converter), i);
                }
                value = values;
            }

            // Set the read value.
            switch (member.MemberInfo)
            {
                case FieldInfo field:
                    field.SetValue(instance, value);
                    break;
                case PropertyInfo property:
                    property.SetValue(instance, value);
                    break;
            }
        }

        private static string ReadStringWithLength(Stream stream, int length, bool lengthInChars, Encoding encoding)
        {
            if (length == 0)
                return String.Empty;

            Decoder decoder = encoding.GetDecoder();
            StringBuilder builder = new StringBuilder(length);
            int totalBytesRead = 0;
            lock (stream)
            {
                byte[] buffer = Buffer;
                char[] charBuffer = CharBuffer;
                do
                {
                    int bufferOffset = 0;
                    int charsDecoded = 0;
                    while (charsDecoded == 0)
                    {
                        // Read raw bytes from the stream.
                        int bytesRead = stream.Read(buffer, bufferOffset++, 1);
                        if (bytesRead == 0)
                            throw new EndOfStreamException("Incomplete string data, missing requested length.");
                        totalBytesRead += bytesRead;
                        // Convert the bytes to chars and append them to the string being built.
                        charsDecoded = decoder.GetCharCount(buffer, 0, bufferOffset);
                        if (charsDecoded > 0)
                        {
                            decoder.GetChars(buffer, 0, bufferOffset, charBuffer, 0);
                            builder.Append(charBuffer, 0, charsDecoded);
                        }
                    }
                } while ((lengthInChars && builder.Length < length) || (!lengthInChars && totalBytesRead < length));
            }
            return builder.ToString();
        }

        private static string ReadStringZeroPostfix(Stream stream, Encoding encoding)
        {
            // Read byte or word values until a 0 value is found (no encoding's char surrogate should consist of 0).
            // Endianness depends on encoding, not the actual values.
            List<byte> bytes = new List<byte>();
            bool isChar = true;
            byte[] buffer = Buffer;
            lock (stream)
            {
                switch (encoding.GetByteCount("A"))
                {
                    case sizeof(Byte):
                        // Read single bytes.
                        while (isChar)
                        {
                            FillBuffer(stream, sizeof(Byte));
                            if (isChar = buffer[0] != 0)
                            {
                                bytes.Add(buffer[0]);
                            }
                        }
                        break;
                    case sizeof(Int16):
                        // Read word values of 2 bytes width.
                        while (isChar)
                        {
                            FillBuffer(stream, sizeof(Int16));
                            if (isChar = buffer[0] != 0 || buffer[1] != 0)
                            {
                                bytes.Add(buffer[0]);
                                bytes.Add(buffer[1]);
                            }
                        }
                        break;
                    default:
                        throw new NotImplementedException(
                            "Unhandled character byte count. Only 1- or 2-byte encodings are support at the moment.");
                }
            }
            // Convert to string.
            return encoding.GetString(bytes.ToArray());
        }
    }
}
