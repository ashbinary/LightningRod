using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NintendoTools.Utils;

internal static class StackExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PushRange<T>(this Stack<T> stack, IEnumerable<T> items)
    {
        foreach (var item in items)
            stack.Push(item);
    }
}
