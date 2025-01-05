using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NintendoTools.Utils;

internal static class CollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool TryAdd<T>(this ICollection<T> source, T element, Func<T, bool> predicate)
    {
        foreach (var item in source)
        {
            if (!predicate(item)) return false;
        }

        source.Add(element);
        return true;
    }
}