using System;

namespace OatmealDome.BinaryData
{
    /// <summary>
    /// Represents a <see cref="BitConverter"/> which handles a specific endianness.
    /// </summary>
    public abstract class ByteConverter
    {
        // ---- CONSTANTS ----------------------------------------------------------------------------------------------

        /// <summary>
        /// The exception thrown if a conversion buffer is too small or <c>null</c>.
        /// </summary>
        protected static readonly Exception BufferException = new Exception(
            "Buffer null or too small."
        );

        // ---- CONSTRUCTORS & DESTRUCTOR ------------------------------------------------------------------------------

        /// <summary>
        /// Initializes static members of the <see cref="ByteConverter"/> class.
        /// </summary>
        static ByteConverter()
        {
            LittleEndian = new ByteConverterLittleEndian();
            BigEndian = new ByteConverterBigEndian();
            System = BitConverter.IsLittleEndian ? LittleEndian : BigEndian;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteConverter"/> class.
        /// </summary>
        protected ByteConverter() { }

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        /// <summary>
        /// Gets a <see cref="ByteConverter"/> instance converting data stored in little endian byte order.
        /// </summary>
        public static ByteConverter LittleEndian { get; }

        /// <summary>
        /// Gets a <see cref="ByteConverter"/> instance converting data stored in big endian byte order.
        /// </summary>
        public static ByteConverter BigEndian { get; }

        /// <summary>
        /// Gets a <see cref="ByteConverter"/> instance converting data stored in the byte order of the system
        /// executing the assembly.
        /// </summary>
        public static ByteConverter System { get; }

        /// <summary>
        /// Gets the <see cref="BinaryData.ByteOrder"/> in which data is stored as converted by this instance.
        /// </summary>
        public abstract ByteOrder ByteOrder { get; }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Returns a <see cref="ByteConverter"/> for the given <paramref name="byteOrder"/>.
        /// </summary>
        /// <param name="byteOrder">The <see cref="ByteOrder"/> to retrieve a converter for.</param>
        /// <returns>The corresponding <see cref="ByteConverter"/> instance.</returns>
        public static ByteConverter GetConverter(ByteOrder byteOrder)
        {
            switch (byteOrder)
            {
                case ByteOrder.BigEndian:
                    return BigEndian;
                case ByteOrder.LittleEndian:
                    return LittleEndian;
                case ByteOrder.System:
                    return System;
                default:
                    throw new ArgumentException($"Invalid {nameof(ByteOrder)}.", nameof(byteOrder));
            }
        }

        /// <summary>
        /// Stores the specified <see cref="Decimal"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public void GetBytes(Decimal value, byte[] buffer, int startIndex = 0)
        {
            if (buffer?.Length - startIndex < sizeof(Decimal))
                throw BufferException;

            // Decimal is composed of low, middle, high and flags Int32 instances which are not affected by endianness.
            int[] parts = Decimal.GetBits(value);
            for (int i = 0; i < 4; i++)
            {
                int offset = startIndex + i * sizeof(int);
                int part = parts[i];
                buffer[offset] = (byte)part;
                buffer[offset + 1] = (byte)(part >> 8);
                buffer[offset + 2] = (byte)(part >> 16);
                buffer[offset + 3] = (byte)(part >> 24);
            }
        }

        /// <summary>
        /// Stores the specified <see cref="Double"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public abstract void GetBytes(Double value, byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Stores the specified <see cref="Int16"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public abstract void GetBytes(Int16 value, byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Stores the specified <see cref="Int32"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public abstract void GetBytes(Int32 value, byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Stores the specified <see cref="Int64"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public abstract void GetBytes(Int64 value, byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Stores the specified <see cref="Single"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public abstract void GetBytes(Single value, byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Stores the specified <see cref="UInt16"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public abstract void GetBytes(UInt16 value, byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Stores the specified <see cref="UInt32"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public abstract void GetBytes(UInt32 value, byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Stores the specified <see cref="UInt64"/> value as bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="value">The value to convert.</param>
        /// <param name="buffer">The byte array to store the value in.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        public abstract void GetBytes(UInt64 value, byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Returns an <see cref="Decimal"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public Decimal ToDecimal(byte[] buffer, int startIndex = 0)
        {
            if (buffer?.Length - startIndex < sizeof(Decimal))
                throw BufferException;

            // Decimal is composed of low, middle, high and flags Int32 instances which are not affected by endianness.
            int[] parts = new int[4];
            for (int i = 0; i < 4; i++)
            {
                int offset = startIndex + i * sizeof(int);
                parts[i] =
                    buffer[offset]
                    | buffer[offset + 1] << 8
                    | buffer[offset + 2] << 16
                    | buffer[offset + 3] << 24;
            }
            return new Decimal(parts);
        }

        /// <summary>
        /// Returns an <see cref="Double"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public abstract Double ToDouble(byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Returns an <see cref="Int16"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public abstract Int16 ToInt16(byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Returns an <see cref="Int32"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public abstract Int32 ToInt32(byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Returns an <see cref="Int64"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public abstract Int64 ToInt64(byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Returns an <see cref="Single"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public abstract Single ToSingle(byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Returns an <see cref="UInt16"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public abstract UInt16 ToUInt16(byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Returns an <see cref="UInt32"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public abstract UInt32 ToUInt32(byte[] buffer, int startIndex = 0);

        /// <summary>
        /// Returns an <see cref="UInt64"/> instance converted from the bytes in the given <paramref name="buffer"/>.
        /// </summary>
        /// <param name="buffer">The byte array storing the raw data.</param>
        /// <param name="startIndex">The index at which to start writing values into the buffer.</param>
        /// <returns>The converted value.</returns>
        public abstract UInt64 ToUInt64(byte[] buffer, int startIndex = 0);
    }
}
