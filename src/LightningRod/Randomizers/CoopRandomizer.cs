using LightningRod.Libraries.Sarc;
using LightningRod.Randomizers.Coop;
using LightningRod.Randomizers.Versus.Stage;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class CoopRandomizer
{
    public static void Randomize()
    {
        CoopEnemyRandomizer.Randomize();

        dynamic coopSceneInfo = GameData.FileSystem.ParseByml(
            $"/RSDB/CoopSceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        if (Options.GetOption("tweakCoopStageLayouts"))
        {
            StageIterator coopIterator = new StageIterator(2);
            coopIterator.editedKeys.AddRange(["Translate", "Rotate", "Scale"]);

            foreach (dynamic coopScene in coopSceneInfo.Array)
            {
                string sceneName = coopScene["__RowId"].Data;

                SarcFile scenePack = GameData.FileSystem.ParseSarc(
                    $"/Pack/Scene/{sceneName}.pack.zs"
                );

                dynamic sceneBanc = FileUtils.ToByml(scenePack.GetSarcFileData($"Banc/{sceneName}.bcett.byml"));
                coopIterator.ProcessBymlRoot(sceneBanc.Root.Values[0]); // Actors
                scenePack.Files[scenePack.GetSarcFileIndex($"Banc/{sceneName}.bcett.byml")].Data = FileUtils.SaveByml(sceneBanc.Root);
                
                GameData.CommitToFileSystem(
                    $"/Pack/Scene/{sceneName}.pack.zs",
                    FileUtils.SaveSarc(scenePack).CompressZSTD()
                );
            }
        }
    }
}
