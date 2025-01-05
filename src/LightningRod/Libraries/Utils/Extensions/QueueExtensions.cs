using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NintendoTools.Utils;

internal static class QueueExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void EnqueueRange<T>(this Queue<T> queue, IEnumerable<T> items)
    {
        foreach (var item in items) queue.Enqueue(item);
    }
}