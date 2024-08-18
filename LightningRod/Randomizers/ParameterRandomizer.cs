using System.Runtime.CompilerServices;
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

            
            
            
        }
    }

    public class ParameterConfig(bool rp) {
        bool randomizeParameters = rp;
    }
}