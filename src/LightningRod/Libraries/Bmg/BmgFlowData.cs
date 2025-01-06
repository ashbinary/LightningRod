namespace LightningRod.Libraries.Bmg;

/// <summary>
/// A class containing flow node data from FLW1/FLI1 sections.
/// </summary>
public class BmgFlowData
{
    public byte[][] Nodes { get; set; } = [];

    public byte[][] Labels { get; set; } = [];

    public byte[][] Indices { get; set; } = [];
}
