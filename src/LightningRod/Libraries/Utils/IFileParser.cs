using System.IO;

namespace LightningRod.Libraries;

/// <summary>
/// The base interface for file parsers.
/// </summary>
public interface IFileParser
{
    /// <summary>
    /// Validates whether the given stream can be parsed with this parser instance.
    /// </summary>
    /// <param name="fileStream">The stream to check.</param>
    /// <returns><see langword="true"/> if can be parsed; otherwise <see langword="false"/>.</returns>
    public bool CanParse(Stream fileStream);
}

/// <summary>
/// The interface for generic file parsers.
/// </summary>
public interface IFileParser<out T> : IFileParser
    where T : class
{
    /// <summary>
    /// Parses a file stream to a file format.
    /// </summary>
    /// <param name="fileStream">The stream to parse.</param>
    /// <returns>The parsed file format.</returns>
    public T Parse(Stream fileStream);
}
