using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A class for serializing a <see cref="BmgFile"/> object to CSV.
/// </summary>
public class BmgCsvSerializer : IBmgSerializer
{
    #region public properties
    /// <summary>
    /// Gets or sets the separator character that should be used.
    /// The default value is '<c>,</c>'.
    /// </summary>
    public string Separator { get; set; } = ",";

    /// <summary>
    /// Determines whether to ignore attribute values in the output.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool IgnoreAttributes { get; set; }
    #endregion

    #region IMsbtSerializer interface
    /// <inheritdoc/>
    public IBmgTagMap TagMap { get; set; } = new BmgDefaultTagMap();

    /// <inheritdoc/>
    public IBmgFormatProvider FormatProvider { get; set; } = new BmgDefaultFormatProvider();

    /// <inheritdoc/>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public void Serialize(TextWriter writer, BmgFile file)
    {
        if (string.IsNullOrEmpty(Separator))
            throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('='))
            throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(TagMap, nameof(TagMap));
        ArgumentNullException.ThrowIfNull(FormatProvider, nameof(FormatProvider));
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        ArgumentNullException.ThrowIfNull(file, nameof(file));
#else
        if (TagMap is null)
            throw new ArgumentNullException(nameof(TagMap));
        if (FormatProvider is null)
            throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));
        if (file is null)
            throw new ArgumentNullException(nameof(file));
#endif

        //write header
        var useIndex = file is { HasMid1: false, HasStr1: false };
        if (useIndex)
        {
            writer.Write("Index");
            writer.Write(Separator);
        }
        else
        {
            if (file.HasMid1)
            {
                writer.Write("Id");
                writer.Write(Separator);
            }
            if (file.HasStr1)
            {
                writer.Write("Label");
                writer.Write(Separator);
            }
        }
        if (!IgnoreAttributes)
        {
            writer.Write("Attribute");
            writer.Write(Separator);
        }
        writer.WriteLine("Text");

        //write messages
        for (var i = 0; i < file.Messages.Count; ++i)
        {
            var message = file.Messages[i];

            if (useIndex)
            {
                writer.Write(i);
                writer.Write(Separator);
            }
            else
            {
                if (file.HasMid1)
                {
                    writer.Write(message.Id);
                    writer.Write(Separator);
                }
                if (file.HasStr1)
                {
                    writer.Write(message.Label);
                    writer.Write(Separator);
                }
            }
            if (!IgnoreAttributes)
            {
                writer.Write(message.Attribute.ToHexString(true));
                writer.Write(Separator);
            }

            var text = message.ToCompiledString(
                TagMap,
                FormatProvider,
                file.BigEndian,
                file.Encoding
            );
            var wrapText = text.Contains(Separator) || text.Contains('\n');
            if (wrapText && text.Contains('"'))
                text = text.Replace("\"", "\"\"");
            writer.WriteLine(wrapText ? '"' + text + '"' : text);
        }

        writer.Flush();
        writer.Close();
    }

    /// <inheritdoc/>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public void Serialize(TextWriter writer, IDictionary<string, BmgFile> files)
    {
        if (string.IsNullOrEmpty(Separator))
            throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('='))
            throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(TagMap, nameof(TagMap));
        ArgumentNullException.ThrowIfNull(FormatProvider, nameof(FormatProvider));
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        ArgumentNullException.ThrowIfNull(files, nameof(files));
#else
        if (TagMap is null)
            throw new ArgumentNullException(nameof(TagMap));
        if (FormatProvider is null)
            throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));
        if (files is null)
            throw new ArgumentNullException(nameof(files));
#endif

        //sort languages
        var languages = files.Keys.ToArray();
        Array.Sort(languages);
        var firstFile = files.Values.First();

        //merge messages by id or label
        var mergedMessages = new BmgMessage[firstFile.Messages.Count][];
        Array.Fill(mergedMessages, new BmgMessage[languages.Length]);
        foreach (var (language, file) in files)
        {
            var index = Array.IndexOf(languages, language);
            var sortedMessages = file.Messages.ToList();
            if (firstFile.HasMid1)
                sortedMessages.Sort((m1, m2) => m1.Id.CompareTo(m2.Id));
            else if (firstFile.HasStr1)
                sortedMessages.Sort((m1, m2) => string.CompareOrdinal(m1.Label, m2.Label));

            for (var i = 0; i < sortedMessages.Count; ++i)
            {
                mergedMessages[i][index] = sortedMessages[i];
            }
        }

        //write header
        var useIndex = firstFile is { HasMid1: false, HasStr1: false };
        if (useIndex)
        {
            writer.Write("Index");
            writer.Write(Separator);
        }
        else
        {
            if (firstFile.HasMid1)
            {
                writer.Write("Id");
                writer.Write(Separator);
            }
            if (firstFile.HasStr1)
            {
                writer.Write("Label");
                writer.Write(Separator);
            }
        }
        if (!IgnoreAttributes)
        {
            writer.Write("Attribute");
            writer.Write(Separator);
        }
        for (var i = 0; i < languages.Length; ++i)
        {
            if (i > 0)
                writer.Write(Separator);
            writer.Write(languages[i]);
        }
        writer.WriteLine();

        //write messages
        for (var i = 0; i < mergedMessages.Length; ++i)
        {
            var messages = mergedMessages[i];

            if (useIndex)
            {
                writer.Write(i);
                writer.Write(Separator);
            }
            else
            {
                if (firstFile.HasMid1)
                {
                    writer.Write(messages[0].Id);
                    writer.Write(Separator);
                }
                if (firstFile.HasStr1)
                {
                    writer.Write(messages[0].Label);
                    writer.Write(Separator);
                }
            }
            if (!IgnoreAttributes)
            {
                writer.Write(messages[0].Attribute.ToHexString(true));
                writer.Write(Separator);
            }

            for (var j = 0; j < languages.Length; ++j)
            {
                if (j > 0)
                    writer.Write(Separator);

                var text = messages[j]
                    .ToCompiledString(
                        TagMap,
                        FormatProvider,
                        firstFile.BigEndian,
                        firstFile.Encoding
                    );
                var wrapText = text.Contains(Separator) || text.Contains('\n');
                if (wrapText && text.Contains('"'))
                    text = text.Replace("\"", "\"\"");
                writer.Write(wrapText ? '"' + text + '"' : text);
            }

            writer.WriteLine();
        }

        writer.Flush();
        writer.Close();
    }
    #endregion
}
