using System.Runtime.CompilerServices;

namespace NintendoTools.Utils;

internal static class BinaryUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetOffset(int length, int alignment) => (-length % alignment + alignment) % alignment;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int GetOffset(long length, int alignment) => ((int) (-length % alignment) + alignment) % alignment;
}