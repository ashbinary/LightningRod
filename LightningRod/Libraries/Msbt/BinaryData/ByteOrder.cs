namespace OatmealDome.BinaryData
{
    /// <summary>
    /// Represents the possible endianness of binary data.
    /// </summary>
    public enum ByteOrder : ushort
    {
        /// <summary>
        /// Indicates the byte order of the system executing the assembly.
        /// </summary>
        System,

        /// <summary>
        /// Indicates big endian byte order.
        /// </summary>
        BigEndian = 0xFEFF,

        /// <summary>
        /// Indicates little endian byte order.
        /// </summary>
        LittleEndian = 0xFFFE,
    }
}
