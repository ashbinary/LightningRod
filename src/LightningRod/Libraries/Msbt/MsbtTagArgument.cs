using LightningRod.Libraries.Bmg;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class holding information about a MSBT tag argument.
/// </summary>
public class MsbtTagArgument
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MsbtTagArgument"/> class.
    /// </summary>
    /// <param name="name">The name of the tag argument.</param>
    /// <param name="value">The value of the tag argument.</param>
    public MsbtTagArgument(string name, object? value = null)
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
    /// Converts a <see cref="BmgTagArgument"/> instance into a new instance of the <see cref="MsbtTagArgument"/> class.
    /// </summary>
    /// <param name="arg">The <see cref="BmgTagArgument"/> instance to convert.</param>
    public static implicit operator MsbtTagArgument(BmgTagArgument arg) => new(arg.Name, arg.Value);
}