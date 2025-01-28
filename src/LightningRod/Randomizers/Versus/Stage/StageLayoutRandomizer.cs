using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Versus.Stage;

public static class StageLayoutRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting versus stage randomizer!");

        dynamic versusSceneInfo = GameData.FileSystem.ParseByml(
            $"/RSDB/VersusSceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        SarcFile paramPack = GameData.FileSystem.ParseSarc($"/Pack/Params.pack.zs");

        foreach (dynamic versusScene in versusSceneInfo.Array)
        {
            string versusSceneName = versusScene["__RowId"].Data;
            VSStageRandomizer.versusSceneIds.Add(versusSceneName);

            if (!Options.GetOption("tweakStageLayouts"))
                continue;
            byte[] rawBancData;

            if (paramPack.GetSarcFileIndex($"Banc/{versusSceneName}.bcett.byml") != -1) // Index does exist
            {
                rawBancData = paramPack.GetSarcFileData($"Banc/{versusSceneName}.bcett.byml");
            }
            else // Index does not exist (Mincemeat in 610, Lemuria/Barnacle/Hammerhead in 800-810, Undertow in 710-810)
            {
                Logger.Log($"Unable to find Versus Scene {versusSceneName} in Params.pack.zs");
                SarcFile actorPack = GameData.FileSystem.ParseSarc(
                    $"/Pack/Scene/{versusSceneName}.pack.zs"
                );
                rawBancData = actorPack.GetSarcFileData($"Banc/{versusSceneName}.bcett.byml");
            }

            dynamic stageBanc = FileUtils.ToByml(rawBancData);
            BymlArrayNode stageActors = stageBanc.Root.Values[0]; // get Actors

            StageIterator actorIterator = new(Options.GetOption("tweakLevel"));

            if (Options.GetOption("tweakStageLayoutPos"))
                actorIterator.editedKeys.Add("Translate");
            if (Options.GetOption("tweakStageLayoutSiz"))
                actorIterator.editedKeys.Add("Scale");
            if (Options.GetOption("tweakStageLayoutRot"))
                actorIterator.editedKeys.Add("Rotate");

            stageBanc.Root.SetNode("Actors", actorIterator.ProcessBymlRoot(stageActors));

            if (paramPack.GetSarcFileIndex($"Banc/{versusSceneName}.bcett.byml") == -1)
                paramPack.Files.Add(
                    new SarcContent()
                    {
                        Name = $"Banc/{versusSceneName}.bcett.byml",
                        Data = FileUtils.SaveByml(stageBanc.Root),
                    }
                );
            else
                paramPack
                    .Files[paramPack.GetSarcFileIndex($"Banc/{versusSceneName}.bcett.byml")]
                    .Data = FileUtils.SaveByml(stageBanc.Root);
        }

        if (Options.GetOption("tweakStageLayouts"))
        {
            GameData.CommitToFileSystem(
                "/Pack/Params.pack.zs",
                FileUtils.SaveSarc(paramPack).CompressZSTD()
            );
        }
    }
}
