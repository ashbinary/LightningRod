using System.Net;
using System.Runtime.CompilerServices;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;

namespace LightningRod.Randomizers;
class ParameterRandomizer {

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

    public class ParameterConfig(bool rp) {
        bool randomizeParameters = rp;
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
            switch (paramData.Id)
            {
                case BymlNodeId.Int:
                    BymlNode<int> paramInt = (BymlNode<int>)paramData; // handling

                    RandomizerUtil.DebugPrint($"Current Typed param data: {paramInt.Data}");
                    
                    if (paramInt.Data > 0) paramInt.Data = rand.NextInt(paramInt.Data * 2) + 1;
                    else if (paramInt.Data == 0) paramInt.Data = rand.NextInt(1); //unfortunate rare edge case
                    else paramInt.Data = -1 * rand.NextInt(Math.Abs(paramInt.Data));

                    RandomizerUtil.DebugPrint($"Handling Integer, {paramInt.Data}");
                    break;
                case BymlNodeId.Float:
                    BymlNode<float> paramFloat = (BymlNode<float>)paramData; // handling

                    RandomizerUtil.DebugPrint($"Current Typed param data: {paramFloat.Data}");
                    
                    paramFloat.Data = rand.NextFloat() * (paramFloat.Data * 2);

                    RandomizerUtil.DebugPrint($"Handling Float, {paramFloat.Data}");
                    break;
            }
            return paramData;
        }
    }
}