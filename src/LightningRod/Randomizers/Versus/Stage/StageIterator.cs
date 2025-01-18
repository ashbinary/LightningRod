using LightningRod.Libraries.Byml;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Versus.Stage;

public class StageIterator(double randLevel) : BymlIterator(randLevel)
{
    public List<string> editedKeys = [];

    public override BymlHashTable ProcessHashTable(BymlHashTable hashTable)
    {
        List<string> tempKeys = editedKeys;
        foreach (BymlHashPair node in hashTable.Pairs)
        {
            if (node.Id == BymlNodeId.Array)
                hashTable.SetNode(node.Name, ProcessStageNode(node.Value, node.Name));
            tempKeys.Remove(node.Name);
        }

        foreach (string key in tempKeys)
        {
            float defaultValue = key == "Scale" ? 1.0f : 0.0f;
            hashTable.AddNode(BymlNodeId.Array, ProcessStageNode(new BymlArrayNode()
            {
                Array = {
                    new BymlNode<float>(BymlNodeId.Float, defaultValue),
                    new BymlNode<float>(BymlNodeId.Float, defaultValue),
                    new BymlNode<float>(BymlNodeId.Float, defaultValue)
                }
            }, key), key);
            Console.WriteLine("girl whatever");
        }

        return hashTable;
    }

    public IBymlNode ProcessStageNode(dynamic dataNode, string positionType)
    {
        float randomBase = 1.0f;
        float randomRange = (float)Options.GetOption("tweakLevel") / 100 * 2;

        if (!editedKeys.Contains(positionType)) return dataNode;

        switch (positionType)
        {
            case "Translate": 
            case "Rotate":
                // Since randomBase is already sent to 1.0f, nothing is needed. Just need to make sure it won't return.
                break;
            case "Scale": 
                randomBase = 1.0f;
                break;
            default: 
                return dataNode;
        }
        randomBase -= (float)Options.GetOption("tweakLevel") / 100;

        for (int i = 0; i < 3; i++)
        {
            float randomValue = dataNode[i].Data * ((GameData.Random.NextFloat() * randomRange) + randomBase);
            BymlNode<float> newBymlNode = new(BymlNodeId.Float, randomValue);

            dataNode.SetNodeAtIdx(newBymlNode, i);
        }

        return dataNode;
    }
}