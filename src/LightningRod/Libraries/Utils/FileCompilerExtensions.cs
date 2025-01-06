using System;
using System.IO;

namespace LightningRod.Libraries;

/// <summary>
/// An extension class for <see cref="IFileCompiler{T}"/> types.
/// </summary>
public static class FileCompilerExtensions
{
    /// <summary>
    /// Compiles a file format to a file.
    /// </summary>
    /// <param name="compiler">The <see cref="IFileCompiler{T}"/> instance to use.</param>
    /// <param name="file">The file to compile.</param>
    /// <param name="filePath">The path of the file to compile to.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public static void Compile<T>(this IFileCompiler<T> compiler, T file, string filePath)
        where T : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(compiler, nameof(compiler));
        ArgumentNullException.ThrowIfNull(file, nameof(file));
        ArgumentNullException.ThrowIfNull(file, nameof(filePath));
#else
        if (compiler is null)
            throw new ArgumentNullException(nameof(compiler));
        if (file is null)
            throw new ArgumentNullException(nameof(file));
        if (filePath is null)
            throw new ArgumentNullException(nameof(filePath));
#endif

        using var stream = new FileStream(
            filePath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.None
        );
        compiler.Compile(file, stream);
    }

    /// <summary>
    /// Compiles a file format to a byte array.
    /// </summary>
    /// <param name="compiler">The <see cref="IFileCompiler{T}"/> instance to use.</param>
    /// <param name="file">The file to compile.</param>
    /// <returns>The file compiled as byte array.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static byte[] Compile<T>(this IFileCompiler<T> compiler, T file)
        where T : class
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(compiler, nameof(compiler));
        ArgumentNullException.ThrowIfNull(file, nameof(file));
#else
        if (compiler is null)
            throw new ArgumentNullException(nameof(compiler));
        if (file is null)
            throw new ArgumentNullException(nameof(file));
#endif

        using var stream = new MemoryStream();
        compiler.Compile(file, stream);
        return stream.ToArray();
    }
}
