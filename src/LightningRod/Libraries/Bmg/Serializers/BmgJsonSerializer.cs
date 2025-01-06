using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A class for serializing a <see cref="BmgFile"/> object to JSON.
/// </summary>
public class BmgJsonSerializer : IBmgSerializer
{
    #region public properties
    /// <summary>
    /// Gets or sets number of indentation characters that should be used.
    /// '<c>0</c>' disables indentation.
    /// The default value is <c>2</c>.
    /// </summary>
    public int Indentation { get; set; } = 2;

    /// <summary>
    /// Gets or sets the indentation character that should be used.
    /// The default value is '<c> </c>'.
    /// </summary>
    public char IndentChar { get; set; } = ' ';

    /// <summary>
    /// Determines whether to skip BMG metadata like version and encoding.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SkipMetadata { get; set; }

    /// <summary>
    /// Determines whether the serialized result should be an object where each message is a property instead of an array of message objects.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool WriteAsObject { get; set; }

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
    public IBmgFormatProvider FormatProvider { get; set; } = new BmgJsonFormatProvider();

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public void Serialize(TextWriter writer, BmgFile file)
    {
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

        using var jsonWriter = new JsonTextWriter(writer);

        if (Indentation > 0)
        {
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = Indentation;
            jsonWriter.IndentChar = IndentChar;
        }
        else jsonWriter.Formatting = Formatting.None;

        if (!SkipMetadata) //write meta data
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("bigEndian");
            jsonWriter.WriteValue(file.BigEndian);
            jsonWriter.WritePropertyName("encoding");
            jsonWriter.WriteValue(file.Encoding.WebName);
            jsonWriter.WritePropertyName("fileId");
            jsonWriter.WriteValue(file.FileId);
            jsonWriter.WritePropertyName("defaultColor");
            jsonWriter.WriteValue(file.DefaultColor);
            jsonWriter.WritePropertyName("hasMid1");
            jsonWriter.WriteValue(file.HasMid1);
            if (file.HasMid1)
            {
                jsonWriter.WritePropertyName("mid1Format");
                jsonWriter.WriteValue(file.Mid1Format.ToHexString(true));
            }
            jsonWriter.WritePropertyName("hasStr1");
            jsonWriter.WriteValue(file.HasStr1);
            jsonWriter.WritePropertyName("messages");
        }

