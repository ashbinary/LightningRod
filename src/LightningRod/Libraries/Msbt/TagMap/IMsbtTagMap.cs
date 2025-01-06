using System.Collections.Generic;
using System.Text;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// An interface for the MSBT tag lookup during message formatting.
/// </summary>
public interface IMsbtTagMap
{
    /// <summary>
    /// Gets the name and argument list of a MSBT tag.
    /// </summary>
    /// <param name="tag">The MSBT tag.</param>
    /// <param name="bigEndian">Whether to use big endian for argument values.</param>
    /// <param name="encoding">The encoding to use for string values.</param>
    /// <param name="tagName">The name of the tag.</param>
    /// <param name="tagArgs">A list of tag arguments.</param>
    public void GetTag(
        MsbtTag tag,
        bool bigEndian,
        Encoding encoding,
        out string tagName,
        out IEnumerable<MsbtTagArgument> tagArgs
    );
}
