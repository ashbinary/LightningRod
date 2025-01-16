using LightningRod.Libraries.Byml;

namespace LightningRod.Utilities;

public static class StageIterator
{
    public static BymlArrayNode RandomizePositions(
        this BymlArrayNode node,
        (float startPoint, float changePoint) dataPoints,
        float[] randInfo
    )
    {
        float basePoint = dataPoints.startPoint - dataPoints.changePoint;
        float modPoint = dataPoints.changePoint * 2;

        bool isNewNode = false;

        if (node.Length < 1)
            isNewNode = true;

        // RNG is handled as (random * 0.02) + 0.99 (example)
        // provides random of 0.99 - 1.01
        // (1, 0.01) in dataPoints does this
        for (int i = 0; i < randInfo.Length; i++)
        {
            if (isNewNode)
                node.AddNodeToArray(
                    new BymlNode<float>(
                        BymlNodeId.Float,
                        (float)((randInfo[i] * modPoint) + basePoint)
                    )
                );
            else
                (node[i] as BymlNode<float>).Data =
                    (float)((randInfo[i] * modPoint) + basePoint)
                    * (node[i] as BymlNode<float>).Data;
        }
        return node;
    }

    public static void RandomizeStageActors(ref BymlArrayNode actorList, List<string> positionData)
    {
        foreach (BymlHashTable actorData in actorList.Array)
        {
            if ((actorData["Name"] as BymlNode<string>).Data == "StartPos")
                continue;
            if (Options.GetOption("tweakStageLayouts"))
            {
                foreach (string positionType in positionData)
                {
                    if (actorData.ContainsKey(positionType))
                    {
                        ((BymlArrayNode)actorData[positionType]).RandomizePositions(
                            (1.0f, (float)Options.GetOption("tweakLevel") / 100),
                            GameData.Random.NextFloatArray(3)
                        );
                    }
                    else
                    {
                        BymlArrayNode positionNode = new BymlArrayNode();
                        (float, float) dataPoints = positionType.Contains("Scale")
                            ? (1.0f, (float)Options.GetOption("tweakLevel") / 100)
                            : (0.0f, (float)Options.GetOption("tweakLevel") / 100);
                        positionNode.RandomizePositions(
                            dataPoints,
                            GameData.Random.NextFloatArray(3)
                        );
                        actorData.AddNode(BymlNodeId.Array, positionNode, positionType);
                    }
                }
            }
        }
    }
}