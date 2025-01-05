using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using NintendoTools.FileFormats.Sarc;
using OatmealDome.BinaryData;

namespace LightningRod.Randomizers;

public static class ParameterRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting parameter randomizer!");
        SarcFile paramPack = GameData.FileSystem.ReadCompressedSarc($"/Pack/Params.pack.zs"); 

        foreach (SarcContent paramFileSarc in paramPack.Files)
        {
            if (!paramFileSarc.Name.StartsWith("Component/GameParameterTable/Weapon"))
                continue; // exact opposite as above
            BymlHashTable paramFile = (BymlHashTable)
                new Byml(new MemoryStream(paramFileSarc.Data)).Root;

            BymlIterator paramIterator = new();
            paramFileSarc.Data = paramIterator.IterateParams(paramFile).ToBytes();
        }

        GameData.CommitToFileSystem("Pack/Params.pack.zs", paramPack.CompileSarc().CompressZSTDBytes());

        BymlArrayNode inkColorByml = GameData.FileSystem.ReadCompressedByml(
            $"/RSDB/TeamColorDataSet.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        if (Options.GetOption("randomizeInkColors"))
        {
            string[] teamNames = ["AlphaTeam", "BravoTeam", "CharlieTeam", "Neutral"];
            string[] colorTypes = ["R", "G", "B"];

            for (int i = 0; i < inkColorByml.Length; i++)
            {
                BymlHashTable? colorData = inkColorByml[i] as BymlHashTable;

                if (!Options.GetOption("randomizeInkColorLock")
                    && (colorData["__RowId"] as BymlNode<string>).Data.Contains("Support"))
                    continue;

                //if (MainBanList.Any((mainData["__RowId"] as BymlNode<string>).Data.Contains)) continue;
                for (int t = 0; t < teamNames.Length; t++)
                {
                    BymlHashTable? colorHashTable = colorData[$"{teamNames[t]}Color"] as BymlHashTable;
                    for (int j = 0; j < colorTypes.Length; j++)
                    {
                        (colorHashTable[colorTypes[j]] as BymlNode<float>).Data = GameData.Random.NextFloat();
                    }

                }
            }

            GameData.CommitToFileSystem(
                $"RSDB/TeamColorDataSet.Product.{GameData.GameVersion}.rstbl.byml.zs", 
                inkColorByml.ToBytes().CompressZSTDBytes()
            );
        }
    }

    public class ParameterConfig(bool rp, int ps, bool mic, bool ric, bool ricl)
    {
        public bool randomizeParameters = rp;
        public int parameterSeverity = ps;
        public bool maxInkConsume = mic;
        public bool randomizeInkColors = ric;
        public bool randomizeInkColorLock = ricl;
    }

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

        public IBymlNode HandleValue(IBymlNode paramData)
        {
            dynamic typedParam = paramData;

            double[] severityValues = [1.5, 2.0, 3.0];
            float severity = (float)severityValues[Options.GetOption("parameterSeverity") - 1];

            switch (paramData.Id)
            {
                case BymlNodeId.Int:
                    if (typedParam.Data > 0)
                        typedParam.Data = GameData.Random.NextInt((int)(typedParam.Data * severity)) + 1;
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
}
