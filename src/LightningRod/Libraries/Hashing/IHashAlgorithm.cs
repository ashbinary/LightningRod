using System.IO;

namespace NintendoTools.Hashing;

/// <summary>
/// The interface for all hashing algorithms.
/// </summary>
public interface IHashAlgorithm
{
    /// <summary>
    /// Computes the hash for the given data.
    /// </summary>
    /// <param name="data">The data to hash.</param>
    /// <returns>The hashed result.</returns>
    public byte[] Compute(Stream data);
}