        if (WriteAsObject) //write one big object, only containing labels and texts
        {
            jsonWriter.WriteStartObject();

            for (var i = 0; i < file.Messages.Count; ++i)
            {
                var message = file.Messages[i];
                jsonWriter.WritePropertyName(file.HasMid1 ? message.Id.ToString() : file.HasStr1 ? message.Label : i.ToString());
                jsonWriter.WriteValue(message.ToCompiledString(TagMap, FormatProvider, file.BigEndian, file.Encoding));
            }

            jsonWriter.WriteEndObject();
        }
        else //write array of full message objects
        {
            jsonWriter.WriteStartArray();

            foreach (var message in file.Messages)
            {
                jsonWriter.WriteStartObject();

                if (file.HasMid1)
                {
                    jsonWriter.WritePropertyName("id");
                    jsonWriter.WriteValue(message.Id);
                }

                if (file.HasStr1)
                {
                    jsonWriter.WritePropertyName("label");
                    jsonWriter.WriteValue(message.Label);
                }

                if (!IgnoreAttributes)
                {
                    jsonWriter.WritePropertyName("attribute");
                    jsonWriter.WriteValue(message.Attribute.ToHexString(true));
                }

                jsonWriter.WritePropertyName("text");
                jsonWriter.WriteValue(message.ToCompiledString(TagMap, FormatProvider, file.BigEndian, file.Encoding));

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
        }

        if (!SkipMetadata) jsonWriter.WriteEndObject();

        jsonWriter.Flush();
        jsonWriter.Close();
    }

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public void Serialize(TextWriter writer, IDictionary<string, BmgFile> files)
    {
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
        var mergedMessages = new BmgMessage[firstFile.Messages.Count][];
        Array.Fill(mergedMessages, new BmgMessage[languages.Length]);
        foreach (var (language, file) in files)
        {
            var index = Array.IndexOf(languages, language);
            var sortedMessages = file.Messages.ToList();
            if (firstFile.HasMid1) sortedMessages.Sort((m1, m2) => m1.Id.CompareTo(m2.Id));
            else if (firstFile.HasStr1) sortedMessages.Sort((m1, m2) => string.CompareOrdinal(m1.Label, m2.Label));

            for (var i = 0; i < sortedMessages.Count; ++i)
            {
                mergedMessages[i][index] = sortedMessages[i];
            }
        }

        using var jsonWriter = new JsonTextWriter(writer);

        if (Indentation > 0)
        {
            jsonWriter.Formatting = Formatting.Indented;
            jsonWriter.Indentation = Indentation;
            jsonWriter.IndentChar = IndentChar;
        }
        else jsonWriter.Formatting = Formatting.None;

        if (!SkipMetadata) //write meta data
        {
            jsonWriter.WriteStartObject();
            jsonWriter.WritePropertyName("bigEndian");
            jsonWriter.WriteValue(firstFile.BigEndian);
            jsonWriter.WritePropertyName("encoding");
            jsonWriter.WriteValue(firstFile.Encoding.WebName);
            jsonWriter.WritePropertyName("fileId");
            jsonWriter.WriteValue(firstFile.FileId);
            jsonWriter.WritePropertyName("defaultColor");
            jsonWriter.WriteValue(firstFile.DefaultColor);
            jsonWriter.WritePropertyName("hasMid1");
            jsonWriter.WriteValue(firstFile.HasMid1);
            if (firstFile.HasMid1)
            {
                jsonWriter.WritePropertyName("mid1Format");
                jsonWriter.WriteValue(firstFile.Mid1Format.ToHexString(true));
            }
            jsonWriter.WritePropertyName("hasStr1");
            jsonWriter.WriteValue(firstFile.HasStr1);
            jsonWriter.WritePropertyName("messages");
        }

        if (WriteAsObject)
        {
            jsonWriter.WriteStartObject();

            for (var i = 0; i < mergedMessages.Length; ++i)
            {
                var messages = mergedMessages[i];
                jsonWriter.WritePropertyName(firstFile.HasMid1 ? messages[0].Id.ToString() : firstFile.HasStr1 ? messages[0].Label : i.ToString());
                jsonWriter.WriteStartObject();

                if (!IgnoreAttributes)
                {
                    jsonWriter.WritePropertyName("attribute");
                    jsonWriter.WriteValue(messages[0].Attribute.ToHexString(true));
                    jsonWriter.WritePropertyName("locale");
                    jsonWriter.WriteStartObject();
                }

                for (var j = 0; j < languages.Length; ++j)
                {
                    jsonWriter.WritePropertyName(languages[j]);
                    jsonWriter.WriteValue(messages[j].ToCompiledString(TagMap, FormatProvider, firstFile.BigEndian, firstFile.Encoding));
                }

                if (!IgnoreAttributes) jsonWriter.WriteEndObject();

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndObject();
        }
        else
        {
            jsonWriter.WriteStartArray();

            foreach (var messages in mergedMessages)
            {
                jsonWriter.WriteStartObject();

                if (firstFile.HasMid1)
                {
                    jsonWriter.WritePropertyName("id");
                    jsonWriter.WriteValue(messages[0].Id);
                }

                if (firstFile.HasStr1)
                {
                    jsonWriter.WritePropertyName("label");
                    jsonWriter.WriteValue(messages[0].Label);
                }

                if (!IgnoreAttributes)
                {
                    jsonWriter.WritePropertyName("attribute");
                    jsonWriter.WriteValue(messages[0].Attribute.ToHexString(true));
                }

                jsonWriter.WritePropertyName("locale");
                jsonWriter.WriteStartObject();
                for (var i = 0; i < languages.Length; ++i)
                {
                    jsonWriter.WritePropertyName(languages[i]);
                    jsonWriter.WriteValue(messages[i].ToCompiledString(TagMap, FormatProvider, firstFile.BigEndian, firstFile.Encoding));
                }
                jsonWriter.WriteEndObject();

                jsonWriter.WriteEndObject();
            }

            jsonWriter.WriteEndArray();
        }

        if (!SkipMetadata) jsonWriter.WriteEndObject();

        jsonWriter.Flush();
        jsonWriter.Close();
    }
    #endregion
}