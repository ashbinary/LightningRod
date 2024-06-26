using System;
using System.IO;
using OatmealDome.BinaryData.Core;

namespace OatmealDome.BinaryData.Extensions
{
    /// <summary>
    /// Represents static extension methods for read and write operations on <see cref="Stream"/> instances.
    /// </summary>
    public static partial class StreamExtensions
    {
        // ---- FIELDS -------------------------------------------------------------------------------------------------

        [ThreadStatic] private static byte[] _buffer;
        [ThreadStatic] private static char[] _charBuffer;

        private static readonly DateTime _cTimeBase = new DateTime(1970, 1, 1);

        // ---- PROPERTIES ---------------------------------------------------------------------------------------------

        private static byte[] Buffer
        {
            get
            {
                if (_buffer == null)
                    _buffer = new byte[16];
                return _buffer;
            }
        }

        private static char[] CharBuffer
        {
            get
            {
                if (_charBuffer == null)
                    _charBuffer = new char[16];
                return _charBuffer;
            }
        }

        // ---- METHODS (PUBLIC) ---------------------------------------------------------------------------------------

        /// <summary>
        /// Aligns the reader to the next given byte multiple.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="alignment">The byte multiple.</param>
        /// <param name="grow"><c>true</c> to enlarge the stream size to include the final position in case it is larger
        /// than the current stream length.</param>
        /// <returns>The new position within the current stream.</returns>
        public static long Align(this Stream stream, int alignment, bool grow = false)
        {
            if (alignment <= 0)
                throw new ArgumentOutOfRangeException("Alignment must be bigger than 0.");
            long position = stream.Seek((-stream.Position % alignment + alignment) % alignment, SeekOrigin.Current);
            if (grow && position > stream.Length)
            {
                stream.SetLength(position);
            }
            return position;
        }

        /// <summary>
        /// Gets a value indicating whether the end of the <paramref name="stream"/> has been reached and no more data
        /// can be read.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <returns>A value indicating whether the end of the stream has been reached.</returns>
        public static bool IsEndOfStream(this Stream stream)
        {
            return stream.Position >= stream.Length;
        }

        /// <summary>
        /// Sets the position within the current <paramref name="stream"/> relative to the current position.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="offset">A byte offset relative to the current position in the stream.</param>
        /// <returns>The new position within the current stream.</returns>
        public static long Seek(this Stream stream, long offset)
        {
            return stream.Seek(offset, SeekOrigin.Current);
        }

        /// <summary>
        /// Creates a <see cref="SeekTask"/> to restore the current position after it has been disposed.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <returns>The <see cref="SeekTask"/> to be disposed to restore to the current position.</returns>
        public static SeekTask TemporarySeek(this Stream stream)
        {
            return stream.TemporarySeek(0, SeekOrigin.Current);
        }

        /// <summary>
        /// Creates a <see cref="SeekTask"/> with the given parameters. As soon as the returned <see cref="SeekTask"/>
        /// is disposed, the previous stream position will be restored.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="offset">A byte offset relative to the current position in the stream.</param>
        /// <returns>The <see cref="SeekTask"/> to be disposed to undo the seek.</returns>
        public static SeekTask TemporarySeek(this Stream stream, long offset)
        {
            return stream.TemporarySeek(offset, SeekOrigin.Current);
        }

        /// <summary>
        /// Creates a <see cref="SeekTask"/> with the given parameters. As soon as the returned <see cref="SeekTask"/>
        /// is disposed, the previous stream position will be restored.
        /// </summary>
        /// <param name="stream">The extended <see cref="Stream"/> instance.</param>
        /// <param name="offset">A byte offset relative to the origin parameter.</param>
        /// <param name="origin">A value of type <see cref="SeekOrigin"/> indicating the reference point used to obtain
        /// the new position.</param>
        /// <returns>The <see cref="SeekTask"/> to be disposed to undo the seek.</returns>
        public static SeekTask TemporarySeek(this Stream stream, long offset, SeekOrigin origin)
        {
            return new SeekTask(stream, offset, origin);
        }

        // ---- METHODS (PRIVATE) --------------------------------------------------------------------------------------

        private static void ValidateEnumValue(Type enumType, object value)
        {
            if (!EnumExtensions.IsValid(enumType, value))
            {
                throw new InvalidDataException($"Read value {value} is not defined in the enum type {enumType}.");
            }
        }
    }
}
