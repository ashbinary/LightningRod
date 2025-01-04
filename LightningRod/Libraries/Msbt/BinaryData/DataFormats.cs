using System;

namespace OatmealDome.BinaryData
{
    /// <summary>
    /// Represents the set of formats of binary boolean encodings.
    /// </summary>
    public enum BooleanDataFormat
    {
        /// <summary>
        /// The boolean is stored in 1 byte and is <c>true</c> when the value is not 0. This is the .NET default.
        /// </summary>
        Byte,

        /// <summary>
        /// The boolean is stored in 2 bytes and is <c>true</c> when the value is not 0.
        /// </summary>
        Word,

        /// <summary>
        /// The boolean is stored in 4 bytes and is <c>true</c> when the value is not 0.
        /// </summary>
        Dword,
    }

    /// <summary>
    /// Represents the set of formats of binary date and time encodings.
    /// </summary>
    public enum DateTimeDataFormat
    {
        /// <summary>
        /// The <see cref="System.DateTime"/> is stored as the ticks of a .NET <see cref="System.DateTime"/> instance.
        /// </summary>
        NetTicks,

        /// <summary>
        /// The <see cref="System.DateTime"/> has the 32-bit time_t format of the C library.
        /// This is a <see cref="UInt32"/> which can store the seconds from 1970-01-01 until approx. 2106-02-07.
        /// </summary>
        CTime,

        /// <summary>
        /// The <see cref="System.DateTime"/> has the 64-bit time_t format of the C library.
        /// This is an <see cref="Int64"/> which can store the seconds from 1970-01-01 until approx.
        /// 292277026596-12-04.
        /// </summary>
        CTime64,
    }

    /// <summary>
    /// Represents formats of binary string encodings.
    /// </summary>
    public enum StringDataFormat
    {
        /// <summary>
        /// The string has a prefix of a 7-bit encoded integer of variable size determining the number of bytes out of
        /// which the string consists and no postfix. This is the .NET <see cref="System.IO.BinaryReader"/> and
        /// <see cref="System.IO.BinaryWriter"/> default.
        /// </summary>
        DynamicByteCount,

        /// <summary>
        /// The string has a prefix of 1 byte determining the number of chars out of which the string consists and no
        /// postfix.
        /// </summary>
        ByteCharCount,

        /// <summary>
        /// The string has a prefix of 2 bytes determining the number of chars out of which the string consists and no
        /// postfix.
        /// </summary>
        Int16CharCount,

        /// <summary>
        /// The string has a prefix of 4 bytes determining number of chars out of which the string consists and no
        /// postfix.
        /// </summary>
        Int32CharCount,

        /// <summary>
        /// The string has no prefix and is terminated with a 0 value. The size of this value depends on the encoding.
        /// </summary>
        ZeroTerminated,

        /// <summary>
        /// The string has neither prefix nor postfix. This format is only valid for writing strings. For reading
        /// strings, the length has to be specified manually.
        /// </summary>
        Raw,
    }
}
