using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class with pre-defined MSBT tag map data types.
/// </summary>
public static class DataTypes
{
    #region private members
    private static IReadOnlyDictionary<string, DataType>? _cache;
    private static readonly object Lock = new();
    #endregion

    #region public properties
    /// <summary>
    /// A boolean.
    /// 1 byte long.
    /// </summary>
    [TypeName("bool", "boolean")]
    public static DataType Bool { get; } = new()
    {
        Name = "bool",
        Length = 1,
        Serialize = (str, _, _) => "true".Equals(str, StringComparison.OrdinalIgnoreCase) ? [1] : [0],
        Deserialize = (bytes, offset, _, _) => (bytes[offset] == 1 ? "true" : "false", 1)
    };

    /// <summary>
    /// A single byte.
    /// 1 byte long.
    /// </summary>
    [TypeName("u8", "uint8", "byte")]
    public static DataType Byte { get; } = new()
    {
        Name = "u8",
        Length = 1,
        Serialize = (str, _, _) => [byte.Parse(str)],
        Deserialize = (bytes, offset, _, _) => (bytes[offset].ToString(), 1)
    };

    /// <summary>
    /// A signed single byte.
    /// 1 byte long.
    /// </summary>
    [TypeName("s8", "i8", "int8", "sbyte")]
    public static DataType SByte { get; } = new()
    {
        Name = "s8",
        Length = 1,
        Serialize = (str, _, _) => [(byte) sbyte.Parse(str)],
        Deserialize = (bytes, offset, _, _) => (((sbyte) bytes[offset]).ToString(), 1)
    };

    /// <summary>
    /// A signed 16bit integer.
    /// 2 bytes long.
    /// </summary>
    [TypeName("s16", "i16", "int16", "short")]
    public static DataType Int16 { get; } = new()
    {
        Name = "s16",
        Length = 2,
        Serialize = (str, isBigEndian, _) => GetBytes(BitConverter.GetBytes(short.Parse(str)), isBigEndian),
        Deserialize = (bytes, offset, isBigEndian, _) => (BitConverter.ToInt16(GetBytes(bytes, offset, 2, isBigEndian)).ToString(), 2)
    };

    /// <summary>
    /// An unsigned 16bit integer.
    /// 2 bytes long.
    /// </summary>
    [TypeName("u16", "uint16", "ushort")]
    public static DataType UInt16 { get; } = new()
    {
        Name = "u16",
        Length = 2,
        Serialize = (str, isBigEndian, _) => GetBytes(BitConverter.GetBytes(ushort.Parse(str)), isBigEndian),
        Deserialize = (bytes, offset, isBigEndian, _) => (BitConverter.ToUInt16(GetBytes(bytes, offset, 2, isBigEndian)).ToString(), 2)
    };

    /// <summary>
    /// A signed 32bit integer.
    /// 4 bytes long.
    /// </summary>
    [TypeName("s32", "i32", "int32", "int")]
    public static DataType Int32 { get; } = new()
    {
        Name = "s32",
        Length = 4,
        Serialize = (str, isBigEndian, _) => GetBytes(BitConverter.GetBytes(int.Parse(str)), isBigEndian),
        Deserialize = (bytes, offset, isBigEndian, _) => (BitConverter.ToInt32(GetBytes(bytes, offset, 4, isBigEndian)).ToString(), 4)
    };

    /// <summary>
    /// An unsigned 32bit integer.
    /// 4 bytes long.
    /// </summary>
    [TypeName("u32", "uint32", "uint")]
    public static DataType UInt32 { get; } = new()
    {
        Name = "u32",
        Length = 4,
        Serialize = (str, isBigEndian, _) => GetBytes(BitConverter.GetBytes(uint.Parse(str)), isBigEndian),
        Deserialize = (bytes, offset, isBigEndian, _) => (BitConverter.ToUInt32(GetBytes(bytes, offset, 4, isBigEndian)).ToString(), 4)
    };

