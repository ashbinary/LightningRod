namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class holding information about a MSBT value used in a type or value map in <see cref="MsbtTagInfo"/>.
/// </summary>
public class MsbtValueInfo
{
    /// <summary>
    /// The mapped value.
    /// </summary>
    public required string Value { get; init; }

    /// <summary>
    /// The name of the value.
    /// </summary>
    public string Name { get; init; } = string.Empty;

    /// <summary>
    /// The description of the value.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// Gets the name of the value if set; otherwise returns the value itself.
    /// </summary>
    public string NameOrValue => string.IsNullOrEmpty(Name) ? Value : Name;
}