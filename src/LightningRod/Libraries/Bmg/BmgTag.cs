using System;
using LightningRod.Libraries.Msbt;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A class holding information about a BMG control tag.
/// </summary>
public class BmgTag
{
    /// <summary>
    /// Gets or sets the group of the tag.
    /// </summary>
    public byte Group { get; set; }

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
    /// <exception cref="ArgumentOutOfRangeException"></exception>
    public static implicit operator BmgTag(MsbtTag tag)
    {
        if (tag.Group > byte.MaxValue) throw new ArgumentOutOfRangeException(nameof(tag), "BMG tag group value cannot be greater than 255.");
        return new BmgTag {Group = (byte) tag.Group, Type = tag.Type, Args = tag.Args};
    }
}