using LightningRod.Libraries.Bmg;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class holding information about a MSBT control tag.
/// </summary>
public class MsbtTag
{
    /// <summary>
    /// Gets or sets the group of the tag.
    /// </summary>
    public ushort Group { get; set; }

    /// <summary>
    /// Gets or sets the type of the tag.
    /// </summary>
    public ushort Type { get; set; }

    /// <summary>
    /// Gets or sets the arguments of the tag.
    /// </summary>
    public byte[] Args { get; set; } = [];

    /// <summary>
    /// Converts a <see cref="BmgTag"/> instance into a new instance of the <see cref="MsbtTag"/> class.
    /// </summary>
    /// <param name="tag">The <see cref="BmgTag"/> instance to convert.</param>
    public static implicit operator MsbtTag(BmgTag tag) =>
        new()
        {
            Group = tag.Group,
            Type = tag.Type,
            Args = tag.Args,
        };
}