    /// <summary>
    /// A signed 64bit integer.
    /// 8 bytes long.
    /// </summary>
    [TypeName("s64", "i64", "int64", "long")]
    public static DataType Int64 { get; } = new()
    {
        Name = "s64",
        Length = 8,
        Serialize = (str, isBigEndian, _) => GetBytes(BitConverter.GetBytes(long.Parse(str)), isBigEndian),
        Deserialize = (bytes, offset, isBigEndian, _) => (BitConverter.ToInt64(GetBytes(bytes, offset, 8, isBigEndian)).ToString(), 8)
    };

    /// <summary>
    /// An unsigned 64bit integer.
    /// 8 bytes long.
    /// </summary>
    [TypeName("u64", "uint64", "ulong")]
    public static DataType UInt64 { get; } = new()
    {
        Name = "u64",
        Length = 8,
        Serialize = (str, isBigEndian, _) => GetBytes(BitConverter.GetBytes(ulong.Parse(str)), isBigEndian),
        Deserialize = (bytes, offset, isBigEndian, _) => (BitConverter.ToInt64(GetBytes(bytes, offset, 8, isBigEndian)).ToString(), 8)
    };

    /// <summary>
    /// A 32bit floating point number.
    /// 4 bytes long.
    /// </summary>
    [TypeName("f32", "single", "float")]
    public static DataType Single { get; } = new()
    {
        Name = "f32",
        Length = 4,
        Serialize = (str, isBigEndian, _) => GetBytes(BitConverter.GetBytes(float.Parse(str, CultureInfo.InvariantCulture)), isBigEndian),
        Deserialize = (bytes, offset, isBigEndian, _) => (BitConverter.ToSingle(GetBytes(bytes, offset, 4, isBigEndian)).ToString(CultureInfo.InvariantCulture), 4)
    };

    /// <summary>
    /// A 64bit floating point number.
    /// 8 bytes long.
    /// </summary>
    [TypeName("f64", "double")]
    public static DataType Double { get; } = new()
    {
        Name = "f64",
        Length = 8,
        Serialize = (str, isBigEndian, _) => GetBytes(BitConverter.GetBytes(double.Parse(str, CultureInfo.InvariantCulture)), isBigEndian),
        Deserialize = (bytes, offset, isBigEndian, _) => (BitConverter.ToDouble(GetBytes(bytes, offset, 8, isBigEndian)).ToString(CultureInfo.InvariantCulture), 8)
    };

    /// <summary>
    /// A MSBT string.
    /// The first 2 bytes are the length of the string.
    /// 2+n bytes long.
    /// </summary>
    [TypeName("str", "string")]
    public static DataType String { get; } = new()
    {
        Name = "str",
        Length = 2,
        Serialize = (str, isBigEndian, encoding) =>
        {
            var strBytes = encoding.GetBytes(str);
            var length = BitConverter.GetBytes((ushort) strBytes.Length);
            if (isBigEndian == BitConverter.IsLittleEndian) Array.Reverse(length);

            var bytes = new byte[length.Length + strBytes.Length];
            Buffer.BlockCopy(length, 0, bytes, 0, length.Length);
            Buffer.BlockCopy(strBytes, 0, bytes, length.Length, strBytes.Length);

            return bytes;
        },
        Deserialize = (bytes, offset, isBigEndian, encoding) =>
        {
            var lengthBytes = bytes[offset..(offset + 2)];
            if (isBigEndian == BitConverter.IsLittleEndian) Array.Reverse(lengthBytes);

            var length = BitConverter.ToUInt16(lengthBytes);
            return (encoding.GetString(bytes, offset + 2, length), length + 2);
        }
    };

    /// <summary>
    /// A null-terminated string.
    /// n+1 bytes long.
    /// </summary>
    [TypeName("nstr", "0str", "nullStr", "nstring", "0string", "nullString")]
    public static DataType NullString { get; } = new()
    {
        Name = "nstr",
        Length = 1,
        Serialize = (str, _, encoding) => encoding.GetBytes(str + '\0'),
        Deserialize = (bytes, offset, _, encoding) => ReadTerminatedString(encoding, bytes, offset)
    };

