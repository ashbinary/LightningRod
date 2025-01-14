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

        List<string> positionData = [];

        if (Options.GetOption("tweakStageLayoutPos"))
            positionData.Add("Translate");
        if (Options.GetOption("tweakStageLayoutSiz"))
            positionData.Add("Scale");
        if (Options.GetOption("tweakStageLayoutRot"))
            positionData.Add("Rotate");

        foreach (KeyValuePair<string, int> sceneKvp in msnScenes)
        {
            SarcFile levelPack = GameData.FileSystem.ParseSarc(
                $"/Pack/Scene/{sceneKvp.Key}.pack.zs"
            );
            dynamic levelMsnInfo = FileUtils
                .ToByml(
                    levelPack.GetSarcFileData(
                        $"Scene/{sceneKvp.Key}.engine__scene__SceneParam.bgyml"
                    )
                )
                .Root;
            string specialMapName = levelMsnInfo["Components"]
                ["StartupMap"]
                .Data.Replace("Work/Banc/Scene", "Banc")
                .Replace("json", "byml"); // Because of course the bancs use proprietary names

            int levelBancIndex = levelPack.GetSarcFileIndex($"{specialMapName}");
            dynamic levelBanc = FileUtils.ToByml(levelPack.Files[levelBancIndex].Data).Root;
            BymlArrayNode levelActors = levelBanc.Values[0];
            VSStageRandomizer.RandomizeStageActors(ref levelActors, positionData);

            var sceneBgymlData = levelPack.GetSarcFileData(
                $"SceneComponent/MissionMapInfo/{sceneKvp.Key}.spl__MissionMapInfo.bgyml"
            );
            dynamic sceneBgyml = (BymlHashTable)new Byml(new MemoryStream(sceneBgymlData)).Root;

            sceneBgyml["OctaSupplyWeaponInfoArray"].Array = missionMapInfo[sceneKvp.Value][
                "OctaSupplyWeaponInfoArray"
            ].Array;

            levelPack
                .Files[
                    levelPack.GetSarcFileIndex(
                        $"SceneComponent/MissionMapInfo/{sceneKvp.Key}.spl__MissionMapInfo.bgyml"
                    )
                ]
                .Data = FileUtils.SaveByml(sceneBgyml);

            levelPack.Files[levelBancIndex].Data = FileUtils.SaveByml((BymlHashTable)levelBanc);
            GameData.CommitToFileSystem(
                $"/Pack/Scene/{sceneKvp.Key}.pack.zs",
                FileUtils.SaveSarc(levelPack)
            );
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
