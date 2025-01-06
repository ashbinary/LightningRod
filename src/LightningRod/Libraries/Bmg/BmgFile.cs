using System.Collections.Generic;
using System.Text;

namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A class holding information about a BMG file.
/// </summary>
public class BmgFile
{
    static BmgFile() => Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

    /// <summary>
    /// Whether the file is encoded in big endian.
    /// </summary>
    public bool BigEndian { get; set; }

    /// <summary>
    /// Whether the file uses big endian magic and section labels.
    /// </summary>
    public bool BigEndianLabels { get; set; }

    /// <summary>
    /// The encoding used for the BMG file.
    /// </summary>
    public Encoding Encoding { get; set; } = Encoding.GetEncoding(1252);

    /// <summary>
    /// The ID of the BMG file. Usually unused.
    /// </summary>
    public int FileId { get; set; }

    /// <summary>
    /// The ID of the default text color.
    /// </summary>
    public int DefaultColor { get; set; }

    /// <summary>
    /// Whether the file contains a MID1 section.
    /// </summary>
    public bool HasMid1 { get; set; }

    /// <summary>
    /// Special MID1 format data.
    /// </summary>
    public byte[] Mid1Format { get; set; } = [];

    /// <summary>
    /// Whether the file contains a STR1 section.
    /// </summary>
    public bool HasStr1 { get; set; }

    /// <summary>
    /// Whether the file contains FLW1/FLI1 sections.
    /// </summary>
    public bool HasFlw1 { get; set; }

    /// <summary>
    /// The specific flow node data from FLW1/FLI1.
    /// </summary>
    public BmgFlowData? FlowData { get; set; }

    /// <summary>
    /// The list of messages in the BMG file.
    /// </summary>
    public List<BmgMessage> Messages { get; set; } = [];
}