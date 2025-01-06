using System.Text;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class containing information about a data type.
/// </summary>
public class DataType
{
    /// <summary>
    /// The name of the data type.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The default length in bytes.
    /// </summary>
    public required int Length { get; init; }

    /// <summary>
    /// Function to serialize a value of this data type into bytes.
    /// </summary>
    public required SerializeMethod Serialize { get; init; }

    /// <summary>
    /// Function to deserialize bytes into a value of this data type.
    /// </summary>
    public required DeserializeMethod Deserialize { get; init; }

    /// <summary>
    /// Serializes a string into this data type as byte array.
    /// </summary>
    /// <param name="value">The string value to serialize.</param>
    /// <param name="isBigEndian">Whether the value is in big endian.</param>
    /// <param name="encoding">The used encoding for the string value.</param>
    /// <returns>The serialized value as byte array.</returns>
    public delegate byte[] SerializeMethod(string value, bool isBigEndian, Encoding encoding);

    /// <summary>
    /// Deserializes a byte array into this data type as string value.
    /// </summary>
    /// <param name="value">The byte array to deserialize.</param>
    /// <param name="offset">The offset into the byte array from which to start.</param>
    /// <param name="isBigEndian">Whether the value is in big endian.</param>
    /// <param name="encoding">The encoding to use for the string value.</param>
    /// <returns>The deserialized string value and the number of bytes read from the array.</returns>
    public delegate (string, int) DeserializeMethod(
        byte[] value,
        int offset,
        bool isBigEndian,
        Encoding encoding
    );
}
