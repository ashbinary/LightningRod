using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Byml.Writer;
using LightningRod.Libraries.Sarc;
using ZstdNet;

namespace LightningRod.Randomizers;
public class VSStageRandomizer {

    private static VSStageConfig? config;
    private static IFileSystem files;
    private readonly string savePath;

    public VSStageRandomizer(VSStageConfig sceneConfig, IFileSystem fileSys, string save) {
        config = sceneConfig;
        files = fileSys;
        savePath = save;
    }

    public void Randomize(long seed, string version) {
        LongRNG rand = new(seed);
        RandomizerUtil.DebugPrint("Loading VS Scene randomizer with seed " + seed + " & file version " + version);

        dynamic versusSceneInfo = files.ReadCompressedByml($"/RSDB/VersusSceneInfo.Product.{version}.rstbl.byml.zs");
        BymlArrayNode sceneInfo = files.ReadCompressedByml($"/RSDB/SceneInfo.Product.{version}.rstbl.byml.zs");

        Sarc paramPack = files.ReadCompressedSarc($"/Pack/Params.pack.zs");
        List<(string, Memory<byte>)> sarcBuilderFileList = [];

        foreach (Sarc.FileNode node in paramPack.FileNodes) { // Setup SARC builder data
            string fileName = paramPack.GetNodeFilename(node);
            if (fileName.StartsWith("Banc")) continue;
            sarcBuilderFileList.Add((fileName, paramPack.GetFileInSarc(fileName).AsMemory()));
        }

        foreach (dynamic versusScene in versusSceneInfo.Array) {
            string versusSceneName = versusScene["__RowId"].Data;

            Byml stageBanc = null;
            byte[] rawBancData = [];

            if (paramPack.GetNodeIndex($"Banc/{versusSceneName}.bcett.byml") >= 0) { // Index does exist
                rawBancData = paramPack.GetFileInSarc($"Banc/{versusSceneName}.bcett.byml");
            } else { // Index does not exist (Mincemeat in 610, Lemuria/Barnacle/Hammerhead in 800-810, Undertow in 710-810)
                Sarc actorPack = files.ReadCompressedSarc($"/Pack/Scene/{versusSceneName}.pack.zs");
                rawBancData = actorPack.GetFileInSarc($"Banc/{versusSceneName}.bcett.byml");
            }

            stageBanc = new Byml(new MemoryStream(rawBancData));

            RandomizerUtil.DebugPrint($"Accessing SARC {versusSceneName}...");
            BymlHashTable stageBancRoot = (BymlHashTable)stageBanc.Root;

            BymlArrayNode stageActors = (BymlArrayNode)stageBancRoot.Values[0]; // get Actors

            List<string> positionData = [];

            if (config.tweakStageLayoutPos) positionData.Add("Translate");
            if (config.tweakStageLayoutSiz) positionData.Add("Scale");
            if (config.tweakStageLayoutRot) positionData.Add("Rotate");

            foreach (BymlHashTable actorData in stageActors.Array) 
            {
                if (config.tweakStageLayouts) {
                    foreach (string positionType in positionData) 
                    {
                        if (actorData.ContainsKey(positionType)) {
                            ((BymlArrayNode)actorData[positionType]).VSStageRandomizePositions((1.0f, (float)config.tweakLevel / 100), rand.NextFloatArray(3));
                        } else {
                            BymlArrayNode positionNode = new BymlArrayNode();
                            (float, float) dataPoints = positionType.Contains("Scale") ? (1.0f, (float)config.tweakLevel / 100) : (0.0f, (float)config.tweakLevel / 100);
                            positionNode.VSStageRandomizePositions(dataPoints, rand.NextFloatArray(3));
                            actorData.AddNode(BymlNodeId.Array, positionNode, positionType);
                        }
                    }
                }    
            }

            string[] sceneInfoLabels = ["StageIconBanner", "StageIconL", "StageIconS"];
            BymlArrayNode newSceneInfo = new();

            if (config.mismatchedStages) 
            {
                foreach (BymlHashTable scene in sceneInfo.Array) 
                {
                    if (((BymlNode<string>)scene["__RowId"]).Data != versusSceneName) continue;

                    for (int i = 0; i < sceneInfoLabels.Length; i++) 
                    {
                        string newStage = versusSceneInfo[rand.NextInt(versusSceneInfo.Length)]["__RowId"].Data;
                        newStage = newStage.TrimEnd(['0', '1', '2', '3', '4', '5']);
                        RandomizerUtil.DebugPrint($"Checking for {newStage} in scene {((BymlNode<string>)scene["__RowId"]).Data}");
                        ((BymlNode<string>)scene[sceneInfoLabels[i]]).Data = newStage;
                    }
                    break;
                }

                using FileStream sceneInfoSaver = File.Create($"{savePath}/romfs/RSDB/SceneInfo.Product.{version}.rstbl.byml.zs");
                sceneInfoSaver.Write(sceneInfo.ToBytes().CompressZSTDBytes());
            }

            using MemoryStream bancStream = new();
                stageBanc.Save(bancStream);

            sarcBuilderFileList.Add(($"Banc/{versusSceneName}.bcett.byml", bancStream.ToArray().AsMemory<byte>()));
        }

        RandomizerUtil.DebugPrint("VS Stage handling complete");

        if (config.tweakStageLayouts) {
            using FileStream fileSaver = File.Create($"{savePath}/romfs/Pack/Params.pack.zs");
            fileSaver.Write(SarcBuilder.Build(sarcBuilderFileList).CompressZSTDBytes());
        }

    }

    public class VSStageConfig(bool rfl, bool rse, bool tsl, int tl, bool tslp, bool tslr, bool tsls, bool ms, bool yrp, int ycn) {
        public bool tweakStageLayouts = tsl;
        public int tweakLevel = tl;
        public bool tweakStageLayoutPos = tslp;
        public bool tweakStageLayoutRot = tslr;
        public bool tweakStageLayoutSiz = tsls;
        public bool mismatchedStages = ms;
    }
}