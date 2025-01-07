using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Solo.BigWorld;

public static class BigWorldStageRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting Alterna randomizer!");

        dynamic missionMapInfo = GameData.FileSystem.ParseByml(
            $"/RSDB/MissionMapInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        List<KeyValuePair<string, int>> msnScenes = [];

        for (int i = 0; i < missionMapInfo.Length; i++)
        {
            string sceneName = missionMapInfo[i]["__RowId"].Data;
            if (sceneName.Contains("_A"))
                msnScenes.Add(new KeyValuePair<string, int>(sceneName, i));
        }

        SarcFile alternaPack = GameData.FileSystem.ParseSarc($"/Pack/Scene/BigWorld.pack.zs");
        int bcettIndex = alternaPack.GetSarcFileIndex($"Banc/BigWorld.bcett.byml");

        BymlHashTable? alternaLayoutRoot = (BymlHashTable)
            new Byml(new MemoryStream(alternaPack.Files[bcettIndex].Data)).Root;
        BymlArrayNode? alternaLayout = alternaLayoutRoot["Actors"] as BymlArrayNode;

        for (int i = 0; i < alternaLayout.Length; i++)
        {
            dynamic alternaItem = alternaLayout[i];
            if (!alternaItem["Gyaml"].Data.Contains("MissionGateway"))
                continue;
            string kettleSceneName = alternaItem["spl__MissionGatewayBancParam"][
                "ChangeSceneName"
            ].Data;

            if (kettleSceneName.Contains("_A"))
            {
                int randomNumber = GameData.Random.NextInt(msnScenes.Count);
                alternaItem["spl__MissionGatewayBancParam"]["ChangeSceneName"].Data = msnScenes[
                    randomNumber
                ].Key;
                msnScenes.RemoveAt(randomNumber);
            }
        }

        alternaPack.Files[bcettIndex].Data = FileUtils.SaveByml(alternaLayoutRoot);

        GameData.CommitToFileSystem(
            $"/Pack/Scene/BigWorld.pack.zs",
            FileUtils.SaveSarc(alternaPack).CompressZSTD()
        );
    }
}
