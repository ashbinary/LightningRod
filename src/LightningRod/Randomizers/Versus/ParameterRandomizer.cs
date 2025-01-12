using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class ParameterRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting parameter randomizer!");
        SarcFile paramPack = GameData.FileSystem.ParseSarc($"/Pack/Params.pack.zs");

        foreach (SarcContent paramFileSarc in paramPack.Files)
        {
            if (!paramFileSarc.Name.StartsWith("Component/GameParameterTable/Weapon"))
                continue;
            BymlHashTable paramFile = (BymlHashTable)
                new Byml(new MemoryStream(paramFileSarc.Data)).Root;

            BymlIterator paramIterator = new();
            paramFileSarc.Data = FileUtils.SaveByml(
                paramIterator.IterateParams(paramFile, "GameParameters")
            );
        }

        GameData.CommitToFileSystem(
            "Pack/Params.pack.zs",
            FileUtils.SaveSarc(paramPack).CompressZSTD()
        );
    }
}
