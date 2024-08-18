using System.Dynamic;
using System.Runtime.CompilerServices;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Byml.Writer;

public static class BymlIterator {
    public static BymlHashTable IterateParams(this BymlHashTable paramFile) {
        BymlHashTable paramData = (BymlHashTable)paramFile["GameParameters"];
        return paramData;
    }

    public static IBymlNode checkType(IBymlNode paramData) {
        BymlNodeId dataType = paramData.Id;
        switch (dataType) 
        {
            case BymlNodeId.Hash:
                paramData = handleHashTable((BymlHashTable)paramData);
                break;
            case BymlNodeId.Array:
                paramData = handleArrayNode((BymlArrayNode)paramData);
                break;
            case BymlNodeId.Null:
                throw new Exception("Illegal byml node.");
            default:
                paramData = handleValue((BymlNode<dynamic>)paramData);
                break;
        }

        return paramData;
    }

    public static BymlHashTable handleHashTable(BymlHashTable paramData) {
        foreach (string paramKey in paramData.Keys) 
        {
            paramData.SetNode(paramKey, checkType(paramData[paramKey]));
        }
        return paramData;
    }

    public static BymlArrayNode handleArrayNode(BymlArrayNode paramData) {
        for (int i = 0; i < paramData.Length; i++)
        {
            paramData.SetNodeAtIdx(checkType(paramData[i]), i);
        }

        return paramData;
    }

    public static BymlNode<dynamic> handleValue(BymlNode<dynamic> paramData) {
        throw new NotImplementedException("I'll do it tomorrow.");
        return paramData;
    }


}