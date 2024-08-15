using System.Runtime.InteropServices;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
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

        List<string> sceneNames = [];

        using Decompressor decompress = new();
        dynamic VSceneByml = files.ReadCompressedByml($"/RSDB/VersusSceneInfo.Product.{version}.rstbl.byml.zs");

        Directory.CreateDirectory(savePath + "/romfs/Pack/Scene");

        for (int i = 0; i < VSceneByml.Length; i++)
            sceneNames.Add(VSceneByml[i]["__RowId"].Data);

        foreach (string sceneName in sceneNames) { // ok this is the meaty part
            Sarc sceneSARC;
            Dictionary<string, Memory<byte>> buildAsset = new();

            sceneSARC = new Sarc(decompress.Unwrap(files.ReadWholeFile($"/Pack/Scene/{sceneName}.pack.zs")));
            dynamic fieldEnvCheck = new Byml(new MemoryStream(sceneSARC.OpenFile(sceneSARC.GetNodeIndex($"Gyml/{sceneName}.game__gfx__parameter__FieldEnv.bgyml")).ToArray())).Root;

            string DayEnvPathRef = fieldEnvCheck["EnvSetDay"].Data.Substring(5);
            string NightEnvPathRef = fieldEnvCheck["EnvSetNight"].Data.Substring(5);

            RandomizerUtil.DebugPrint($"Day env: {DayEnvPathRef}"); 
            RandomizerUtil.DebugPrint($"Night env: {NightEnvPathRef}");

            foreach (Sarc.FileNode fileNode in sceneSARC.FileNodes) {
                buildAsset.Add(sceneSARC.GetNodeFilename(fileNode), sceneSARC.OpenFile(fileNode).ToArray());
            }

            Byml fieldLayout = new Byml(new MemoryStream(sceneSARC.OpenFile(sceneSARC.GetNodeIndex($"Banc/{sceneName}.bcett.byml")).ToArray()));
            BymlHashTable layoutRoot = fieldLayout.Root as BymlHashTable;

            for (int i = 0; i < (layoutRoot["Actors"] as BymlArrayNode).Length; i++) {
                BymlHashTable layoutActor = (layoutRoot["Actors"] as BymlArrayNode)[i] as BymlHashTable;
                BymlArrayNode pos, rot, siz;
                if (layoutActor.ContainsKey("Translate")) {
                    var stuff = (layoutActor["Translate"] as BymlArrayNode);
                    ((layoutActor["Translate"] as BymlArrayNode)[0] as BymlNode<float>).Data = (float)((rand.NextFloat() * ((stuff[0] as BymlNode<float>).Data * (config.tweakLevel / 20))) + ((stuff[0] as BymlNode<float>).Data * 1 - (config.tweakLevel / 20 / 2)));
                    ((layoutActor["Translate"] as BymlArrayNode)[1] as BymlNode<float>).Data = (float)((rand.NextFloat() * ((stuff[1] as BymlNode<float>).Data * (config.tweakLevel / 20))) + ((stuff[1] as BymlNode<float>).Data * 1 - (config.tweakLevel / 20 / 2)));
                    ((layoutActor["Translate"] as BymlArrayNode)[2] as BymlNode<float>).Data = (float)((rand.NextFloat() * ((stuff[2] as BymlNode<float>).Data * (config.tweakLevel / 20))) + ((stuff[2] as BymlNode<float>).Data * 1 - (config.tweakLevel / 20 / 2)));
                } else {
                    BymlArrayNode tempBymlNode = new BymlArrayNode();
                    tempBymlNode.AddNodeToArray(new BymlNode<float>(BymlNodeId.Float, ((float)(1 * (config.tweakLevel / 20))) + (1 * 1 - (config.tweakLevel / 20 / 2))));
                    tempBymlNode.AddNodeToArray(new BymlNode<float>(BymlNodeId.Float, ((float)(1 * (config.tweakLevel / 20))) + (1 * 1 - (config.tweakLevel / 20 / 2))));
                    tempBymlNode.AddNodeToArray(new BymlNode<float>(BymlNodeId.Float, ((float)(1 * (config.tweakLevel / 20))) + (1 * 1 - (config.tweakLevel / 20 / 2))));

                    layoutActor.AddNode(BymlNodeId.Array, tempBymlNode, "Translate");
                }

                if (layoutActor.ContainsKey("Rotate")) {
                    var stuff = (layoutActor["Rotate"] as BymlArrayNode);
                    ((layoutActor["Rotate"] as BymlArrayNode)[0] as BymlNode<float>).Data = (float)((rand.NextFloat() * ((stuff[0] as BymlNode<float>).Data * (config.tweakLevel / 20))) + ((stuff[0] as BymlNode<float>).Data * 1 - (config.tweakLevel / 20 / 2)));
                    ((layoutActor["Rotate"] as BymlArrayNode)[1] as BymlNode<float>).Data = (float)((rand.NextFloat() * ((stuff[1] as BymlNode<float>).Data * (config.tweakLevel / 20))) + ((stuff[1] as BymlNode<float>).Data * 1 - (config.tweakLevel / 20 / 2)));
                    ((layoutActor["Rotate"] as BymlArrayNode)[2] as BymlNode<float>).Data = (float)((rand.NextFloat() * ((stuff[2] as BymlNode<float>).Data * (config.tweakLevel / 20))) + ((stuff[2] as BymlNode<float>).Data * 1 - (config.tweakLevel / 20 / 2)));
                } else {
                    BymlArrayNode tempBymlNode = new BymlArrayNode();
                    tempBymlNode.AddNodeToArray(new BymlNode<float>(BymlNodeId.Float, ((float)(1 * (config.tweakLevel / 20))) + (1 * 1 - (config.tweakLevel / 20 / 2))));
                    tempBymlNode.AddNodeToArray(new BymlNode<float>(BymlNodeId.Float, ((float)(1 * (config.tweakLevel / 20))) + (1 * 1 - (config.tweakLevel / 20 / 2))));
                    tempBymlNode.AddNodeToArray(new BymlNode<float>(BymlNodeId.Float, ((float)(1 * (config.tweakLevel / 20))) + (1 * 1 - (config.tweakLevel / 20 / 2))));

                    layoutActor.AddNode(BymlNodeId.Array, tempBymlNode, "Rotate");
                }

                if (layoutActor.ContainsKey("Scale")) {
                    var stuff = (layoutActor["Scale"] as BymlArrayNode);
                    ((layoutActor["Scale"] as BymlArrayNode)[0] as BymlNode<float>).Data = (float)((rand.NextFloat() * ((stuff[0] as BymlNode<float>).Data * (config.tweakLevel / 20))) + ((stuff[0] as BymlNode<float>).Data * 1 - (config.tweakLevel / 20 / 2)));
                    ((layoutActor["Scale"] as BymlArrayNode)[1] as BymlNode<float>).Data = (float)((rand.NextFloat() * ((stuff[1] as BymlNode<float>).Data * (config.tweakLevel / 20))) + ((stuff[1] as BymlNode<float>).Data * 1 - (config.tweakLevel / 20 / 2)));
                    ((layoutActor["Scale"] as BymlArrayNode)[2] as BymlNode<float>).Data = (float)((rand.NextFloat() * ((stuff[2] as BymlNode<float>).Data * (config.tweakLevel / 20))) + ((stuff[2] as BymlNode<float>).Data * 1 - (config.tweakLevel / 20 / 2)));
                } else {
                    BymlArrayNode tempBymlNode = new BymlArrayNode();
                    tempBymlNode.AddNodeToArray(new BymlNode<float>(BymlNodeId.Float, ((float)(1 * (config.tweakLevel / 20))) + (1 * 1 - (config.tweakLevel / 20 / 2))));
                    tempBymlNode.AddNodeToArray(new BymlNode<float>(BymlNodeId.Float, ((float)(1 * (config.tweakLevel / 20))) + (1 * 1 - (config.tweakLevel / 20 / 2))));
                    tempBymlNode.AddNodeToArray(new BymlNode<float>(BymlNodeId.Float, ((float)(1 * (config.tweakLevel / 20))) + (1 * 1 - (config.tweakLevel / 20 / 2))));

                    layoutActor.AddNode(BymlNodeId.Array, tempBymlNode, "Scale");
                }

                // if (layoutActor.ContainsKey("Scale")) {
                //     siz = layoutActor["Scale"] as BymlArrayNode;
                //     for (int j = 0; j < 2; i++)
                //         (((layoutActor["Scale"] as BymlArrayNode)[j] as BymlArrayNode)[j] as BymlNode<float>).Data = rand.NextFloat() * ((siz[j] as BymlNode<float>).Data * 2);
                // }

            }

            fieldLayout.Root = layoutRoot;

            using (MemoryStream prezs = new()) {
                fieldLayout.Save(prezs);
                buildAsset[$"Banc/{sceneName}.bcett.byml"] = prezs.ToArray();
            }

            List<(string, Memory<byte>)> fixedBuilder = new();
            foreach (string key in buildAsset.Keys) {
                fixedBuilder.Add((key, buildAsset[key]));
            } 

            byte[] finalData = SarcBuilder.Build(fixedBuilder);

            Stream streamwrite = File.Create($"{savePath}/romfs/Pack/Scene/{sceneName}.pack.zs");
        
            using (MemoryStream preCompress = new()) {
                using Compressor compressor= new();
                var compressedByml = compressor.Wrap(finalData);
                streamwrite.Write(compressedByml, 0, compressedByml.Length);
            }
            
            streamwrite.Flush();
            streamwrite.Close();

            byte[] resaveReader = File.ReadAllBytes($"{savePath}/romfs/Pack/Scene/{sceneName}.pack.zs");

            using (FileStream resaveWriter = File.Open($"{savePath}/romfs/Pack/Scene/{sceneName}.pack.zs", FileMode.Create)) {
                using Decompressor awesomeDecompressor = new();
                using Compressor awesomeCompressor = new();
                byte[] recompressedData = awesomeCompressor.Wrap(awesomeDecompressor.Unwrap(resaveReader));
                resaveWriter.Write(recompressedData);
            }

        }

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