using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class for serializing a <see cref="MsbtFile"/> object to CSV.
/// </summary>
public class MsbtCsvSerializer : IMsbtSerializer
{
    #region public properties
    /// <summary>
    /// Gets or sets the separator character that should be used.
    /// The default value is '<c>,</c>'.
    /// </summary>
    public string Separator { get; set; } = ",";

    /// <summary>
    /// Determines whether to ignore message metadata in the output.
    /// This includes message index, attribute data, and style index.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SkipMessageMetadata { get; set; }
    #endregion

    #region IMsbtSerializer interface
    /// <inheritdoc/>
    public IMsbtTagMap TagMap { get; set; } = new MsbtDefaultTagMap();

    /// <inheritdoc/>
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtDefaultFormatProvider();

    /// <inheritdoc/>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public void Serialize(TextWriter writer, MsbtFile file)
    {
        if (string.IsNullOrEmpty(Separator)) throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('=')) throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(TagMap, nameof(TagMap));
        ArgumentNullException.ThrowIfNull(FormatProvider, nameof(FormatProvider));
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        ArgumentNullException.ThrowIfNull(file, nameof(file));
        #else
        if (TagMap is null) throw new ArgumentNullException(nameof(TagMap));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (file is null) throw new ArgumentNullException(nameof(file));
        #endif

        //write header
        var useIndex = file is {HasNli1: false, HasLbl1: false};
        if (useIndex)
        {
            writer.Write("Index");
            writer.Write(Separator);
        }
        else
        {
            if (file.HasNli1)
            {
                writer.Write("Id");
                writer.Write(Separator);
            }
            if (file.HasLbl1)
            {
                writer.Write("Label");
                writer.Write(Separator);
            }
        }
        if (!SkipMessageMetadata)
        {
            if (file.HasAtr1)
            {
                writer.Write("Attribute");
                writer.Write(Separator);
            }
            if (file.HasTsy1)
            {
                writer.Write("StyleIndex");
                writer.Write(Separator);
            }
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
                if (file.HasNli1)
                {
                    writer.Write(message.Id);
                    writer.Write(Separator);
                }
                if (file.HasLbl1)
                {
                    writer.Write(message.Label);
                    writer.Write(Separator);
                }
            }
            if (!SkipMessageMetadata)
            {
                if (file.HasAtr1)
                {
                    writer.Write(message.AttributeText ?? message.Attribute.ToHexString(true));
                    writer.Write(Separator);
                }
                if (file.HasTsy1)
                {
                    writer.Write(message.StyleIndex.ToString());
                    writer.Write(Separator);
                }
            }

            writer.Write(Separator);
            var text = message.ToCompiledString(TagMap, FormatProvider, file.BigEndian, file.Encoding);
            var wrapText = text.Contains(Separator) || text.Contains('\n');
            if (wrapText && text.Contains('"')) text = text.Replace("\"", "\"\"");
            writer.WriteLine(wrapText ? '"' + text + '"' : text);
        }

        writer.Flush();
        writer.Close();
    }

    /// <inheritdoc/>
    /// <exception cref="FormatException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public void Serialize(TextWriter writer, IDictionary<string, MsbtFile> files)
    {
        if (string.IsNullOrEmpty(Separator)) throw new FormatException("CSV separator cannot be empty.");
        if (Separator.Contains('=')) throw new FormatException($"\"{Separator}\" cannot be used as CSV separator.");
        #if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(TagMap, nameof(TagMap));
        ArgumentNullException.ThrowIfNull(FormatProvider, nameof(FormatProvider));
        ArgumentNullException.ThrowIfNull(writer, nameof(writer));
        ArgumentNullException.ThrowIfNull(files, nameof(files));
        #else
        if (TagMap is null) throw new ArgumentNullException(nameof(TagMap));
        if (FormatProvider is null) throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null) throw new ArgumentNullException(nameof(writer));
        if (files is null) throw new ArgumentNullException(nameof(files));
        #endif

        //sort languages
        var languages = files.Keys.ToArray();
        Array.Sort(languages);
        var firstFile = files.Values.First();

        //merge messages by id or label
        var mergedMessages = new MsbtMessage[firstFile.Messages.Count][];
        Array.Fill(mergedMessages, new MsbtMessage[languages.Length]);
        foreach (var (language, file) in files)
        {
            var index = Array.IndexOf(languages, language);
            var sortedMessages = file.Messages.ToList();
            if (firstFile.HasNli1) sortedMessages.Sort((m1, m2) => m1.Id.CompareTo(m2.Id));
            else if (firstFile.HasLbl1) sortedMessages.Sort((m1, m2) => string.CompareOrdinal(m1.Label, m2.Label));

            for (var i = 0; i < sortedMessages.Count; ++i)
            {
                mergedMessages[i][index] = sortedMessages[i];
            }
        }

        //write header
        var useIndex = firstFile is {HasNli1: false, HasLbl1: false};
        if (useIndex)
        {
            writer.Write("Index");
            writer.Write(Separator);
        }
        else
        {
            if (firstFile.HasNli1)
            {
                writer.Write("Id");
                writer.Write(Separator);
            }
            if (firstFile.HasLbl1)
            {
                writer.Write("Label");
                writer.Write(Separator);
            }
        }
        if (!SkipMessageMetadata)
        {
            if (firstFile.HasAtr1)
            {
                writer.Write("Attribute");
                writer.Write(Separator);
            }
            if (firstFile.HasTsy1)
            {
                writer.Write("StyleIndex");
                writer.Write(Separator);
            }
        }
        for (var i = 0; i < languages.Length; ++i)
        {
            if (i > 0) writer.Write(Separator);
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
                if (firstFile.HasNli1)
                {
                    writer.Write(messages[0].Id);
                    writer.Write(Separator);
                }
                if (firstFile.HasLbl1)
                {
                    writer.Write(messages[0].Label);
                    writer.Write(Separator);
                }
            }
            if (!SkipMessageMetadata)
            {
                if (firstFile.HasAtr1)
                {
                    writer.Write(Separator);
                    writer.Write(messages[0].AttributeText ?? messages[0].Attribute.ToHexString(true));
                }
                if (firstFile.HasTsy1)
                {
                    writer.Write(Separator);
                    writer.Write(messages[0].StyleIndex.ToString());
                }
            }

            for (var j = 0; j < languages.Length; ++j)
            {
                if (j > 0) writer.Write(Separator);

                var text = messages[j].ToCompiledString(TagMap, FormatProvider, firstFile.BigEndian, firstFile.Encoding);
                var wrapText = text.Contains(Separator) || text.Contains('\n');
                if (wrapText && text.Contains('"')) text = text.Replace("\"", "\"\"");
                writer.Write(wrapText ? '"' + text + '"' : text);
            }

            writer.WriteLine();
        }

        writer.Flush();
        writer.Close();
    }
    #endregion
}