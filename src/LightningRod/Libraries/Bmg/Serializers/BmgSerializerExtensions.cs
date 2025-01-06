using System;
using System.Collections.Generic;
using System.IO;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A collection of extension methods for <see cref="IBmgSerializer"/> classes.
/// </summary>
public static class BmgSerializerExtensions
{
    /// <summary>
    /// Serializes a collection of <see cref="BmgFile"/> objects from multiple languages.
    /// </summary>
    /// <param name="serializer">The <see cref="IBmgSerializer"/> instance to use.</param>
    /// <param name="files">The collection of <see cref="BmgFile"/> objects to serialize.</param>
    /// <returns>The serialized string.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public static string Serialize(
        this IBmgSerializer serializer,
        IDictionary<string, BmgFile> files
    )
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));
        ArgumentNullException.ThrowIfNull(files, nameof(files));
#else
        if (serializer is null)
            throw new ArgumentNullException(nameof(serializer));
        if (files is null)
            throw new ArgumentNullException(nameof(files));
#endif

        using var writer = new StringWriter();
        serializer.Serialize(writer, files);
        return writer.ToString();
    }
}
