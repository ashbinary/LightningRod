using System.Collections.Generic;
using LightningRod.Libraries.Bmg;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class holding information about a MSBT tag.
/// </summary>
public class MsbtTagInfo
{
    /// <summary>
    /// The group ID of the tag.
    /// </summary>
    public required ushort Group { get; init; }

    /// <summary>
    /// The type ID of the tag.
    /// Optional, if <see cref="TypeList"/>, <see cref="TypeMap"/> or <see cref="HasDiscard"/> is set.
    /// </summary>
    public ushort? Type { get; init; }

    /// <summary>
    /// A list of type IDs assigned to this information instance.
    /// </summary>
    public IReadOnlyList<ushort> TypeList { get; init; } = [];

    /// <summary>
    /// A map of types assigned to this information instance.
    /// </summary>
    public MsbtValueMap TypeMap { get; init; } = MsbtValueMap.Empty;

    /// <summary>
    /// Whether this tag also captures other unassigned type IDs of the same group.
    /// Only one tag discard per group ID can exist.
    /// </summary>
    public bool HasDiscard { get; init; }

    /// <summary>
    /// The name of the tag.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The description of the tag.
    /// </summary>
    public string Description { get; init; } = string.Empty;

    /// <summary>
    /// A collection of tag arguments.
    /// </summary>
    public IReadOnlyList<MsbtArgumentInfo> Arguments { get; init; } = [];

    /// <summary>
    /// Converts a <see cref="BmgTagInfo"/> instance into a new instance of the <see cref="MsbtTagInfo"/> class.
    /// </summary>
    /// <param name="info">The <see cref="BmgTagInfo"/> instance to convert.</param>
    public static implicit operator MsbtTagInfo(BmgTagInfo info) =>
        new()
        {
            Group = info.Group,
            Type = info.Type,
            TypeList = info.TypeList,
            TypeMap = info.TypeMap,
            HasDiscard = info.HasDiscard,
            Name = info.Name,
            Description = info.Description,
            Arguments = info.Arguments,
        };
}
