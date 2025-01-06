using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using LightningRod.Libraries.Msbt;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A dynamic BMG tag map.
/// </summary>
public class BmgDynamicTagMap : IBmgTagMap, IEnumerable<BmgTagInfo>
{
    #region private members
    private readonly MsbtDynamicTagMap _map = new();
    private readonly Dictionary<MsbtTagInfo, BmgTagInfo> _referenceMap = [];
    #endregion

    #region IMsbtTagMap interface
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    public void GetTag(
        BmgTag tag,
        bool bigEndian,
        Encoding encoding,
        out string tagName,
        out IEnumerable<BmgTagArgument> tagArgs
    )
    {
        _map.GetTag(tag, bigEndian, encoding, out tagName, out var msbtTagArgs);
        tagArgs = msbtTagArgs.Select(arg => (BmgTagArgument)arg);
    }
    #endregion

    #region public properties
    /// <summary>
    /// Gets the number of tag definitions in the map.
    /// </summary>
    public int Count => _map.Count;
    #endregion

    #region public methods
    /// <summary>
    /// Adds a new tag definition entry to the map.
    /// </summary>
    /// <param name="tag">The tag definition to add.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentException"></exception>
    public void AddInfo(BmgTagInfo tag)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(tag, nameof(tag));
#else
        if (tag is null)
            throw new ArgumentNullException(nameof(tag));
#endif

        var msbtTag = (MsbtTagInfo)tag;
        _map.AddInfo(msbtTag);
        _referenceMap[msbtTag] = tag;
    }

    /// <summary>
    /// Attempts to get tag definition with the given group and type values.
    /// </summary>
    /// <param name="group">The tag group value.</param>
    /// <param name="type">The tag type value.</param>
    /// <param name="tag">The retrieved tag definition.</param>
    /// <returns><see langword="true"/> if tag was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetInfo(ushort group, ushort type, [MaybeNullWhen(false)] out BmgTagInfo tag)
    {
        tag = null;
        if (!_map.TryGetInfo(group, type, out var msbtTag))
            return false;
        tag = _referenceMap[msbtTag];
        return true;
    }

    /// <summary>
    /// Attempts to get tag definition with the given name value.
    /// </summary>
    /// <param name="tagName">The tag name value.</param>
    /// <param name="tag">The retrieved tag definition.</param>
    /// <returns><see langword="true"/> if tag was found; otherwise <see langword="false"/>.</returns>
    public bool TryGetInfo(string tagName, [MaybeNullWhen(false)] out BmgTagInfo tag)
    {
        tag = null;
        if (!_map.TryGetInfo(tagName, out var msbtTag))
            return false;
        tag = _referenceMap[msbtTag];
        return true;
    }
    #endregion

    #region IEnumerable interface
    /// <inheritdoc/>
    public IEnumerator<BmgTagInfo> GetEnumerator() => _referenceMap.Values.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}
