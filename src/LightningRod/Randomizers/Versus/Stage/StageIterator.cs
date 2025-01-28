using LightningRod.Libraries.Byml;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Versus.Stage;

public class StageIterator(double randLevel) : BymlIterator(randLevel)
{
    public List<string> editedKeys = [];

    public override BymlHashTable ProcessHashTable(BymlHashTable hashTable)
    {
        List<string> tempKeys = [.. editedKeys]; // Deep copy to prevent issues with the original list
        foreach (BymlHashPair node in hashTable.Pairs)
        {
            if (node.Id == BymlNodeId.Array)
                hashTable.SetNode(node.Name, ProcessStageNode(node.Value, node.Name));
            tempKeys.Remove(node.Name);
        }

        foreach (string key in tempKeys)
        {
            float defaultValue = key switch
            {
                "Translate" or "Rotate" => 0.25f,
                "Scale" => 1.0f,
            };
            hashTable.AddNode(BymlNodeId.Array, ProcessStageNode(new BymlArrayNode()
            {
                Array = {
                    new BymlNode<float>(BymlNodeId.Float, defaultValue),
                    new BymlNode<float>(BymlNodeId.Float, defaultValue),
                    new BymlNode<float>(BymlNodeId.Float, defaultValue)
                }
            }, key), key);
        }

        return hashTable;
    }

    public IBymlNode ProcessStageNode(dynamic dataNode, string positionType)
    {
        if (!editedKeys.Contains(positionType)) return dataNode;

        float Max = positionType switch
        {
            "Translate" => 20f,
            "Rotate" =>  3.14159f,
            "Scale" => 0.8f,
        };

        for (int i = 0; i < 3; i++)
        {
            double cappedRand = Max * (randLevel / 100); 
            double randBase = GameData.Random.NextFloat((float)(cappedRand * 2)) - cappedRand;
            float randomValue = (float)(randBase + dataNode[i].Data);

            if (positionType == "Scale") randomValue = Math.Abs(randomValue); 
            BymlNode<float> newBymlNode = new(BymlNodeId.Float, randomValue);

            dataNode.SetNodeAtIdx(newBymlNode, i);
        }

        return dataNode;
    }
}