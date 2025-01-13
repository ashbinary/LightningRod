using System.Text;
using LibHac;
using LibHac.Common;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LibHac.Tools.FsSystem;
using LightningRod.Libraries;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using ZstdNet;

namespace LightningRod.Utilities;

public static class MiscUtils
{
    public static void CreateFolder(string folderName)
    {
        Directory.CreateDirectory($"{GameData.DataPath}/{folderName}");
    }

    public static BymlArrayNode RandomizeData(this BymlArrayNode arrayNode, LongRNG rand)
    {
        foreach (BymlNode<float> bymlNode in arrayNode.Array)
        {
            bymlNode.Data = rand.NextFloat() * 2 * bymlNode.Data;
        }
        return arrayNode;
    }

    public static void SimpleAddNode<T>(
        this BymlHashTable table,
        string name,
        T data,
        BymlNodeId nodeId
    )
    {
        table.AddNode(nodeId, new BymlNode<T>(nodeId, data), name);
    }

    public static void AddHashPair<T>(
        this BymlHashTable table,
        string name,
        T data,
        BymlNodeId nodeId
    )
    {
        BymlHashPair pair = new BymlHashPair();
        pair.Name = name;
        pair.Id = BymlNodeId.Hash;
        pair.Value = new BymlNode<T>(nodeId, data);
        table.Pairs.Add(pair);
    }

    public static string GetRandomIndex(this List<string> dataTable, string constraint)
    {
        string data = "";
        int tableLength = dataTable.Count;
        while (!data.Contains(constraint))
        {
            data = dataTable[GameData.Random.NextInt(tableLength)];
        }
        return data;
    }
}
