using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class for serializing a <see cref="MsbtFile"/> object to JSON.
/// </summary>
public class MsbtJsonSerializer : IMsbtSerializer
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
    /// Determines whether to skip MSBT metadata like version and encoding.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SkipMetadata { get; set; }

    /// <summary>
    /// Determines whether the serialized result should be an object where each message is a property instead of an array of message objects.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool WriteAsObject { get; set; }

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
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtJsonFormatProvider();

    /// <inheritdoc/>
    /// <exception cref="ArgumentNullException"></exception>
    public void Serialize(TextWriter writer, MsbtFile file)
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
            jsonWriter.WritePropertyName("version");
            jsonWriter.WriteValue(file.Version);
            jsonWriter.WritePropertyName("encoding");
            jsonWriter.WriteValue(file.Encoding.WebName);
            jsonWriter.WritePropertyName("hasNli1");
            jsonWriter.WriteValue(file.HasNli1);
            jsonWriter.WritePropertyName("hasLbl1");
            jsonWriter.WriteValue(file.HasLbl1);
            jsonWriter.WritePropertyName("hasAtr1");
            jsonWriter.WriteValue(file.HasAtr1);
            if (file.AdditionalAttributeData.Length > 0)
            {
                jsonWriter.WritePropertyName("additionalAttributeData");
                jsonWriter.WriteValue(file.AdditionalAttributeData.ToHexString(true));
            }
            jsonWriter.WritePropertyName("hasTsy1");
            jsonWriter.WriteValue(file.HasTsy1);
            jsonWriter.WritePropertyName("messages");
        }

        if (WriteAsObject) //write one big object, only containing labels and texts
        {
            jsonWriter.WriteStartObject();

            for (var i = 0; i < file.Messages.Count; ++i)
            {
                var message = file.Messages[i];
                jsonWriter.WritePropertyName(file.HasNli1 ? message.Id.ToString() : file.HasLbl1 ? message.Label : i.ToString());
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

                if (file.HasNli1)
                {
                    jsonWriter.WritePropertyName("id");
                    jsonWriter.WriteValue(message.Id);
                }

                if (file.HasLbl1)
                {
                    jsonWriter.WritePropertyName("label");
                    jsonWriter.WriteValue(message.Label);
                }

                if (!SkipMessageMetadata)
                {
                    if (file.HasAtr1)
                    {
                        jsonWriter.WritePropertyName("attribute");
                        jsonWriter.WriteValue(message.AttributeText ?? message.Attribute.ToHexString(true));
                    }
                    if (file.HasTsy1)
                    {
                        jsonWriter.WritePropertyName("styleIndex");
                        jsonWriter.WriteValue(message.StyleIndex);
                    }
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
    public void Serialize(TextWriter writer, IDictionary<string, MsbtFile> files)
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
            jsonWriter.WritePropertyName("version");
            jsonWriter.WriteValue(firstFile.Version);
            jsonWriter.WritePropertyName("encoding");
            jsonWriter.WriteValue(firstFile.Encoding.WebName);
            jsonWriter.WritePropertyName("hasNli1");
            jsonWriter.WriteValue(firstFile.HasNli1);
            jsonWriter.WritePropertyName("hasLbl1");
            jsonWriter.WriteValue(firstFile.HasLbl1);
            jsonWriter.WritePropertyName("hasAtr1");
            jsonWriter.WriteValue(firstFile.HasAtr1);
            if (firstFile.AdditionalAttributeData.Length > 0)
            {
                jsonWriter.WritePropertyName("additionalAttributeData");
                jsonWriter.WriteValue(firstFile.AdditionalAttributeData.ToHexString(true));
            }
            jsonWriter.WritePropertyName("hasTsy1");
            jsonWriter.WriteValue(firstFile.HasTsy1);
            jsonWriter.WritePropertyName("messages");
        }

        if (WriteAsObject)
        {
            jsonWriter.WriteStartObject();

            for (var i = 0; i < mergedMessages.Length; ++i)
            {
                var messages = mergedMessages[i];
                jsonWriter.WritePropertyName(firstFile.HasNli1 ? messages[0].Id.ToString() : firstFile.HasLbl1 ? messages[0].Label : i.ToString());
                jsonWriter.WriteStartObject();

                if (!SkipMetadata)
                {
                    if (firstFile.HasAtr1)
                    {
                        jsonWriter.WritePropertyName("attribute");
                        jsonWriter.WriteValue(messages[0].AttributeText ?? messages[0].Attribute.ToHexString(true));
                    }
                    if (firstFile.HasTsy1)
                    {
                        jsonWriter.WritePropertyName("styleIndex");
                        jsonWriter.WriteValue(messages[0].StyleIndex);
                    }
                }

                if (!SkipMetadata)
                {
                    jsonWriter.WritePropertyName("locale");
                    jsonWriter.WriteStartObject();
                }

                for (var j = 0; j < languages.Length; ++j)
                {
                    jsonWriter.WritePropertyName(languages[j]);
                    jsonWriter.WriteValue(messages[j].ToCompiledString(TagMap, FormatProvider, firstFile.BigEndian, firstFile.Encoding));
                }

                if (!SkipMetadata) jsonWriter.WriteEndObject();

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

                if (firstFile.HasNli1)
                {
                    jsonWriter.WritePropertyName("id");
                    jsonWriter.WriteValue(messages[0].Id);
                }

                if (firstFile.HasLbl1)
                {
                    jsonWriter.WritePropertyName("label");
                    jsonWriter.WriteValue(messages[0].Label);
                }

                if (!SkipMetadata)
                {
                    if (firstFile.HasAtr1)
                    {
                        jsonWriter.WritePropertyName("attribute");
                        jsonWriter.WriteValue(messages[0].AttributeText ?? messages[0].Attribute.ToHexString(true));
                    }
                    if (firstFile.HasTsy1)
                    {
                        jsonWriter.WritePropertyName("styleIndex");
                        jsonWriter.WriteValue(messages[0].StyleIndex);
                    }
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