using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using NintendoTools.Utils;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A class for serializing a <see cref="BmgFile"/> object to XML.
/// </summary>
public class BmgXmlSerializer : IBmgSerializer
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
    public string RootNode { get; set; } = "bmg";

    /// <summary>
    /// Determines whether to skip BMG metadata like version and encoding.
    /// The default value is <see langword="false"/>.
    /// </summary>
    public bool SkipMetadata { get; set; }

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
    public IBmgFormatProvider FormatProvider { get; set; } = new BmgXmlFormatProvider();

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
            xmlWriter.WriteAttributeString("encoding", file.Encoding.WebName);
            xmlWriter.WriteAttributeString("fileId", file.FileId.ToString());
            xmlWriter.WriteAttributeString("defaultColor", file.DefaultColor.ToString());
            xmlWriter.WriteAttributeString("hasMid1", file.HasMid1.ToString());
            if (file.HasMid1)
                xmlWriter.WriteAttributeString("mid1Format", file.Mid1Format.ToHexString(true));
            xmlWriter.WriteAttributeString("hasStr1", file.HasStr1.ToString());
        }

        foreach (var message in file.Messages)
        {
            xmlWriter.WriteStartElement("message");
            if (file.HasMid1)
                xmlWriter.WriteAttributeString("id", message.Id.ToString());
            if (file.HasStr1)
                xmlWriter.WriteAttributeString("label", message.Label);
            if (!IgnoreAttributes)
                xmlWriter.WriteAttributeString("attribute", message.Attribute.ToHexString(true));
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
    public void Serialize(TextWriter writer, IDictionary<string, BmgFile> files)
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
            xmlWriter.WriteAttributeString("encoding", firstFile.Encoding.WebName);
            xmlWriter.WriteAttributeString("fileId", firstFile.FileId.ToString());
            xmlWriter.WriteAttributeString("defaultColor", firstFile.DefaultColor.ToString());
            xmlWriter.WriteAttributeString("hasMid1", firstFile.HasMid1.ToString());
            if (firstFile.HasMid1)
                xmlWriter.WriteAttributeString(
                    "mid1Format",
                    firstFile.Mid1Format.ToHexString(true)
                );
            xmlWriter.WriteAttributeString("hasStr1", firstFile.HasStr1.ToString());
        }

        foreach (var messages in mergedMessages)
        {
            xmlWriter.WriteStartElement("message");
            if (firstFile.HasMid1)
                xmlWriter.WriteAttributeString("id", messages[0].Id.ToString());
            if (firstFile.HasStr1)
                xmlWriter.WriteAttributeString("label", messages[0].Label);
            if (!IgnoreAttributes)
                xmlWriter.WriteAttributeString(
                    "attribute",
                    messages[0].Attribute.ToHexString(true)
                );

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
