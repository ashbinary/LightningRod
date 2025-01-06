using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace NintendoTools.Utils;

internal static class RegexExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsMatch(
        this Regex regex,
        string input,
        [MaybeNullWhen(false)] out Match match
    )
    {
        match = regex.Match(input);
        return match.Success;
    }
}
