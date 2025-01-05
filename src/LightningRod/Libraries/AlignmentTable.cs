using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace NintendoTools.FileFormats;

/// <summary>
/// A table to map file extensions to archive data alignments.
/// </summary>
public class AlignmentTable : IEnumerable<KeyValuePair<string, int>>
{
    #region private members
    private readonly Dictionary<string, int> _alignment = new(StringComparer.OrdinalIgnoreCase);
    private int _defaultValue = 8;
    #endregion

    #region public properties
    /// <summary>
    /// Gets or sets the default alignment in bytes for file extensions not defined in the table.
    /// Defaults to 8.
    /// </summary>
    public int Default
    {
        get => _defaultValue;
        set
        {
            if (value == 0) _defaultValue = 1;
            _defaultValue = Math.Abs(value);
        }
    }

    /// <summary>
    /// Gets the number of alignment mappings in this table.
    /// </summary>
    public int Count => _alignment.Count;
    #endregion

    #region public methods
    /// <summary>
    /// Adds a new alignment in bytes for a given file extension.
    /// File extensions are case-insensitive.
    /// </summary>
    /// <param name="extension">The file extension to add.</param>
    /// <param name="alignment">The alignment in bytes.</param>
    /// <returns><see langword="true"/> if the alignment was added successfully; otherwise <see langword="false"/>.</returns>
    /// <exception cref="ArgumentNullException"></exception>
    public bool Add(string extension, int alignment)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(extension, nameof(extension));
        #else
        if (extension is null) throw new ArgumentNullException(nameof(extension));
        #endif

        return _alignment.TryAdd(extension.StartsWith('.') ? extension : '.' + extension, alignment == 0 ? 1 : Math.Abs(alignment));
    }

    /// <summary>
    /// Determines whether an alignment exists for a given file extension.
    /// </summary>
    /// <param name="extension">The file extension to find.</param>
    /// <returns><see langword="true"/> if the alignment was found; otherwise <see langword="false"/>.</returns>
    public bool Contains(string extension) => _alignment.ContainsKey(extension);

    /// <summary>
    /// Removes the alignment for a given file extension.
    /// </summary>
    /// <param name="extension">The file extension to remove.</param>
    /// <returns><see langword="true"/> if the alignment was found and removed successfully; otherwise <see langword="false"/>.</returns>
    public bool Remove(string extension) => _alignment.Remove(extension.StartsWith('.') ? extension : '.' + extension);

    /// <summary>
    /// Removes the alignments for all file extensions.
    /// </summary>
    public void Clear() => _alignment.Clear();

    /// <summary>
    /// Gets the alignment in bytes for a given file extension.
    /// Falls back to the value specified in <see cref="Default"/> if the extension was not found.
    /// </summary>
    /// <param name="extension">The file extension to find.</param>
    /// <returns>The alignment in bytes for that file extension.</returns>
    public int Get(string extension) => _alignment.GetValueOrDefault(extension, Default);

    /// <summary>
    /// Attempts to get the alignment in bytes for a given file extension.
    /// </summary>
    /// <param name="extension">The file extension to find.</param>
    /// <param name="alignment">The alignment in bytes for that file extension.</param>
    /// <returns><see langword="true"/> if the alignment was found; otherwise <see langword="false"/>.</returns>
    public bool TryGet(string extension, out int alignment) => _alignment.TryGetValue(extension, out alignment);

    /// <summary>
    /// Gets the alignment in bytes for a given file name. Each '.' in the name will be considered as extension.
    /// Falls back to the value specified in <see cref="Default"/> if the extension was not found.
    /// </summary>
    /// <param name="fileName">The file name to check.</param>
    /// <returns>The alignment in bytes for that file extension.</returns>
    public int GetFromName(string fileName)
    {
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(fileName, nameof(fileName));
        #else
        if (fileName is null) throw new ArgumentNullException(nameof(fileName));
        #endif

        fileName = Path.GetFileName(fileName);

        var index = fileName.IndexOf('.');
        while (index > -1)
        {
            if (_alignment.TryGetValue(fileName[index..], out var value))
            {
                return value;
            }
            index = fileName.IndexOf('.', index + 1);
        }

        return Default;
    }
    #endregion

    #region IEnumerable interface
    /// <inheritdoc/>
    public IEnumerator<KeyValuePair<string, int>> GetEnumerator() => _alignment.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    #endregion
}