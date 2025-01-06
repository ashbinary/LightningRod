using LightningRod.Libraries.Msbt;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A class holding information about a BMG tag argument.
/// </summary>
public class BmgTagArgument
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BmgTagArgument"/> class.
    /// </summary>
    /// <param name="name">The name of the tag argument.</param>
    /// <param name="value">The value of the tag argument.</param>
    public BmgTagArgument(string name, object? value = null)
    {
        Name = name;
        Value = value;
    }

    /// <summary>
    /// Gets or sets the name of the tag argument.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Gets or sets the value of the tag argument.
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// Converts a <see cref="MsbtTagArgument"/> instance into a new instance of the <see cref="BmgTagArgument"/> class.
    /// </summary>
    /// <param name="arg">The <see cref="MsbtTagArgument"/> instance to convert.</param>
    public static implicit operator BmgTagArgument(MsbtTagArgument arg) => new(arg.Name, arg.Value);
}
