namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class holding information about a MSBT tag argument.
/// </summary>
public class MsbtArgumentInfo
{
    /// <summary>
    /// The datatype of the argument value.
    /// </summary>
    public required DataType DataType { get; init; }

    /// <summary>
    /// A map of possible argument values.
    /// </summary>
    public MsbtValueMap ValueMap { get; init; } = MsbtValueMap.Empty;

    /// <summary>
    /// The length of the argument value, if the value is an array.
    /// </summary>
    public int ArrayLength { get; init; }

    /// <summary>
    /// Whether the argument value is data padding.
    /// Padding arguments are not shown in the editor.
    /// </summary>
    public bool IsPadding => DataType is PaddingDataType;

    /// <summary>
    /// Whether the argument value is a data discard.
    /// Discard arguments can only be the last argument in the list.
    /// Discard arguments are not shown in the editor.
    /// </summary>
    public bool IsDiscard { get; init; }

    /// <summary>
    /// The name of the argument.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The description of the argument.
    /// </summary>
    public string Description { get; init; } = string.Empty;
}