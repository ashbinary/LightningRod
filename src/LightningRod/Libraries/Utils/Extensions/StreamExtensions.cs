using System.IO;
using System.Runtime.CompilerServices;

namespace NintendoTools.Utils;

internal static class StreamExtensions
{
    //converts a stream to byte-array
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] ToArray(this Stream stream)
    {
        if (stream is MemoryStream castedStream)
            return castedStream.ToArray();

        var buffer = new byte[stream.Length];
        stream.Seek(0, SeekOrigin.Begin);
        _ = stream.Read(buffer, 0, buffer.Length);
        return buffer;
    }
}
