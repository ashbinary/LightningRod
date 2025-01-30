using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;
using static LightningRod.Randomizers.BigWorldRandomizer;

namespace LightningRod.Randomizers.Solo.BigWorld;

public static class BigWorldSceneRandomizer
{
    public static void Randomize()
    {
        BymlArrayNode missionMapRSDB = GameData.FileSystem.ParseByml(
            $"/RSDB/MissionMapInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        List<string> skyboxActorNames = [];
        Dictionary<string, string> sceneToEnv = [];

        List<string> bgmAssetNames = [];
        dynamic bgmRSDB = GameData.FileSystem.ParseByml(
            $"/RSDB/BgmInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        foreach (dynamic bgm in bgmRSDB.Array)
        {
            if (bgm["Category"].Data == "Mission")
                for (int i = 0; i < bgm["AssetNames"].Length; i++)
                    if (bgm["AssetNames"][i].Data.Contains("Mission") 
                        && !bgm["AssetNames"][i].Data.Contains("Jukebox"))
                        bgmAssetNames.Add(bgm["AssetNames"][i].Data);
        }
        
        // For getting all the data for environments
        foreach (MissionStage stage in missionStages)
        {
            SarcFile levelPack = GameData.FileSystem.ParseSarc(
                $"/Pack/Scene/{stage.SceneName}.pack.zs"
            );

            dynamic sceneInfo = FileUtils.ToByml(levelPack.GetSarcFileData($"Scene/{stage.SceneName}.engine__scene__SceneParam.bgyml")).Root;
            string sceneGfxFieldEnvPath = sceneInfo["Components"]["SceneGfxFieldEnvMission"].Data;

            dynamic sceneGfxFieldEnv = FileUtils.ToByml(levelPack.GetSarcFileData(sceneGfxFieldEnvPath.Compile())).Root;
            string envSetPath = sceneGfxFieldEnv["EnvSet"].Data;

            dynamic envSet = FileUtils.ToByml(levelPack.GetSarcFileData(envSetPath.Compile())).Root;

            skyboxActorNames.Add(envSet["Lighting"]["SkySphere"]["ActorName"].Data);
            sceneToEnv.Add(stage.SceneName, envSetPath.Compile());
        }


        foreach (MissionStage stage in missionStages)
        {
            if (stage.MapType != MsnMapType.ChallengeStage) continue;
            SarcFile levelPack = GameData.FileSystem.ParseSarc(
                $"/Pack/Scene/{stage.SceneName}.pack.zs"
            );

            int missionMapInfoIndex = levelPack.GetSarcFileIndex(
                $"SceneComponent/MissionMapInfo/{stage.SceneName}.spl__MissionMapInfo.bgyml"
            );
            dynamic missionMapInfo = FileUtils
                .ToByml(levelPack.Files[missionMapInfoIndex].Data)
                .Root;
            missionMapInfo = missionMapRSDB.Array.FirstOrDefault(item => (item as dynamic)["__RowId"].Data == stage.SceneName);

            levelPack.Files[missionMapInfoIndex].Data = FileUtils.SaveByml(missionMapInfo);

            int envSetInt = levelPack.GetSarcFileIndex(sceneToEnv[stage.SceneName]);

            dynamic envSet = FileUtils.ToByml(levelPack.Files[envSetInt].Data).Root;
            envSet["Lighting"]["SkySphere"]["ActorName"].Data = skyboxActorNames[GameData.Random.NextInt(skyboxActorNames.Count)];
            levelPack.Files[envSetInt].Data = FileUtils.SaveByml(envSet);

            string bgmString = $"SceneComponent/SceneBgm/{stage.SceneName}.spl__SceneBgmParam.bgyml";
            dynamic sceneBgmParam = FileUtils.ToByml(levelPack.GetSarcFileData(bgmString)).Root;
            sceneBgmParam["SceneSpecificBgm"].Data = bgmAssetNames[GameData.Random.NextInt(bgmAssetNames.Count)];
            levelPack.Files[levelPack.GetSarcFileIndex(bgmString)].Data = FileUtils.SaveByml(sceneBgmParam);

            GameData.CommitToFileSystem(
                $"/Pack/Scene/{stage.SceneName}.pack.zs",
                FileUtils.SaveSarc(levelPack).CompressZSTD()
            );
        }
    }
}
