using System.Collections.Generic;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// An interface for the format provider of BMG messages and tags.
/// </summary>
public interface IBmgFormatProvider
{
    /// <summary>
    /// Formats the raw text of a <see cref="BmgMessage"/>.
    /// </summary>
    /// <param name="message">The <see cref="BmgMessage"/> object.</param>
    /// <param name="rawText">The raw message text.</param>
    /// <returns>A formatted string representing a <see cref="BmgMessage"/>.</returns>
    public string FormatMessage(BmgMessage message, string rawText);

    /// <summary>
    /// Formats a BMG tag and its arguments.
    /// </summary>
    /// <param name="message">The <see cref="BmgMessage"/> object.</param>
    /// <param name="tagName">The name of the tag.</param>
    /// <param name="arguments">The list of tag arguments.</param>
    /// <returns>A formatted string representing a BMG tag.</returns>
    public string FormatTag(
        BmgMessage message,
        string tagName,
        IEnumerable<BmgTagArgument> arguments
    );
}
