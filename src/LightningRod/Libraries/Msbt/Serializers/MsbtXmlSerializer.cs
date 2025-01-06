using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Msbt;

/// <summary>
/// A class for serializing a <see cref="MsbtFile"/> object to XML.
/// </summary>
public class MsbtXmlSerializer : IMsbtSerializer
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
    /// Gets or sets the name of the root node.
    /// </summary>
    public string RootNode { get; set; } = "msbt";

    /// <summary>
    /// Determines whether to skip MSBT metadata like version and encoding.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SkipMetadata { get; set; }

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
    public IMsbtFormatProvider FormatProvider { get; set; } = new MsbtXmlFormatProvider();

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
        if (TagMap is null)
            throw new ArgumentNullException(nameof(TagMap));
        if (FormatProvider is null)
            throw new ArgumentNullException(nameof(FormatProvider));
        if (writer is null)
            throw new ArgumentNullException(nameof(writer));
        if (file is null)
            throw new ArgumentNullException(nameof(file));
#endif

        using var xmlWriter = new XmlTextWriter(writer);

        if (Indentation > 0)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = Indentation;
            xmlWriter.IndentChar = IndentChar;
        }
        else
            xmlWriter.Formatting = Formatting.None;

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement(RootNode);

        if (!SkipMetadata) //write meta data
        {
            xmlWriter.WriteAttributeString("bigEndian", file.BigEndian.ToString());
            xmlWriter.WriteAttributeString("version", file.Version.ToString());
            xmlWriter.WriteAttributeString("encoding", file.Encoding.WebName);
            xmlWriter.WriteAttributeString("hasNli1", file.HasNli1.ToString());
            xmlWriter.WriteAttributeString("hasLbl1", file.HasLbl1.ToString());
            xmlWriter.WriteAttributeString("hasAtr1", file.HasAtr1.ToString());
            if (file.AdditionalAttributeData.Length > 0)
                xmlWriter.WriteAttributeString(
                    "additionalAttributeData",
                    file.AdditionalAttributeData.ToHexString(true)
                );
            xmlWriter.WriteAttributeString("hasTsy1", file.HasTsy1.ToString());
        }

        foreach (var message in file.Messages)
        {
            xmlWriter.WriteStartElement("message");
            if (file.HasNli1)
                xmlWriter.WriteAttributeString("id", message.Id.ToString());
            if (file.HasLbl1)
                xmlWriter.WriteAttributeString("label", message.Label);
            if (!SkipMessageMetadata)
            {
                if (file.HasAtr1)
                {
                    xmlWriter.WriteAttributeString(
                        "attribute",
                        message.AttributeText ?? message.Attribute.ToHexString(true)
                    );
                }
                if (file.HasTsy1)
                {
                    xmlWriter.WriteAttributeString("styleIndex", message.StyleIndex.ToString());
                }
            }
            xmlWriter.WriteRaw(
                message.ToCompiledString(TagMap, FormatProvider, file.BigEndian, file.Encoding)
            );
            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        xmlWriter.Flush();
        xmlWriter.Close();
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
        var mergedMessages = new MsbtMessage[firstFile.Messages.Count][];
        Array.Fill(mergedMessages, new MsbtMessage[languages.Length]);
        foreach (var (language, file) in files)
        {
            var index = Array.IndexOf(languages, language);
            var sortedMessages = file.Messages.ToList();
            if (firstFile.HasNli1)
                sortedMessages.Sort((m1, m2) => m1.Id.CompareTo(m2.Id));
            else if (firstFile.HasLbl1)
                sortedMessages.Sort((m1, m2) => string.CompareOrdinal(m1.Label, m2.Label));

            for (var i = 0; i < sortedMessages.Count; ++i)
            {
                mergedMessages[i][index] = sortedMessages[i];
            }
        }

        using var xmlWriter = new XmlTextWriter(writer);

        if (Indentation > 0)
        {
            xmlWriter.Formatting = Formatting.Indented;
            xmlWriter.Indentation = Indentation;
            xmlWriter.IndentChar = IndentChar;
        }
        else
            xmlWriter.Formatting = Formatting.None;

        xmlWriter.WriteStartDocument();
        xmlWriter.WriteStartElement(RootNode);

        if (!SkipMetadata) //write meta data
        {
            xmlWriter.WriteAttributeString("bigEndian", firstFile.BigEndian.ToString());
            xmlWriter.WriteAttributeString("version", firstFile.Version.ToString());
            xmlWriter.WriteAttributeString("encoding", firstFile.Encoding.WebName);
            xmlWriter.WriteAttributeString("hasNli1", firstFile.HasNli1.ToString());
            xmlWriter.WriteAttributeString("hasLbl1", firstFile.HasLbl1.ToString());
            xmlWriter.WriteAttributeString("hasAtr1", firstFile.HasAtr1.ToString());
            if (firstFile.AdditionalAttributeData.Length > 0)
                xmlWriter.WriteAttributeString(
                    "additionalAttributeData",
                    firstFile.AdditionalAttributeData.ToHexString(true)
                );
            xmlWriter.WriteAttributeString("hasTsy1", firstFile.HasTsy1.ToString());
        }

        foreach (var messages in mergedMessages)
        {
            xmlWriter.WriteStartElement("message");
            if (firstFile.HasNli1)
                xmlWriter.WriteAttributeString("id", messages[0].Id.ToString());
            if (firstFile.HasLbl1)
                xmlWriter.WriteAttributeString("label", messages[0].Label);

            if (!SkipMessageMetadata)
            {
                if (firstFile.HasAtr1)
                {
                    xmlWriter.WriteAttributeString(
                        "attribute",
                        messages[0].AttributeText ?? messages[0].Attribute.ToHexString(true)
                    );
                }
                if (firstFile.HasTsy1)
                {
                    xmlWriter.WriteAttributeString("styleIndex", messages[0].StyleIndex.ToString());
                }
            }

            for (var i = 0; i < languages.Length; ++i)
            {
                xmlWriter.WriteStartElement("language");
                xmlWriter.WriteAttributeString("type", languages[i]);
                xmlWriter.WriteRaw(
                    messages[i]
                        .ToCompiledString(
                            TagMap,
                            FormatProvider,
                            firstFile.BigEndian,
                            firstFile.Encoding
                        )
                );
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        xmlWriter.WriteEndElement();
        xmlWriter.WriteEndDocument();

        xmlWriter.Flush();
        xmlWriter.Close();
    }
    #endregion
}
