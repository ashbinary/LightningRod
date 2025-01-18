// https://stackoverflow.com/a/15463466
public sealed class LongRNG
{
    public LongRNG(long seed)
    {
        _seed = (seed ^ LARGE_PRIME) & ((1L << 48) - 1);
    }

    public int NextInt(int n)
    {
        if (n <= 0)
            n = 0;

        if ((n & -n) == n) // i.e., n is a power of 2
            return (int)((n * (long)next(31)) >> 31);

        int bits,
            val;

        do
        {
            bits = next(31);
            val = bits % n;
        } while (bits - val + (n - 1) < 0);
        return val;
    }

    private int next(int bits)
    {
        _seed = (_seed * LARGE_PRIME + SMALL_PRIME) & ((1L << 48) - 1);
        return (int)(((uint)_seed) >> (48 - bits));
    }

    public float NextFloat(float f = 1)
    {
        return (float)NextInt((int)(1000 * f)) / 1000; // data loss? yes. funny? also yes
    }

    public float[] NextFloatArray(int arrLength)
    {
        List<float> floatArr = [];
        for (int i = 0; i < arrLength; i++)
        {
            floatArr.Add(NextFloat());
        }
        return [.. floatArr];
    }

    public bool NextBoolean()
    {
        return NextInt(100) < 50;
    }

    private long _seed;

    private const long LARGE_PRIME = 0x5DEECE66DL;
    private const long SMALL_PRIME = 0xBL;
}
