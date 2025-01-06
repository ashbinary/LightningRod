using System.Collections.Generic;
using System.IO;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// An interface for serializing <see cref="BmgFile"/> objects.
/// </summary>
public interface IBmgSerializer : IFileSerializer<BmgFile>
{
    #region properties
    /// <summary>
    /// Gets or sets the tag map to use.
    /// </summary>
    public IBmgTagMap TagMap { get; set; }

    /// <summary>
    /// Gets or sets the message format provider to use.
    /// </summary>
    public IBmgFormatProvider FormatProvider { get; set; }
    #endregion

    #region methods
    /// <summary>
    /// Serializes a collection of <see cref="BmgFile"/> objects from multiple languages.
    /// </summary>
    /// <param name="writer">A <see cref="TextWriter"/> to use for the serialization.</param>
    /// <param name="files">The collection of <see cref="BmgFile"/> objects to serialize.</param>
    public void Serialize(TextWriter writer, IDictionary<string, BmgFile> files);
    #endregion
}
