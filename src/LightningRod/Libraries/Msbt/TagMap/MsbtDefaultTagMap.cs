using System;
using System.Collections.Generic;
using System.Text;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// Default implementation of a <see cref="IMsbtTagMap"/>.
/// Returns tag name as <c>fun_groupHash_typeHash</c> and argument data as single argument converted to hex string.
/// </summary>
public class MsbtDefaultTagMap : IMsbtTagMap
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public void GetTag(MsbtTag tag, bool bigEndian, Encoding encoding, out string tagName, out IEnumerable<MsbtTagArgument> tagArgs)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(tag, nameof(tag));
        ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));
        #else
        if (tag is null) throw new ArgumentNullException(nameof(tag));
        if (encoding is null) throw new ArgumentNullException(nameof(encoding));
        #endif

        tagName = $"fun_{tag.Group:X4}_{tag.Type:X4}";
        var argList = new List<MsbtTagArgument>();
        if (tag.Args.Length > 0) argList.Add(new MsbtTagArgument("arg", tag.Args.ToHexString(true)));
        tagArgs = argList;
    }
}