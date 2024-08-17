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

            string[] positionData = ["Rotate", "Scale", "Translate"];

            foreach (BymlHashTable actorData in stageActors.Array) 
            {
                if (config.tweakStageLayouts) {
                    foreach (string positionType in positionData) 
                    {
                        if (actorData.ContainsKey(positionType)) {
                            ((BymlArrayNode)actorData[positionType]).VSStageRandomizePositions((1.0f, 0.2f), rand.NextFloatArray(3));
                        } else {
                            BymlArrayNode positionNode = new BymlArrayNode();
                            (float, float) dataPoints = positionType.Contains("Scale") ? (1.0f, 0.2f) : (0.0f, 0.2f);
                            positionNode.VSStageRandomizePositions(dataPoints, rand.NextFloatArray(3));
                            actorData.AddNode(BymlNodeId.Array, positionNode, positionType);
                        }
                    }
                }
                
                // if (actorData.ContainsKey("Scale")) {
                //     RandomizerUtil.DebugPrint("handling scale EXISTS");
                //     BymlArrayNode scaleNode = (BymlArrayNode)actorData["Scale"];
                //     scaleNode.SetNodeAtIdx(new BymlNode<float>(BymlNodeId.Float, ((BymlNode<float>)scaleNode[0]).Data * -1), 0);
                //     scaleNode.SetNodeAtIdx(new BymlNode<float>(BymlNodeId.Float, ((BymlNode<float>)scaleNode[2]).Data * -1), 2);
                //     actorData.SetNode("Scale", scaleNode);
                // } else {
                //     RandomizerUtil.DebugPrint("handling scale DOES NOT EXISTS");
                //     if (actorData.ContainsKey("Scale")) continue; // because something is getting through?
                //     BymlArrayNode scaleNode = new() {
                //         Array = [new BymlNode<float>(BymlNodeId.Float, -1.0f), new BymlNode<float>(BymlNodeId.Float, 1.0f), new BymlNode<float>(BymlNodeId.Float, 1.0f)]
                //     };
                //     actorData.AddNode(BymlNodeId.Array, scaleNode, "Scale");
                // }

                if (actorData.ContainsKey("Translate")) {
                    RandomizerUtil.DebugPrint("handling rotation EXISTS");
                    BymlArrayNode positionNode = (BymlArrayNode)actorData["Translate"];
                    positionNode.SetNodeAtIdx(new BymlNode<float>(BymlNodeId.Float, ((BymlNode<float>)positionNode[0]).Data * -1), 0);
                    actorData.SetNode("Translate", positionNode);
                }
            }

            using MemoryStream bancStream = new();
                stageBanc.Save(bancStream);

            sarcBuilderFileList.Add(($"Banc/{versusSceneName}.bcett.byml", bancStream.ToArray().AsMemory<byte>()));
        }
        
        RandomizerUtil.DebugPrint("VS Stage handling complete");
        using FileStream fileSaver = File.Create($"{savePath}/romfs/Pack/Params.pack.zs");
        fileSaver.Write(SarcBuilder.Build(sarcBuilderFileList).CompressZSTDBytes());

    }

    public class VSStageConfig(bool rfl, bool rse, bool tsl, int tl, bool tslp, bool tslr, bool tsls, bool ms, bool yrp, int ycn) {
        public bool randomFogLevels = rfl;
        public bool randomStageEnv = rse;
        public bool tweakStageLayouts = tsl;
        public int tweakLevel = tl;
        public bool tweakStageLayoutPos = tslp;
        public bool tweakStageLayoutRot = tslr;
        public bool tweakStageLayoutSiz = tsls;
        public bool mismatchedStages = ms;
        public bool yaguraRandomPath = yrp;
        public int yaguraCheckpointNum = ycn;
    }
}