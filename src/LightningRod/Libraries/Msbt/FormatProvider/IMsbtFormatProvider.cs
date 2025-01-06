using System.Collections.Generic;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// An interface for the format provider of MSBT messages and tags.
/// </summary>
public interface IMsbtFormatProvider
{
    /// <summary>
    /// Formats the raw text of a <see cref="MsbtMessage"/>.
    /// </summary>
    /// <param name="message">The <see cref="MsbtMessage"/> object.</param>
    /// <param name="rawText">The raw message text.</param>
    /// <returns>A formatted string representing a <see cref="MsbtMessage"/>.</returns>
    public string FormatMessage(MsbtMessage message, string rawText);

    /// <summary>
    /// Formats a MSBT tag and its arguments.
    /// </summary>
    /// <param name="message">The <see cref="MsbtMessage"/> object.</param>
    /// <param name="tagName">The name of the tag.</param>
    /// <param name="arguments">The list of tag arguments.</param>
    /// <returns>A formatted string representing a MSBT tag.</returns>
    public string FormatTag(
        MsbtMessage message,
        string tagName,
        IEnumerable<MsbtTagArgument> arguments
    );
}
