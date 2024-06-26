using System.IO;

namespace OatmealDome.BinaryData
{
    /// <summary>
    /// Represents a converter for reading and writing custom binary values.
    /// </summary>
    public interface IBinaryConverter
    {
        // ---- METHODS ------------------------------------------------------------------------------------------------

        /// <summary>
        /// Reads the value from the given <paramref name="stream"/> and returns it to set the corresponding member.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read the value from.</param>
        /// <param name="instance">The instance to which the value belongs.</param>
        /// <param name="memberAttribute">The <see cref="BinaryMemberAttribute"/> containing configuration which can be
        /// used to modify the behavior of the converter.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multi-byte data.</param>
        /// <returns>The read value.</returns>
        object Read(Stream stream, object instance, BinaryMemberAttribute memberAttribute, ByteConverter converter);

        /// <summary>
        /// Writes the value with the given <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to write the value with.</param>
        /// <param name="instance">The instance to which the value belongs.</param>
        /// <param name="memberAttribute">The <see cref="BinaryMemberAttribute"/> containing configuration which can be
        /// used to modify the behavior of the converter.</param>
        /// <param name="value">The value to write.</param>
        /// <param name="converter">The <see cref="ByteConverter"/> to use for converting multi-byte data.</param>
        void Write(Stream stream, object instance, BinaryMemberAttribute memberAttribute, object value,
            ByteConverter converter);
    }
}
