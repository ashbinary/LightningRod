using System.Net;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Versus.Stage;

public static class StageEnvRandomizer
{
    public static void Randomize()
    {
        List<(string envName, bool isNight)> versusStageEnvs = [];

        foreach (string versusStageName in  VSStageRandomizer.versusSceneIds)
        {
            SarcFile stagePack = GameData.FileSystem.ParseSarc(
                $"/Pack/Scene/{versusStageName}.pack.zs"
            );

            dynamic stageSceneInfo = FileUtils.ToByml(
                stagePack.GetSarcFileData(
                    $"Scene/{versusStageName}.engine__scene__SceneParam.bgyml"
                )
            ).Root;

            string gfxEnvPath = stageSceneInfo["Components"]["SceneGfxFieldEnv"].Data;

            dynamic stageGfxEnvData = FileUtils.ToByml(
                stagePack.GetSarcFileData(gfxEnvPath.Compile())
            ).Root;

            // Only Grand Splatlands Bowl doesn't contain a day environment
            if (!stageGfxEnvData.ContainsKey("EnvSetDay")) continue;

            string envSetDay = stageGfxEnvData["EnvSetDay"].Data;
            string envSetNight = stageGfxEnvData.ContainsKey("EnvSetNight") ? stageGfxEnvData["EnvSetNight"].Data : envSetDay;

            string[] envPaths = [envSetDay.Compile(), envSetNight.Compile()];
            if (Options.GetOption("swapStageEnv")) 
            {
                versusStageEnvs.Add((envSetDay, false));
                versusStageEnvs.Add((envSetNight, true));
            }

            if (Options.GetOption("randomStageEnv"))
            {
                BymlIterator envIterator = new(Options.GetOption("envIntensity") * 0.3); // Max (10) -> 3

                if (!Options.GetOption("randomFogLevels"))
                {
                    envIterator.ruleKeys.Add(
                        key => key.Contains("Fog"),
                        (key, table) => {}
                    );
                }

                if (!Options.GetOption("randomLighting"))
                {
                    envIterator.ruleKeys.Add(
                        key => key.Contains("Lighting"),
                        (key, table) => {}
                    );
                }

                foreach (string envPath in envPaths)
                {
                    dynamic envData = FileUtils.ToByml(
                        stagePack.GetSarcFileData(envPath)
                    ).Root;
                    envData = envIterator.ProcessBymlRoot(envData);
                    stagePack.Files[stagePack.GetSarcFileIndex(envPath)].Data = FileUtils.SaveByml(envData);
                }

                GameData.CommitToFileSystem(
                    $"/Pack/Scene/{versusStageName}.pack.zs",
                    FileUtils.SaveSarc(stagePack).CompressZSTD()
                );
            }
        }

        if (Options.GetOption("swapStageEnv"))
        {
            foreach (string versusStageName in VSStageRandomizer.versusSceneIds)
            {
                SarcFile stagePack = GameData.FileSystem.ParseSarc(
                    $"/Pack/Scene/{versusStageName}.pack.zs"
                );

                dynamic stageSceneInfo = FileUtils.ToByml(
                    stagePack.GetSarcFileData(
                        $"Scene/{versusStageName}.engine__scene__SceneParam.bgyml"
                    )
                ).Root;

                string gfxEnvPath = stageSceneInfo["Components"]["SceneGfxFieldEnv"].Data;

                BymlHashTable stageGfxEnvData = (BymlHashTable)FileUtils.ToByml(
                    stagePack.GetSarcFileData(gfxEnvPath.Compile())
                ).Root;

                for (int i = 0; i < stageGfxEnvData.Pairs.Count; i++)
                {
                    int envRandom = GameData.Random.NextInt(versusStageEnvs.Count);
                    if (!Options.GetOption("mixDayNightEnv"))
                    {
                        while ((versusStageEnvs[envRandom].isNight && i == 0) || (!versusStageEnvs[envRandom].isNight && i == 1))
                        {
                            envRandom = GameData.Random.NextInt(versusStageEnvs.Count);
                        }
                    }

                    stageGfxEnvData.Pairs[i] = new BymlHashPair() {
                        Name = stageGfxEnvData.Pairs[i].Name,
                        Id = BymlNodeId.String,
                        Value = new BymlNode<string>(BymlNodeId.String, versusStageEnvs[envRandom].envName)
                    };
                }

                stagePack.Files[stagePack.GetSarcFileIndex(gfxEnvPath.Compile())].Data = FileUtils.SaveByml(stageGfxEnvData);

                GameData.CommitToFileSystem(
                    $"/Pack/Scene/{versusStageName}.pack.zs",
                    FileUtils.SaveSarc(stagePack).CompressZSTD()
                );
            }
        }
    }
}