using System.Collections.Generic;
using System.IO;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// An interface for serializing <see cref="MsbtFile"/> objects.
/// </summary>
public interface IMsbtSerializer : IFileSerializer<MsbtFile>
{
    #region properties
    /// <summary>
    /// Gets or sets the function table to use.
    /// </summary>
    public IMsbtTagMap TagMap { get; set; }

    /// <summary>
    /// Gets or sets the message format provider to use.
    /// </summary>
    public IMsbtFormatProvider FormatProvider { get; set; }
    #endregion

    #region methods
    /// <summary>
    /// Serializes a collection of <see cref="MsbtFile"/> objects from multiple languages.
    /// </summary>
    /// <param name="writer">A <see cref="TextWriter"/> to use for the serialization.</param>
    /// <param name="files">The collection of <see cref="MsbtFile"/> objects to serialize.</param>
    public void Serialize(TextWriter writer, IDictionary<string, MsbtFile> files);
    #endregion
}