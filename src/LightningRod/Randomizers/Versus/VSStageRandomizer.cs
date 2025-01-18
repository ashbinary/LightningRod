using System.Security.Cryptography.X509Certificates;
using LightningRod.Libraries.Byml;
using LightningRod.Randomizers.Versus.Stage;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Versus;

public static class VSStageRandomizer
{
    public static List<string> versusSceneIds = [];
    
    public static void Randomize()
    {
        Logger.Log("Starting versus stage randomizer!");

        StageLayoutRandomizer.Randomize();

        dynamic sceneInfo = GameData.FileSystem.ParseByml(
            $"/RSDB/SceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        string[] sceneInfoLabels = ["StageIconBanner", "StageIconL", "StageIconS"];

        if (Options.GetOption("mismatchedStages") && GameData.IsNewerVersion(120)) //SceneInfo was changed in 1.2.0 to make this possible
        {
            for (int i = 0; i < sceneInfo.Length; i++)
            {
                if (!versusSceneIds.Contains(sceneInfo.Array[i]["__RowId"].Data))
                    continue;

                for (int label = 0; label < 3; label++)
                {
                    string randomScene = versusSceneIds[
                        GameData.Random.NextInt(versusSceneIds.Count)
                    ]
                        .TrimEnd(['0', '1', '2', '3', '4', '5']);
                    sceneInfo.Array[i][sceneInfoLabels[label]].Data = randomScene;
                }
            }

            GameData.CommitToFileSystem(
                $"/RSDB/SceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs",
                FileUtils.SaveByml((BymlArrayNode)sceneInfo).CompressZSTD()
            );
        }
    }
}