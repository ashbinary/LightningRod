using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class holding information about a MSBT message.
/// </summary>
public class MsbtMessage
{
    #region private members
    private static readonly IMsbtTagMap DefaultTable = new MsbtDefaultTagMap();
    private static readonly IMsbtFormatProvider DefaultFormatProvider = new MsbtDefaultFormatProvider();
    #endregion

    #region public properties
    /// <summary>
    /// The message ID.
    /// Only present if the file has a NLI1 section.
    /// </summary>
    public uint Id { get; set; }

    /// <summary>
    /// The label of the message.
    /// Only present if the file has a LBL1 section.
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// The attribute data of the message.
    /// Only present if the file has an ATR1 section.
    /// </summary>
    public byte[] Attribute { get; set; } = [];

    /// <summary>
    /// The attribute data of the message as text.
    /// Only present if the encoded ATR1 section had a string table.
    /// </summary>
    public string? AttributeText { get; set; }

    /// <summary>
    /// The text style index into a MSBP file.
    /// Only present if the file has a TSY1 section.
    /// </summary>
    public uint StyleIndex { get; set; }

    /// <summary>
    /// The message text.
    /// </summary>
    public string Text { get; set; } = string.Empty;

    /// <summary>
    /// A list of <see cref="MsbtTag"/> instances found in the text of the message.
    /// </summary>
    public List<MsbtTag> Tags { get; set; } = [];
    #endregion

    #region public methods
    /// <inheritdoc/>
    public override string ToString() => Text;

    /// <summary>
    /// Converts the message text to a clean string. All function call templates are removed.
    /// </summary>
    /// <returns>A cleaned string.</returns>
    public string ToCleanString() => Regex.Replace(Text, @"{{\d+}}", string.Empty, RegexOptions.Compiled);

    /// <summary>
    /// Converts the message text, adding function call declarations and values.
    /// Uses instances of the <see cref="MsbtDefaultTagMap"/> and <see cref="MsbtDefaultFormatProvider"/> classes to convert and format.
    /// </summary>
    /// <param name="bigEndian">Whether to use big endian for parsing argument values.</param>
    /// <param name="encoding">The encoding to use for string values in function arguments.</param>
    /// <returns>A converted string.</returns>
    public string ToCompiledString(bool bigEndian, Encoding encoding) => ToCompiledString(DefaultTable, DefaultFormatProvider, bigEndian, encoding);

    /// <summary>
    /// Converts the message text, adding function call declarations and values.
    /// </summary>
    /// <param name="tagMap">The tag map to use for tag lookup.</param>
    /// <param name="formatProvider">The format provider to use for string formatting.</param>
    /// <param name="bigEndian">Whether to use big endian for parsing argument values.</param>
    /// <param name="encoding">The encoding to use for string values in function arguments.</param>
    /// <returns>A converted string.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public string ToCompiledString(IMsbtTagMap tagMap, IMsbtFormatProvider formatProvider, bool bigEndian, Encoding encoding)
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
            tagMap.GetTag(Tags[i], bigEndian, encoding, out var functionName, out var functionArgs);
            result = result.Replace("{{" + i + "}}", formatProvider.FormatTag(this, functionName, functionArgs));
        }

        return result.ToString();
    }
    #endregion
}