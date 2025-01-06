using System.Collections.Generic;
using System.Text;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// An interface for the BMG tag lookup during message formatting.
/// </summary>
public interface IBmgTagMap
{
    /// <summary>
    /// Gets the name and argument list of a BMG tag.
    /// </summary>
    /// <param name="tag">The BMG tag.</param>
    /// <param name="bigEndian">Whether to use big endian for argument values.</param>
    /// <param name="encoding">The encoding to use for string values.</param>
    /// <param name="tagName">The name of the tag.</param>
    /// <param name="tagArgs">A list of tag arguments.</param>
    public void GetTag(
        BmgTag tag,
        bool bigEndian,
        Encoding encoding,
        out string tagName,
        out IEnumerable<BmgTagArgument> tagArgs
    );
}
