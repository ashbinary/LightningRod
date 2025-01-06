using System;
using System.IO;
using Force.Crc32;

namespace NintendoTools.Hashing;

/// <summary>
/// A class for computing CRC32C hashes.
/// </summary>
public class Crc32CHash : IHashAlgorithm
{
    #region private members
    private readonly Crc32CAlgorithm _algorithm = new();
    #endregion

    #region IHashAlgorithm interface
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public byte[] Compute(Stream data)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(data, nameof(data));
#else
        if (data is null)
            throw new ArgumentNullException(nameof(data));
#endif

        return _algorithm.ComputeHash(data);
    }
    #endregion
}
