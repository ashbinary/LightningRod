using LightningRod.Libraries.Byml;

namespace LightningRod.Utilities;

public class BymlIterator
{
    public BymlHashTable IterateParams(BymlHashTable paramFile)
    {
        if (!paramFile.ContainsKey("GameParameters"))
            return paramFile;

        BymlHashTable paramData = (BymlHashTable)paramFile["GameParameters"];
        paramData = (BymlHashTable)CheckType(paramData);
        paramFile.SetNode("GameParameters", paramData);
        return paramFile;
    }

    public IBymlNode CheckType(IBymlNode paramData)
    {
        BymlNodeId dataType = paramData.Id;
        switch (dataType)
        {
            case BymlNodeId.Hash:
                paramData = HandleHashTable((BymlHashTable)paramData);
                break;
            case BymlNodeId.Array:
                paramData = HandleArrayNode((BymlArrayNode)paramData);
                break;
            case BymlNodeId.Null:
                throw new Exception("Illegal byml node.");
            default:
                paramData = HandleValue(paramData);
                break;
        }

        return paramData;
    }

    public BymlHashTable HandleHashTable(BymlHashTable paramData)
    {
        foreach (string paramKey in paramData.Keys)
        {
            if (paramKey.Contains("InkConsume") && Options.GetOption("maxInkConsume"))
                paramData.SetNode(
                    paramKey,
                    new BymlNode<float>(BymlNodeId.Float, GameData.Random.NextFloat())
                );
            paramData.SetNode(paramKey, CheckType(paramData[paramKey]));
        }
        return paramData;
    }

    public BymlArrayNode HandleArrayNode(BymlArrayNode paramData)
    {
        for (int i = 0; i < paramData.Length; i++)
        {
            paramData.SetNodeAtIdx(CheckType(paramData[i]), i);
        }

        return paramData;
    }

    public static IBymlNode HandleValue(IBymlNode paramData)
    {
        dynamic typedParam = paramData;

        double[] severityValues = [1.5, 2.0, 3.0];
        float severity = (float)severityValues[Options.GetOption("parameterSeverity") - 1];

        switch (paramData.Id)
        {
            case BymlNodeId.Int:
                if (typedParam.Data > 0)
                    typedParam.Data =
                        GameData.Random.NextInt((int)(typedParam.Data * severity)) + 1;
                else if (typedParam.Data == 0)
                    typedParam.Data = GameData.Random.NextInt((int)severity); //unfortunate rare edge case
                else
                    typedParam.Data = -1 * GameData.Random.NextInt(Math.Abs(typedParam.Data));
                break;
            case BymlNodeId.Float:
                typedParam.Data = GameData.Random.NextFloat() * (typedParam.Data * severity);
                break;
            case BymlNodeId.Bool:
                typedParam.Data = GameData.Random.NextBoolean();
                break;
        }
        return paramData;
    }
}
