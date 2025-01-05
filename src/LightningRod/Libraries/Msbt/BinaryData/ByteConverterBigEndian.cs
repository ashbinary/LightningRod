using System;
using System.Security;

namespace OatmealDome.BinaryData
{
    /// <summary>
    /// Represents a <see cref="ByteConverter"/> which handles little endianness.
    /// </summary>
    [SecuritySafeCritical]
    public sealed class ByteConverterBigEndian : ByteConverter
    {
        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets the <see cref="BinaryData.ByteOrder"/> in which data is stored as converted by this instance.
        /// </summary>
        public override ByteOrder ByteOrder => ByteOrder.BigEndian;

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Stores the specified <see cref="Double"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(Double value, byte[] buffer, int startIndex = 0)
        {
            UInt64 raw = *(UInt64*)&value;
            buffer[startIndex] = (byte)(raw >> 56);
            buffer[startIndex + 1] = (byte)(raw >> 48);
            buffer[startIndex + 2] = (byte)(raw >> 40);
            buffer[startIndex + 3] = (byte)(raw >> 32);
            buffer[startIndex + 4] = (byte)(raw >> 24);
            buffer[startIndex + 5] = (byte)(raw >> 16);
            buffer[startIndex + 6] = (byte)(raw >> 8);
            buffer[startIndex + 7] = (byte)raw;
        }

        /// <summary>
        /// Stores the specified <see cref="Int16"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public override void GetBytes(Int16 value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)(value >> 8);
            buffer[startIndex + 1] = (byte)value;
        }

        /// <summary>
        /// Stores the specified <see cref="Int32"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public override void GetBytes(Int32 value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)(value >> 24);
            buffer[startIndex + 1] = (byte)(value >> 16);
            buffer[startIndex + 2] = (byte)(value >> 8);
            buffer[startIndex + 3] = (byte)value;
        }

        /// <summary>
        /// Stores the specified <see cref="Int64"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public override void GetBytes(Int64 value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)(value >> 56);
            buffer[startIndex + 1] = (byte)(value >> 48);
            buffer[startIndex + 2] = (byte)(value >> 40);
            buffer[startIndex + 3] = (byte)(value >> 32);
            buffer[startIndex + 4] = (byte)(value >> 24);
            buffer[startIndex + 5] = (byte)(value >> 16);
            buffer[startIndex + 6] = (byte)(value >> 8);
            buffer[startIndex + 7] = (byte)value;
        }

        /// <summary>
        /// Stores the specified <see cref="Single"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        [SecuritySafeCritical]
        public override unsafe void GetBytes(Single value, byte[] buffer, int startIndex = 0)
        {
            UInt32 raw = *(UInt32*)&value;
            buffer[startIndex] = (byte)(raw >> 24);
            buffer[startIndex + 1] = (byte)(raw >> 16);
            buffer[startIndex + 2] = (byte)(raw >> 8);
            buffer[startIndex + 3] = (byte)raw;
        }

        /// <summary>
        /// Stores the specified <see cref="UInt16"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public override void GetBytes(UInt16 value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)(value >> 8);
            buffer[startIndex + 1] = (byte)value;
        }

        /// <summary>
        /// Stores the specified <see cref="UInt32"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public override void GetBytes(UInt32 value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)(value >> 24);
            buffer[startIndex + 1] = (byte)(value >> 16);
            buffer[startIndex + 2] = (byte)(value >> 8);
            buffer[startIndex + 3] = (byte)value;
        }

        /// <summary>
        /// Stores the specified <see cref="UInt64"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public override void GetBytes(UInt64 value, byte[] buffer, int startIndex = 0)
        {
            buffer[startIndex] = (byte)(value >> 56);
            buffer[startIndex + 1] = (byte)(value >> 48);
            buffer[startIndex + 2] = (byte)(value >> 40);
            buffer[startIndex + 3] = (byte)(value >> 32);
            buffer[startIndex + 4] = (byte)(value >> 24);
            buffer[startIndex + 5] = (byte)(value >> 16);
            buffer[startIndex + 6] = (byte)(value >> 8);
            buffer[startIndex + 7] = (byte)value;
        }

        /// <summary>
        /// Returns an <see cref="Double"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        [SecuritySafeCritical]
        public override unsafe Double ToDouble(byte[] buffer, int startIndex = 0)
        {
            Int64 raw =
                (long)buffer[startIndex] << 56
                | (long)buffer[startIndex + 1] << 48
                | (long)buffer[startIndex + 2] << 40
                | (long)buffer[startIndex + 3] << 32
                | (long)buffer[startIndex + 4] << 24
                | (long)buffer[startIndex + 5] << 16
                | (long)buffer[startIndex + 6] << 8
                | buffer[startIndex + 7];
            return *(Double*)&raw;
        }

        /// <summary>
        /// Returns an <see cref="Int16"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public override Int16 ToInt16(byte[] buffer, int startIndex = 0)
        {
            return (Int16)(buffer[startIndex] << 8 | buffer[startIndex + 1]);
        }

        /// <summary>
        /// Returns an <see cref="Int32"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public override Int32 ToInt32(byte[] buffer, int startIndex = 0)
        {
            return buffer[startIndex] << 24
                | buffer[startIndex + 1] << 16
                | buffer[startIndex + 2] << 8
                | buffer[startIndex + 3];
        }

        /// <summary>
        /// Returns an <see cref="Int64"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public override Int64 ToInt64(byte[] buffer, int startIndex = 0)
        {
            return (long)buffer[startIndex] << 56
                | (long)buffer[startIndex + 1] << 48
                | (long)buffer[startIndex + 2] << 40
                | (long)buffer[startIndex + 3] << 32
                | (long)buffer[startIndex + 4] << 24
                | (long)buffer[startIndex + 5] << 16
                | (long)buffer[startIndex + 6] << 8
                | buffer[startIndex + 7];
        }

        /// <summary>
        /// Returns an <see cref="Single"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        [SecuritySafeCritical]
        public override unsafe Single ToSingle(byte[] buffer, int startIndex = 0)
        {
            Int32 raw =
                buffer[startIndex] << 24
                | buffer[startIndex + 1] << 16
                | buffer[startIndex + 2] << 8
                | buffer[startIndex + 3];
            return *(Single*)&raw;
        }

        /// <summary>
        /// Returns an <see cref="UInt16"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public override UInt16 ToUInt16(byte[] buffer, int startIndex = 0)
        {
            return (UInt16)(buffer[startIndex] << 8 | buffer[startIndex + 1]);
        }

        /// <summary>
        /// Returns an <see cref="UInt32"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public override UInt32 ToUInt32(byte[] buffer, int startIndex = 0)
        {
            return (UInt32)(
                buffer[startIndex] << 24
                | buffer[startIndex + 1] << 16
                | buffer[startIndex + 2] << 8
                | buffer[startIndex + 3]
            );
        }

        /// <summary>
        /// Returns an <see cref="UInt64"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public override UInt64 ToUInt64(byte[] buffer, int startIndex = 0)
        {
            return (ulong)buffer[startIndex] << 56
                | (ulong)buffer[startIndex + 1] << 48
                | (ulong)buffer[startIndex + 2] << 40
                | (ulong)buffer[startIndex + 3] << 32
                | (ulong)buffer[startIndex + 4] << 24
                | (ulong)buffer[startIndex + 5] << 16
                | (ulong)buffer[startIndex + 6] << 8
                | buffer[startIndex + 7];
        }
    }
}
