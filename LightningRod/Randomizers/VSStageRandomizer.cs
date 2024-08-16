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

        Sarc paramPack = files.ReadCompressedSarc($"/Pack/Params.pack.zs");
        List<(string, Memory<byte>)> sarcBuilderFileList = [];
        
        foreach (Sarc.FileNode node in paramPack.FileNodes) {
            var nodeFilename = paramPack.GetNodeFilename(node);
            if (!nodeFilename.StartsWith("Banc/")) {
                var componentData = paramPack.GetFileInSarc(nodeFilename);
                sarcBuilderFileList.Add((nodeFilename, componentData.AsMemory())); // Remove files that aren't related to stages
                continue;
            }

            RandomizerUtil.DebugPrint($"Accessing SARC {nodeFilename}...");

            Byml stageBanc = new Byml(new MemoryStream(paramPack.GetFileInSarc(nodeFilename)));
            BymlHashTable stageBancRoot = (BymlHashTable)stageBanc.Root;

            BymlArrayNode stageActors = (BymlArrayNode)stageBancRoot.Values[0]; // get Actors

            string[] positionData = ["Rotate", "Scale", "Translate"];

            foreach (BymlHashTable actorData in stageActors.Array) 
            {
                foreach (string positionType in positionData) 
                {
                    if (!actorData.ContainsKey(positionType)) continue;
                    foreach (BymlNode<float> bymlNode in (actorData[positionType] as BymlArrayNode).Array) 
                    {
                        bymlNode.Data = rand.NextFloat() * 2 * bymlNode.Data;
                        RandomizerUtil.DebugPrint($"Returned array node with float unk and RNG unk");
                    }
                }
            }

            using MemoryStream bancStream = new();
                stageBanc.Save(bancStream);

            sarcBuilderFileList.Add((nodeFilename, bancStream.ToArray().AsMemory<byte>()));
        }
        
        using FileStream fileSaver = File.Create($"{savePath}/romfs/Pack/Params.pack.zs");
        fileSaver.Write(SarcBuilder.Build(sarcBuilderFileList));

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