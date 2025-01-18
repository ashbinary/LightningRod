using LightningRod.Libraries.Byml;

namespace LightningRod.Utilities;

public class BymlIterator
{
    public static double randomizationLevel;
    public Dictionary<Func<string, bool>, Action<string, BymlHashTable>> ruleKeys = [];

    public BymlIterator(double randLevel)
    {
        randomizationLevel = randLevel;
    }

    public dynamic ProcessBymlRoot(dynamic bymlRoot)
    {
        return ProcessNode(bymlRoot);
    }

    public IBymlNode ProcessNode(IBymlNode bymlNode)
    {
        switch (bymlNode.Id)
        {
            case BymlNodeId.Array:
                bymlNode = ProcessArray(bymlNode as BymlArrayNode);
                break;
            case BymlNodeId.Hash:
                bymlNode = ProcessHashTable(bymlNode as BymlHashTable);
                break;
            default:
                bymlNode = ProcessPrimitiveNode(bymlNode);
                break;
        }
        return bymlNode;
    }

    public BymlArrayNode ProcessArray(BymlArrayNode arrayNode)
    {
        for (int i = 0; i < arrayNode.Length; i++)
            arrayNode.SetNodeAtIdx(ProcessNode(arrayNode[i]), i);
        return arrayNode;
    }

    public virtual BymlHashTable ProcessHashTable(BymlHashTable hashTable)
    {
        foreach (BymlHashPair node in hashTable.Pairs)
        {
            foreach (var rule in ruleKeys)
                if (rule.Key(node.Name)) 
                {
                    Logger.Log($"Found special case for {node.Name}");
                    rule.Value(node.Name, hashTable);
                }
                else
                    hashTable.SetNode(node.Name, ProcessNode(node.Value));
        }
        return hashTable;
    }

    public virtual IBymlNode ProcessPrimitiveNode(dynamic dataNode)
    {
        switch (dataNode.Id)
        {
            case BymlNodeId.Int:
                dataNode = ProcessIntNode(dataNode);
                break;
            case BymlNodeId.Float:
                dataNode.Data *= GameData.Random.NextFloat((float)randomizationLevel) + 0.01;
                break;
            case BymlNodeId.Bool:
                dataNode.Data = GameData.Random.NextBoolean();
                break;
        }
        return dataNode;
    }

    public static IBymlNode ProcessIntNode(BymlNode<int> intNode)
    {
        if (intNode.Data == 0)
        {
            intNode.Data = GameData.Random.NextInt((int)randomizationLevel);
            return intNode;
        }
        
        intNode.Data *= GameData.Random.NextInt((int)randomizationLevel) + 1;
        return intNode;
    }
}