    /// <summary>
    /// A hex value encoded as 0x-prefixed string.
    /// n bytes long.
    /// </summary>
    [TypeName("hex", "hexStr", "hexString")]
    public static DataType HexString { get; } = new()
    {
        Name = "hex",
        Length = 0,
        Serialize = (str, _, _) =>
        {
            var skip = str.StartsWith("0x") ? 2 : 0;

            #if NET5_0_OR_GREATER
            return Convert.FromHexString(str.AsSpan(skip));
            #else
            var bytes = new byte[(str.Length - skip) / 2];
            for (var i = 0; i < bytes.Length; ++i)
            {
                bytes[i] = byte.Parse(str.AsSpan(skip + i * 2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }
            return bytes;
            #endif
        },
        Deserialize = (bytes, offset, _, _) =>
        {
            #if NET5_0_OR_GREATER
            return (Convert.ToHexString(bytes, offset, bytes.Length), bytes.Length - offset);
            #else
            return (BitConverter.ToString(bytes, offset).Replace("-", string.Empty), bytes.Length - offset);
            #endif
        }
    };
    #endregion

    #region public methods
    /// <summary>
    /// Gets a map of all defined data types with their specified names.
    /// </summary>
    /// <returns>A collection of defined data types.</returns>
    public static IReadOnlyDictionary<string, DataType> GetTypeList()
    {
        lock (Lock)
        {
            if (_cache is not null) return _cache;

            var cache = new Dictionary<string, DataType>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var field in typeof(DataTypes).GetProperties(BindingFlags.Public | BindingFlags.Static))
            {
                var type = (DataType) field.GetValue(null)!;
                var attr = field.GetCustomAttribute<TypeNameAttribute>();
                if (attr is null) cache.TryAdd(field.Name, type);
                else foreach (var name in attr.Names) cache.TryAdd(name, type);
            }

            _cache = cache;
            return _cache;
        }
    }

    /// <summary>
    /// Gets a dynamic padding data type.
    /// </summary>
    /// <param name="hexValue">The bytes used for padding as hex string.</param>
    /// <returns>A new padding data type instance.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static PaddingDataType GetPadding(string hexValue)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hexValue, nameof(hexValue));
        #else
        if (hexValue is null) throw new ArgumentNullException(nameof(hexValue));
        #endif

        return new PaddingDataType(HexString.Serialize(hexValue, false, Encoding.UTF8));
    }
    #endregion

    #region private methods
    private static (string, int) ReadTerminatedString(Encoding encoding, byte[] data, int offset)
    {
        if (offset >= data.Length) return (string.Empty, 0);

        var bytes = new List<byte>(256);
        var nullByteLength = encoding.GetByteCount("\0");

        var nullCount = 0;
        do
        {
            var value = data[offset];
            nullCount = value == 0x00 ? nullCount + 1 : 0;
            bytes.Add(value);
            ++offset;
        } while (offset < data.Length && nullCount < nullByteLength);

        var count = bytes.Count;

        //append enough null bytes to ensure we have a full null-byte to trim
        for (var i = 0; i < nullByteLength; ++i) bytes.Add(0x00);

        //trim invalid characters at the end
        var str = encoding.GetString([..bytes]);
        return (str[..str.IndexOf('\0')], count);
    }

    private static byte[] GetBytes(byte[] data, bool isBigEndian) => GetBytes(data, 0, data.Length, isBigEndian);

    private static byte[] GetBytes(byte[] data, int offset, int length, bool isBigEndian)
    {
        var bytes = data[offset..(offset + length)];
        if (isBigEndian == BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }
    #endregion

    #region helper class
    [AttributeUsage(AttributeTargets.Property)]
    private class TypeNameAttribute(params string[] names) : Attribute
    {
        public IEnumerable<string> Names { get; } = names;
    }
    #endregion
}