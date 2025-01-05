using System.IO;

namespace NintendoTools.FileFormats;

/// <summary>
/// Base interface for all file format serializers.
/// </summary>
/// <typeparam name="T">The type of the file.</typeparam>
public interface IFileSerializer<in T> where T : class
{
    /// <summary>
    /// Serializes a file object.
    /// </summary>
    /// <param name="writer">A <see cref="TextWriter"/> to use for the serialization.</param>
    /// <param name="file">The file to serialize.</param>
    public void Serialize(TextWriter writer, T file);
}