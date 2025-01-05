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

    public MiscRandomizer(MiscConfig sceneConfig)
    {
        config = sceneConfig;
    }

    public void Randomize()
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
