using System.Net;
using System.Runtime.CompilerServices;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;

namespace LightningRod.Randomizers;
public class ParameterRandomizer {

    private static ParameterConfig? config;
    private static IFileSystem files;
    private readonly string savePath;

    public ParameterRandomizer(ParameterConfig sceneConfig, IFileSystem fileSys, string save) {
        config = sceneConfig;
        files = fileSys;
        savePath = save;
    }

    public void Randomize(long seed, string version) {
        LongRNG rand = new LongRNG(seed);
        RandomizerUtil.DebugPrint($"Loading parameter randomizer with seed {seed} and version {version}");

        Sarc paramPack = files.ReadCompressedSarc($"/Pack/Params.pack.zs");
        List<(string, Memory<byte>)> sarcBuilderFileList = []; // Essentially taken from VSStageRandomizer

        foreach (Sarc.FileNode node in paramPack.FileNodes) { // Setup SARC builder data
            string fileName = paramPack.GetNodeFilename(node);
            if (fileName.StartsWith("Component/GameParameterTable/Weapon")) continue;
            sarcBuilderFileList.Add((fileName, paramPack.GetFileInSarc(fileName).AsMemory()));
        }

        foreach (Sarc.FileNode paramNode in paramPack.FileNodes)
        {
            string paramFileName = paramPack.GetNodeFilename(paramNode);
            if (!paramFileName.StartsWith("Component/GameParameterTable/Weapon")) continue; // exact opposite as above
            BymlHashTable paramFile = (BymlHashTable)new Byml(new MemoryStream(paramPack.GetFileInSarc(paramFileName))).Root;

            BymlIterator paramIterator = new(seed);
            paramFile = paramIterator.IterateParams(paramFile);
            RandomizerUtil.DebugPrint("Handled param file");
            
            sarcBuilderFileList.Add((paramFileName, paramFile.ToBytes().AsMemory()));
        }

        using FileStream fileSaver = File.Create($"{savePath}/romfs/Pack/Params.pack.zs");
        fileSaver.Write(SarcBuilder.Build(sarcBuilderFileList).CompressZSTDBytes());
    }

    public class ParameterConfig(bool rp, int ps, bool mic) {
        public bool randomizeParameters = rp;
        public int parameterSeverity = ps;
        public bool maxInkConsume = mic;
    }

    public class BymlIterator {
        private LongRNG rand;
        public BymlIterator(long seed) {
            rand = new LongRNG(seed);
        }
        public BymlHashTable IterateParams(BymlHashTable paramFile) {
            if (!paramFile.ContainsKey("GameParameters")) return paramFile;

            BymlHashTable paramData = (BymlHashTable)paramFile["GameParameters"];
            paramData = (BymlHashTable)CheckType(paramData);
            paramFile.SetNode("GameParameters", paramData);
            return paramFile;
        }

        public IBymlNode CheckType(IBymlNode paramData) {
            BymlNodeId dataType = paramData.Id;
            RandomizerUtil.DebugPrint($"Checking param data ID {paramData.Id}");
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

        public BymlHashTable HandleHashTable(BymlHashTable paramData) {
            foreach (string paramKey in paramData.Keys) 
            {
                if (paramKey.Contains("InkConsume") && config.maxInkConsume) paramData.SetNode(paramKey, new BymlNode<float>(BymlNodeId.Float, rand.NextFloat()));
                paramData.SetNode(paramKey, CheckType(paramData[paramKey]));
            }
            return paramData;
        }

        public BymlArrayNode HandleArrayNode(BymlArrayNode paramData) {
            for (int i = 0; i < paramData.Length; i++)
            {
                paramData.SetNodeAtIdx(CheckType(paramData[i]), i);
            }

            return paramData;
        }

        public IBymlNode HandleValue(IBymlNode paramData) {
            dynamic typedParam = paramData;

            double[] severityValues = [1.5, 2.0, 3.0];
            float severity = (float)severityValues[config.parameterSeverity - 1];

            RandomizerUtil.DebugPrint($"Current Typed param data: {typedParam.Data}");
            switch (paramData.Id)
            {
                case BymlNodeId.Int:
                    if (typedParam.Data > 0) typedParam.Data = rand.NextInt((int)(typedParam.Data * severity)) + 1;
                    else if (typedParam.Data == 0) typedParam.Data = rand.NextInt((int)severity); //unfortunate rare edge case
                    else typedParam.Data = -1 * rand.NextInt(Math.Abs(typedParam.Data));
                    break;
                case BymlNodeId.Float:
                    typedParam.Data = rand.NextFloat() * (typedParam.Data * severity);
                    break;
                case BymlNodeId.Bool:
                    typedParam.Data = rand.NextBoolean();
                    break;
            }
            RandomizerUtil.DebugPrint($"Handling, {typedParam.Data}");
            return paramData;
        }
    }
}