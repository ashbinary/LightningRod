using System;
using System.IO;
using System.Text;

namespace NintendoTools.Hashing;

/// <summary>
/// An extension class for <see cref="IHashAlgorithm"/> types.
/// </summary>
public static class HashAlgorithmExtensions
{
    /// <summary>
    /// Computes the hash for the given data.
    /// </summary>
    /// <param name="hashAlgorithm">The <see cref="IHashAlgorithm"/> instance to use.</param>
    /// <param name="data">The data to hash.</param>
    /// <returns>The hashed result.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string Compute(this IHashAlgorithm hashAlgorithm, string data)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hashAlgorithm, nameof(hashAlgorithm));
        ArgumentNullException.ThrowIfNull(data, nameof(data));
#else
        if (hashAlgorithm is null)
            throw new ArgumentNullException(nameof(hashAlgorithm));
        if (data is null)
            throw new ArgumentNullException(nameof(data));
#endif

        var hash = hashAlgorithm.Compute(Encoding.UTF8.GetBytes(data));
        return BitConverter.ToString(hash).Replace("-", "");
    }

    /// <summary>
    /// Computes the hash for the given data.
    /// </summary>
    /// <param name="hashAlgorithm">The <see cref="IHashAlgorithm"/> instance to use.</param>
    /// <param name="data">The data to hash.</param>
    /// <returns>The hashed result.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static byte[] Compute(this IHashAlgorithm hashAlgorithm, byte[] data)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(hashAlgorithm, nameof(hashAlgorithm));
        ArgumentNullException.ThrowIfNull(data, nameof(data));
#else
        if (hashAlgorithm is null)
            throw new ArgumentNullException(nameof(hashAlgorithm));
        if (data is null)
            throw new ArgumentNullException(nameof(data));
#endif

        using var stream = new MemoryStream(data, false);
        return hashAlgorithm.Compute(stream);
    }
}
