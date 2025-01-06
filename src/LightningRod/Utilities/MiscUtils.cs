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
}
