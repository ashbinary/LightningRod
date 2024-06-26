using System;
using System.Collections;
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
        /// Writes a <see cref="Boolean"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The <see cref="BooleanDataFormat"/> format in which the data is stored.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, Boolean value,
            BooleanDataFormat format = BooleanDataFormat.Byte, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            byte[] buffer;
            switch (format)
            {
                case BooleanDataFormat.Byte:
                    stream.WriteByte((Byte)(value ? 1 : 0));
                    break;
                case BooleanDataFormat.Word:
                    buffer = Buffer;
                    converter.GetBytes((Int16)(value ? 1 : 0), buffer, 0);
                    stream.Write(Buffer, 0, sizeof(Int16));
                    break;
                case BooleanDataFormat.Dword:
                    buffer = Buffer;
                    converter.GetBytes(value ? 1 : 0, buffer, 0);
                    stream.Write(Buffer, 0, sizeof(Int32));
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(BooleanDataFormat)}.", nameof(format));
            }
        }

        /// <summary>
        /// Writes an array of <see cref="Boolean"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="format">The <see cref="BooleanDataFormat"/> format in which the data is stored.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<Boolean> values,
            BooleanDataFormat format = BooleanDataFormat.Byte, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                byte[] buffer;
                switch (format)
                {
                    case BooleanDataFormat.Byte:
                        foreach (var value in values)
                        {
                            stream.WriteByte((Byte)(value ? 1 : 0));
                        }
                        break;
                    case BooleanDataFormat.Word:
                        buffer = Buffer;
                        foreach (var value in values)
                        {
                            converter.GetBytes((Int16)(value ? 1 : 0), buffer, 0);
                            stream.Write(Buffer, 0, sizeof(Int16));
                        }
                        break;
                    case BooleanDataFormat.Dword:
                        buffer = Buffer;
                        foreach (var value in values)
                        {
                            converter.GetBytes(value ? 1 : 0, buffer, 0);
                            stream.Write(Buffer, 0, sizeof(Int32));
                        }
                        break;
                    default:
                        throw new ArgumentException($"Invalid {nameof(BooleanDataFormat)}.", nameof(format));
                }
            }
        }

        // ---- Byte ----

        /// <summary>
        /// Writes a <see cref="Byte"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this Stream stream, Byte value)
        {
            stream.WriteByte(value);
        }

        /// <summary>
        /// Writes an array of <see cref="Byte"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        public static void Write(this Stream stream, IEnumerable<Byte> values)
        {
            lock (stream)
            {
                foreach (var value in values)
                {
                    stream.WriteByte(value);
                }
            }
        }

        // ---- DateTime ----

        /// <summary>
        /// Writes a <see cref="DateTime"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The <see cref="DateTimeDataFormat"/> format in which the data is stored.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, DateTime value,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            switch (format)
            {
                case DateTimeDataFormat.NetTicks:
                    Write(stream, value.Ticks, converter);
                    break;
                case DateTimeDataFormat.CTime:
                    Write(stream, (uint)(new DateTime(1970, 1, 1) - value).TotalSeconds, converter);
                    break;
                case DateTimeDataFormat.CTime64:
                    Write(stream, (ulong)(new DateTime(1970, 1, 1) - value).TotalSeconds, converter);
                    break;
                default:
                    throw new ArgumentException($"Invalid {nameof(DateTimeDataFormat)}.", nameof(format));
            }
        }

        /// <summary>
        /// Writes an array of <see cref="DateTime"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="format">The <see cref="DateTimeDataFormat"/> format in which the data is stored.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<DateTime> values,
            DateTimeDataFormat format = DateTimeDataFormat.NetTicks, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
        }

        // ---- Decimal ----

        /// <summary>
        /// Writes a <see cref="Decimal"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, Decimal value, ByteConverter converter = null)
        {
            byte[] buffer = Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, sizeof(Decimal));
        }

        /// <summary>
        /// Writes an array of <see cref="Decimal"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<Decimal> values, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = Buffer;
                foreach (var value in values)
                {
                    converter.GetBytes(value, buffer, 0);
                    stream.Write(buffer, 0, sizeof(Decimal));
                }
            }
        }

        // ---- Double ----

        /// <summary>
        /// Writes a <see cref="Double"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, Double value, ByteConverter converter = null)
        {
            byte[] buffer = Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, sizeof(Double));
        }

        /// <summary>
        /// Writes an array of <see cref="Double"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<Double> values, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = Buffer;
                foreach (var value in values)
                {
                    converter.GetBytes(value, buffer, 0);
                    stream.Write(buffer, 0, sizeof(Double));
                }
            }
        }

        // ---- Enum ----

        /// <summary>
        /// Writes an <see cref="Enum"/> value of type <typeparamref name="T"/> to the <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="strict"><c>true</c> to raise an <see cref="ArgumentOutOfRangeException"/> if the value is not
        /// defined in the enum type.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void WriteEnum<T>(this Stream stream, T value, bool strict = false,
            ByteConverter converter = null)
            where T : struct, IComparable, IFormattable
            => WriteEnum(stream, typeof(T), value, strict, converter);

        /// <summary>
        /// Writes an array of <see cref="Enum"/> values of type <typeparamref name="T"/> to the
        /// <paramref name="stream"/>.
        /// </summary>
        /// <typeparam name="T">The type of the enum.</typeparam>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="strict"><c>true</c> to raise an <see cref="ArgumentOutOfRangeException"/> if the value is not
        /// defined in the enum type.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void WriteEnums<T>(this Stream stream, IEnumerable<T> values, bool strict = false,
            ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            Type enumType = typeof(T);
            lock (stream)
            {
                foreach (var value in values)
                {
                    WriteEnum(stream, enumType, value, strict, converter);
                }
            }
        }

        // ---- Int16 ----

        /// <summary>
        /// Writes an <see cref="Int16"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, Int16 value, ByteConverter converter = null)
        {
            byte[] buffer = Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, sizeof(Int16));
        }

        /// <summary>
        /// Writes an array of <see cref="Int16"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<Int16> values, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = Buffer;
                foreach (var value in values)
                {
                    converter.GetBytes(value, buffer, 0);
                    stream.Write(buffer, 0, sizeof(Int16));
                }
            }
        }

        // ---- Int32 ----

        /// <summary>
        /// Writes an <see cref="Int32"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, Int32 value, ByteConverter converter = null)
        {
            byte[] buffer = Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, sizeof(Int32));
        }

        /// <summary>
        /// Writes an array of <see cref="Int32"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<Int32> values, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = Buffer;
                foreach (var value in values)
                {
                    converter.GetBytes(value, buffer, 0);
                    stream.Write(buffer, 0, sizeof(Int32));
                }
            }
        }

        // ---- Int64 ----

        /// <summary>
        /// Writes an <see cref="Int64"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, Int64 value, ByteConverter converter = null)
        {
            byte[] buffer = Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, sizeof(Int64));
        }

        /// <summary>
        /// Writes an array of <see cref="Int64"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<Int64> values, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = Buffer;
                foreach (var value in values)
                {
                    converter.GetBytes(value, buffer, 0);
                    stream.Write(buffer, 0, sizeof(Int64));
                }
            }
        }

        // ---- Object ----

        /// <summary>
        /// Writes an object or enumerable of objects to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The object or enumerable of objects to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void WriteObject(this Stream stream, object value, ByteConverter converter = null)
            => WriteObject(stream, null, BinaryMemberAttribute.Default, value.GetType(), value, converter);

        // ---- SByte ----

        /// <summary>
        /// Writes an <see cref="SByte"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        public static void Write(this Stream stream, SByte value)
        {
            byte[] buffer = Buffer;
            buffer[0] = (byte)value;
            stream.Write(buffer, 0, sizeof(SByte));
        }

        /// <summary>
        /// Writes an array of <see cref="SByte"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        public static void Write(this Stream stream, IEnumerable<SByte> values)
        {
            lock (stream)
            {
                byte[] buffer = Buffer;
                foreach (var value in values)
                {
                    buffer[0] = (byte)value;
                    stream.Write(buffer, 0, sizeof(SByte));
                }
            }
        }

        // ---- Single ----

        /// <summary>
        /// Writes a <see cref="Single"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, Single value, ByteConverter converter = null)
        {
            byte[] buffer = Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, sizeof(Single));
        }

        /// <summary>
        /// Writes an array of <see cref="Single"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<Single> values, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = Buffer;
                foreach (var value in values)
                {
                    converter.GetBytes(value, buffer, 0);
                    stream.Write(buffer, 0, sizeof(Single));
                }
            }
        }

        // ---- String ----

        /// <summary>
        /// Writes a <see cref="String"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="format">The <see cref="StringDataFormat"/> format determining how the length of the string is
        /// stored.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to parse the bytes with, or <c>null</c> to use
        /// <see cref="Encoding.UTF8"/>.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, String value,
            StringDataFormat format = StringDataFormat.DynamicByteCount, Encoding encoding = null,
            ByteConverter converter = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            converter = converter ?? ByteConverter.System;
            byte[] textBuffer = encoding.GetBytes(value);
            lock (stream)
            {
                switch (format)
                {
                    case StringDataFormat.DynamicByteCount:
                        Write7BitEncodedInt(stream, textBuffer.Length);
                        stream.Write(textBuffer, 0, textBuffer.Length);
                        break;
                    case StringDataFormat.ByteCharCount:
                        stream.WriteByte((byte)value.Length);
                        stream.Write(textBuffer, 0, textBuffer.Length);
                        break;
                    case StringDataFormat.Int16CharCount:
                        converter.GetBytes((Int16)value.Length, Buffer, 0);
                        stream.Write(Buffer, 0, sizeof(Int16));
                        stream.Write(textBuffer, 0, textBuffer.Length);
                        break;
                    case StringDataFormat.Int32CharCount:
                        converter.GetBytes(value.Length, Buffer, 0);
                        stream.Write(Buffer, 0, sizeof(Int32));
                        stream.Write(textBuffer, 0, textBuffer.Length);
                        break;
                    case StringDataFormat.ZeroTerminated:
                        stream.Write(textBuffer, 0, textBuffer.Length);
                        switch (encoding.GetByteCount("A"))
                        {
                            case sizeof(Byte):
                                stream.WriteByte(0);
                                break;
                            case sizeof(Int16):
                                stream.WriteByte(0);
                                stream.WriteByte(0);
                                break;
                        }
                        break;
                    case StringDataFormat.Raw:
                        stream.Write(textBuffer, 0, textBuffer.Length);
                        break;
                    default:
                        throw new ArgumentException($"Invalid {nameof(StringDataFormat)}.", nameof(format));
                }
            }
        }

        /// <summary>
        /// Writes an array of <see cref="String"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="format">The <see cref="StringDataFormat"/> format determining how the length of the strings is
        /// stored.</param>
        /// <param name="encoding">The <see cref="Encoding"/> to parse the bytes with, or <c>null</c> to use
        /// <see cref="Encoding.UTF8"/>.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<String> values,
            StringDataFormat format = StringDataFormat.DynamicByteCount, Encoding encoding = null, 
            ByteConverter converter = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                foreach (var value in values)
                {
                    Write(stream, value, format, encoding, converter);
                }
            }
        }

        // ---- UInt16 ----

        /// <summary>
        /// Writes an <see cref="UInt16"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, UInt16 value, ByteConverter converter = null)
        {
            byte[] buffer = Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, sizeof(UInt16));
        }

        /// <summary>
        /// Writes an array of <see cref="UInt16"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<UInt16> values, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = Buffer;
                foreach (var value in values)
                {
                    converter.GetBytes(value, buffer, 0);
                    stream.Write(buffer, 0, sizeof(UInt16));
                }
            }
        }
        
        // ---- UInt32 ----

        /// <summary>
        /// Writes a <see cref="UInt32"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, UInt32 value, ByteConverter converter = null)
        {
            byte[] buffer = Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, sizeof(UInt32));
        }

        /// <summary>
        /// Writes an array of <see cref="UInt32"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<UInt32> values, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = Buffer;
                foreach (var value in values)
                {
                    converter.GetBytes(value, buffer, 0);
                    stream.Write(buffer, 0, sizeof(UInt32));
                }
            }
        }

        // ---- UInt64 ----

        /// <summary>
        /// Writes a <see cref="UInt64"/> value to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, UInt64 value, ByteConverter converter = null)
        {
            byte[] buffer = Buffer;
            (converter ?? ByteConverter.System).GetBytes(value, buffer, 0);
            stream.Write(buffer, 0, sizeof(UInt64));
        }

        /// <summary>
        /// Writes an array of <see cref="UInt64"/> values to the <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="values">The values to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multibyte data.</param>
        public static void Write(this Stream stream, IEnumerable<UInt64> values, ByteConverter converter = null)
        {
            converter = converter ?? ByteConverter.System;
            lock (stream)
            {
                byte[] buffer = Buffer;
                foreach (var value in values)
                {
                    converter.GetBytes(value, buffer, 0);
                    stream.Write(buffer, 0, sizeof(UInt64));
                }
            }
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private static void Write7BitEncodedInt(Stream stream, int value)
        {
            // The highest bit determines whether to continue writing more bytes to form the Int32 value.
            while (value >= 0b10000000)
            {
                stream.WriteByte((byte)(value | 0b10000000));
                value >>= 7;
            }
            stream.WriteByte((byte)value);
        }

        private static void WriteEnum(Stream stream, Type enumType, object value, bool strict, ByteConverter converter)
        {
            converter = converter ?? ByteConverter.System;
            Type valueType = Enum.GetUnderlyingType(enumType);

            // Write the enum value.
            byte[] buffer = Buffer;
            if (valueType == typeof(Byte))
            {
                Buffer[0] = (byte)value;
            }
            else if (valueType == typeof(SByte))
            {
                Buffer[0] = (byte)(sbyte)value;
            }
            else if (valueType == typeof(Int16))
            {
                converter.GetBytes((Int16)value, buffer, 0);
            }
            else if (valueType == typeof(Int32))
            {
                converter.GetBytes((Int32)value, buffer, 0);
            }
            else if (valueType == typeof(Int64))
            {
                converter.GetBytes((Int64)value, buffer, 0);
            }
            else if (valueType == typeof(UInt16))
            {
                converter.GetBytes((UInt16)value, buffer, 0);
            }
            else if (valueType == typeof(UInt32))
            {
                converter.GetBytes((UInt32)value, buffer, 0);
            }
            else if (valueType == typeof(UInt64))
            {
                converter.GetBytes((UInt64)value, buffer, 0);
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
            stream.Write(buffer, 0, Marshal.SizeOf(valueType));
        }

        private static void WriteObject(Stream stream, object instance, BinaryMemberAttribute attribute, Type type,
            object value, ByteConverter converter)
        {
            converter = converter ?? ByteConverter.System;

            if (attribute.Converter == null)
            {
                if (value == null)
                {
                    return;
                }
                else if (type == typeof(String))
                {
                    Write(stream, (String)value, attribute.StringFormat, converter: converter);
                }
                else if (type.TryGetEnumerableElementType(out Type elementType))
                {
                    foreach (object element in (IEnumerable)value)
                    {
                        WriteObject(stream, null, BinaryMemberAttribute.Default, elementType, element, converter);
                    }
                }
                else if (type == typeof(Boolean))
                {
                    Write(stream, (Boolean)value, attribute.BooleanFormat, converter);
                }
                else if (type == typeof(Byte))
                {
                    Write(stream, (Byte)value);
                }
                else if (type == typeof(DateTime))
                {
                    Write(stream, (DateTime)value, attribute.DateTimeFormat, converter);
                }
                else if (type == typeof(Decimal))
                {
                    Write(stream, (Decimal)value, converter);
                }
                else if (type == typeof(Double))
                {
                    Write(stream, (Double)value, converter);
                }
                else if (type == typeof(Int16))
                {
                    Write(stream, (Int16)value, converter);
                }
                else if (type == typeof(Int32))
                {
                    Write(stream, (Int32)value, converter);
                }
                else if (type == typeof(Int64))
                {
                    Write(stream, (Int64)value, converter);
                }
                else if (type == typeof(SByte))
                {
                    Write(stream, (SByte)value);
                }
                else if (type == typeof(Single))
                {
                    Write(stream, (Single)value, converter);
                }
                else if (type == typeof(UInt16))
                {
                    Write(stream, (UInt16)value, converter);
                }
                else if (type == typeof(UInt32))
                {
                    Write(stream, (UInt32)value, converter);
                }
                else if (type == typeof(UInt64))
                {
                    Write(stream, (UInt64)value, converter);
                }
                else if (type.IsEnum)
                {
                    WriteEnum(stream, type, value, attribute.Strict, converter);
                }
                else
                {
                    if (stream.CanSeek)
                        WriteCustomObject(stream, type, value, stream.Position, converter);
                    else
                        WriteCustomObject(stream, type, value, -1, converter);
                }
            }
            else
            {
                // Let a binary converter do all the work.
                IBinaryConverter binaryConverter = BinaryConverterCache.GetConverter(attribute.Converter);
                binaryConverter.Write(stream, instance, attribute, value, converter);
            }
        }

        private static void WriteCustomObject(Stream stream, Type type, object instance, long startOffset,
            ByteConverter converter)
        {
            TypeData typeData = TypeData.GetTypeData(type);

            // Write inherited members first if required.
            if (typeData.Attribute.Inherit && typeData.Type.BaseType != null)
            {
                WriteCustomObject(stream, typeData.Type.BaseType, instance, startOffset, converter);
            }

            // Write members.
            foreach (KeyValuePair<int, MemberData> orderedMember in typeData.OrderedMembers)
            {
                WriteMember(stream, instance, startOffset, converter, orderedMember.Value);
            }
            foreach (KeyValuePair<string, MemberData> unorderedMember in typeData.UnorderedMembers)
            {
                WriteMember(stream, instance, startOffset, converter, unorderedMember.Value);
            }
        }

        private static void WriteMember(Stream stream, object instance, long startOffset, ByteConverter converter,
            MemberData member)
        {
            // If possible, reposition the stream according to offset.
            if (stream.CanSeek)
            {
                if (member.Attribute.OffsetOrigin == OffsetOrigin.Begin)
                    stream.Position = startOffset + member.Attribute.Offset;
                else
                    stream.Position += member.Attribute.Offset;
            }
            else
            {
                if (member.Attribute.OffsetOrigin == OffsetOrigin.Begin || member.Attribute.Offset < 0)
                    throw new NotSupportedException("Cannot reposition the stream as it is not seekable.");
                else if (member.Attribute.Offset > 0) // Simulate moving forward by writing bytes.
                    stream.Write(new byte[member.Attribute.Offset]);
            }

            // Get the value to write.
            object value;
            switch (member.MemberInfo)
            {
                case FieldInfo field:
                    value = field.GetValue(instance);
                    break;
                case PropertyInfo property:
                    value = property.GetValue(instance);
                    break;
                default:
                    throw new InvalidOperationException($"Tried to write an invalid member {member.MemberInfo}.");
            }

            // Write the value and respect settings stored in the member attribute.
            Type elementType = member.Type.GetEnumerableElementType();
            if (elementType == null)
            {
                WriteObject(stream, instance, member.Attribute, member.Type, value, converter);
            }
            else
            {
                foreach (object element in (IEnumerable)value)
                {
                    WriteObject(stream, instance, member.Attribute, elementType, element, converter);
                }
            }
        }
    }
}
