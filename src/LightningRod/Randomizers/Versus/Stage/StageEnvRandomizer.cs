using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Versus.Stage;

public static class StageEnvRandomizer
{
    public static void Randomize()
    {
        List<string> versusStageEnvs = [];

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
            if (Options.GetOption("swapStageEnv")) versusStageEnvs.AddRange(envPaths);

            if (Options.GetOption("randomStageEnv"))
            {
                BymlIterator envIterator = new(3);
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
                    stageGfxEnvData.Pairs[i] = new BymlHashPair() {
                        Name = stageGfxEnvData.Pairs[i].Name,
                        Id = BymlNodeId.String,
                        Value = new BymlNode<string>(BymlNodeId.String, versusStageEnvs[GameData.Random.NextInt(versusStageEnvs.Count)])
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