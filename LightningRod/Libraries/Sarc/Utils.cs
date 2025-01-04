using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LightningRod.Libraries.Sarc
{
    public static class Utils
    {
        public static ref T AsStruct<T>(this Span<byte> data)
            where T : unmanaged
        {
            return ref MemoryMarshal.Cast<byte, T>(data[..Unsafe.SizeOf<T>()])[0];
        }

        public static Span<T> AsStructSpan<T>(this Span<byte> data, int count)
            where T : unmanaged
        {
            return MemoryMarshal.Cast<byte, T>(data[..(count * Unsafe.SizeOf<T>())]);
        }

        public static T DivideAndCeil<T>(T numerator, T denominator)
            where T : INumber<T>, IModulusOperators<T, T, T>
        {
            var div = numerator / denominator;
            var mod = numerator % denominator;
            return div + (T.IsZero(mod) ? T.Zero : T.One);
        }

        public static T RoundUp<T>(T value, T alignment)
            where T : INumber<T>, IModulusOperators<T, T, T>
        {
            return DivideAndCeil(value, alignment) * alignment;
        }

        public static int BinarySearch<T, K>(ReadOnlySpan<T> arr, K v)
            where T : IComparable<K>
        {
            var start = 0;
            var end = arr.Length - 1;

            while (start <= end)
            {
                var mid = (start + end) / 2;
                var entry = arr[mid];
                var cmp = entry.CompareTo(v);

                if (cmp == 0)
                    return mid;
                if (cmp > 0)
                    end = mid - 1;
                else /* if (cmp < 0) */
                    start = mid + 1;
            }

            return ~start;
        }
    }
}
