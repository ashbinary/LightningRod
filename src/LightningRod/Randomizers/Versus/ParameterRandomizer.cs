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

        BymlIterator paramIterator = new(Options.GetOption("parameterSeverity"));

        paramIterator.ruleKeys.Add(
            key => key.Contains("InkConsume"),
            (key, table) => table.SetNode(
                key,
                new BymlNode<float>(BymlNodeId.String, GameData.Random.NextFloat(0.99f))
            )
        );

        foreach (SarcContent paramFileSarc in paramPack.Files)
        {
            if (!paramFileSarc.Name.StartsWith("Component/GameParameterTable/Weapon"))
                continue;
            BymlHashTable paramFile = (BymlHashTable)
                FileUtils.ToByml(paramFileSarc.Data).Root;

            paramFile = paramIterator.ProcessBymlRoot(paramFile);
            paramFileSarc.Data = FileUtils.SaveByml(paramFile);
        }

        GameData.CommitToFileSystem(
            "/Pack/Params.pack.zs",
            FileUtils.SaveSarc(paramPack).CompressZSTD()
        );
    }
}
