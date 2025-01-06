using System;
using System.Collections.Generic;
using System.Text;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// Default implementation of a <see cref="IBmgTagMap"/>.
/// Returns tag name as <c>fun_groupHash_typeHash</c> and argument data as single argument converted to hex string.
/// </summary>
public class BmgDefaultTagMap : IBmgTagMap
{
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public void GetTag(
        BmgTag tag,
        bool bigEndian,
        Encoding encoding,
        out string tagName,
        out IEnumerable<BmgTagArgument> tagArgs
    )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(tag, nameof(tag));
        ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));
#else
        if (tag is null)
            throw new ArgumentNullException(nameof(tag));
        if (encoding is null)
            throw new ArgumentNullException(nameof(encoding));
#endif

        tagName = $"fun_{tag.Group:X2}_{tag.Type:X4}";
        var argList = new List<BmgTagArgument>();
        if (tag.Args.Length > 0)
            argList.Add(new BmgTagArgument("arg", tag.Args.ToHexString(true)));
        tagArgs = argList;
    }
}
