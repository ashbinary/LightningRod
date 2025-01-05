using System;
using System.IO;
using System.Text;

namespace NintendoTools.Hashing;

/// <summary>
/// A class for computing MurmurHash3 hashes.
/// </summary>
public class Mmh3Hash : IHashAlgorithm
{
    #region private members
    private readonly uint _seed;
    #endregion

    #region constructors
    /// <summary>
    /// Initializes a new instance of the <see cref="Mmh3Hash"/> class with the default seed.
    /// </summary>
    public Mmh3Hash() : this(0)
    { }

    /// <summary>
    /// Initializes a new instance of the <see cref="Mmh3Hash"/> class with the given seed.
    /// </summary>
    /// <param name="seed">The seed value to use.</param>
    public Mmh3Hash(uint seed) => _seed = seed;
    #endregion

    #region IHashAlgorithm interface
    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public byte[] Compute(Stream data)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(data, nameof(data));
        #else
        if (data is null) throw new ArgumentNullException(nameof(data));
        #endif

        var hash = InternalCompute(data);
        var bytes = BitConverter.GetBytes(hash);
        if (BitConverter.IsLittleEndian) Array.Reverse(bytes);
        return bytes;
    }
    #endregion

    #region private methods
    private uint InternalCompute(Stream stream)
    {
        const uint c1 = 0xCC9E2D51;
        const uint c2 = 0x1B873593;

        var h1 = _seed;
        uint streamLength = 0;

        using (var reader = new BinaryReader(stream, Encoding.UTF8, true))
        {
            var chunk = reader.ReadBytes(4);
            while (chunk.Length > 0)
            {
                streamLength += (uint)chunk.Length;
                uint k1;
                switch (chunk.Length)
                {
                    case 4:
                        k1 = (uint)(chunk[0] | chunk[1] << 8 | chunk[2] << 16 | chunk[3] << 24);
                        k1 *= c1;
                        k1 = Rot(k1, 15);
                        k1 *= c2;
                        h1 ^= k1;
                        h1 = Rot(h1, 13);
                        h1 = h1 * 5 + 0xE6546b64;
                        break;
                    case 3:
                        k1 = (uint)(chunk[0] | chunk[1] << 8 | chunk[2] << 16);
                        k1 *= c1;
                        k1 = Rot(k1, 15);
                        k1 *= c2;
                        h1 ^= k1;
                        break;
                    case 2:
                        k1 = (uint)(chunk[0] | chunk[1] << 8);
                        k1 *= c1;
                        k1 = Rot(k1, 15);
                        k1 *= c2;
                        h1 ^= k1;
                        break;
                    case 1:
                        k1 = chunk[0];
                        k1 *= c1;
                        k1 = Rot(k1, 15);
                        k1 *= c2;
                        h1 ^= k1;
                        break;
                }
                chunk = reader.ReadBytes(4);
            }
        }

        h1 ^= streamLength;
        h1 = Mix(h1);

        return h1;
    }

    private static uint Rot(uint x, byte r) => x << r | x >> 32 - r;

    private static uint Mix(uint h)
    {
        h ^= h >> 16;
        h *= 0x85EBCA6B;
        h ^= h >> 13;
        h *= 0xC2B2AE35;
        h ^= h >> 16;
        return h;
    }
    #endregion
}