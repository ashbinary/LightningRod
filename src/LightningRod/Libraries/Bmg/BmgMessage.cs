using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A class holding information about a BMG message.
/// </summary>
public class BmgMessage
{
    #region private members
    private static readonly IBmgTagMap DefaultTagMap = new BmgDefaultTagMap();
    private static readonly IBmgFormatProvider DefaultFormatProvider = new BmgDefaultFormatProvider();
    #endregion

    #region public properties
    /// <summary>
    /// The message ID.
    /// Only present if the file has a MID1 section.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// The label of the message.
    /// Only present if the file has a STR1 section.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The attribute data of the message.
    /// Only present if the file has an INF1 section.
    /// </summary>
    public byte[] Attribute { get; set; } = [];

    /// <summary>
    /// The message text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// A list of <see cref="BmgTag"/> instances found in the text of the message.
    /// </summary>
    public List<BmgTag> Tags { get; set; } = [];
    #endregion

    #region public methods
    /// <inheritdoc/>
    public override string ToString() => Text;

    /// <summary>
    /// Converts the message text to a clean string. All tag templates are removed.
    /// </summary>
    /// <returns>A cleaned string.</returns>
    public string ToCleanString() => Regex.Replace(Text, @"{{\d+}}", string.Empty, RegexOptions.Compiled);

    /// <summary>
    /// Converts the message text, adding tag declarations and values.
    /// Uses instances of the <see cref="BmgDefaultTagMap"/> and <see cref="BmgDefaultFormatProvider"/> classes to convert and format.
    /// </summary>
    /// <param name="bigEndian">Whether to use big endian for parsing argument values.</param>
    /// <param name="encoding">The encoding to use for string values in tag arguments.</param>
    /// <returns>A converted string.</returns>
    public string ToCompiledString(bool bigEndian, Encoding encoding) => ToCompiledString(DefaultTagMap, DefaultFormatProvider, bigEndian, encoding);

    /// <summary>
    /// Converts the message text, adding tag declarations and values.
    /// </summary>
    /// <param name="tagMap">The tag map to use for tag lookup.</param>
    /// <param name="formatProvider">The format provider to use for string formatting.</param>
    /// <param name="bigEndian">Whether to use big endian for parsing argument values.</param>
    /// <param name="encoding">The encoding to use for string values in tag arguments.</param>
    /// <returns>A converted string.</returns>
    public string ToCompiledString(IBmgTagMap tagMap, IBmgFormatProvider formatProvider, bool bigEndian, Encoding encoding)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(tagMap, nameof(tagMap));
        ArgumentNullException.ThrowIfNull(formatProvider, nameof(formatProvider));
        ArgumentNullException.ThrowIfNull(encoding, nameof(encoding));
        #else
        if (tagMap is null) throw new ArgumentNullException(nameof(tagMap));
        if (formatProvider is null) throw new ArgumentNullException(nameof(formatProvider));
        if (encoding is null) throw new ArgumentNullException(nameof(encoding));
        #endif

        var result = new StringBuilder(formatProvider.FormatMessage(this, Text));

        for (var i = 0; i < Tags.Count; ++i)
        {
            tagMap.GetTag(Tags[i], bigEndian, encoding, out var tagName, out var tagArgs);
            result = result.Replace("{{" + i + "}}", formatProvider.FormatTag(this, tagName, tagArgs));
        }

        return result.ToString();
    }
    #endregion
}