using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NintendoTools.Utils;

internal static class StringExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string Reverse(this string str)
    {
        var chars = str.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string str, params char[] values)
    {
        foreach (var c in values)
        {
            if (str.Contains(c))
                return true;
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ContainsAny(this string str, IEnumerable<char> values)
    {
        foreach (var value in values)
        {
            if (str.Contains(value))
                return true;
        }
        return false;
    }
}
