using System.Runtime.CompilerServices;
using System.Text;

namespace NintendoTools.Utils;

internal static class EncodingExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetMinByteCount(this Encoding encoding) => encoding.GetByteCount("\0");
}
