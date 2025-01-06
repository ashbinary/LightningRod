using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class containing information about a padding data type.
/// </summary>
public class PaddingDataType : DataType
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PaddingDataType"/> class.
    /// </summary>
    /// <param name="value">The padding value to use.</param>
    [SetsRequiredMembers]
    public PaddingDataType(byte[] value)
    {
        PaddingValue = value;
        Name = DataTypes.HexString.Deserialize(value, 0, false, Encoding.UTF8).Item1;
        Length = value.Length;
        Serialize = (_, _, _) => value;
        Deserialize = (_, _, _, _) => (string.Empty, value.Length);
    }

    /// <summary>
    /// Gets the value used for padding.
    /// </summary>
    public byte[] PaddingValue { get; }
}