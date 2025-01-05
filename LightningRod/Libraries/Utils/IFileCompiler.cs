using System.IO;

namespace NintendoTools.FileFormats;

/// <summary>
/// The interface for generic file compilers.
/// </summary>
public interface IFileCompiler<in T> where T : class
{
    /// <summary>
    /// Compiles a file format to a file stream.
    /// </summary>
    /// <param name="file">The file to compile.</param>
    /// <param name="fileStream">The stream to compile to.</param>
    public void Compile(T file, Stream fileStream);
}