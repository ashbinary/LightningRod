namespace LightningRod.Libraries;

/// <summary>
/// A class holding alignment information about an archive file.
/// </summary>
public class AlignmentInfo
{
    /// <summary>
    /// The name of the file.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// The absolute start position of the file data.
    /// </summary>
    public required long DataStart { get; set; }

    /// <summary>
    /// The absolute end position of the file data.
    /// </summary>
    public required long DataEnd { get; set; }

    /// <summary>
    /// The length of the file data.
    /// </summary>
    public long DataLength => DataEnd - DataStart;

    /// <summary>
    /// The padding between the previous file data and the start of this file data.
    /// </summary>
    public required long Padding { get; set; }

    /// <summary>
    /// The used alignment value from a <see cref="AlignmentTable"/>.
    /// </summary>
    public required int Alignment { get; set; }

    /// <summary>
    /// The expected/aligned start position of the file data.
    /// </summary>
    public required long ExpectedDataStart { get; set; }

    /// <summary>
    /// Determines whether the used alignment value matches the data start position.
    /// </summary>
    public bool IsValid => DataStart == ExpectedDataStart;
}
