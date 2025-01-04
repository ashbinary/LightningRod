using System.Net;
using System.Runtime.CompilerServices;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;

namespace LightningRod.Randomizers;

public class MiscRandomizer
{
    private static MiscConfig? config;
    private static IFileSystem files;
    private readonly string savePath;

    public MiscRandomizer(MiscConfig sceneConfig, IFileSystem fileSys, string save)
    {
        config = sceneConfig;
        files = fileSys;
        savePath = save;
    }

    public void Randomize(long seed, string version)
    {
        throw new Exception();
    }

    public class MiscConfig(bool rp, int ps, bool mic)
    {
        public bool randomizeParameters = rp;
        public int parameterSeverity = ps;
        public bool maxInkConsume = mic;
    }
}
