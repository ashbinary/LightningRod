using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Randomizers.Versus.Stage;
using LightningRod.Utilities;
using static LightningRod.Randomizers.BigWorldRandomizer;

namespace LightningRod.Randomizers.Solo.BigWorld;

public static class BigWorldStageRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting Alterna randomizer!");

        if (Options.GetOption("randomizeKettles"))
        {
            SarcFile alternaPack = GameData.FileSystem.ParseSarc($"/Pack/Scene/BigWorld.pack.zs");
            int bcettIndex = alternaPack.GetSarcFileIndex($"Banc/BigWorld.bcett.byml");

            dynamic alternaLayoutRoot = FileUtils.ToByml(alternaPack.Files[bcettIndex].Data).Root;
            dynamic alternaActors = alternaLayoutRoot["Actors"];

            for (int i = 0; i < alternaActors.Length; i++)
            {
                if (alternaActors[i]["Gyaml"].Data.Contains("KebaInkCore"))
                    alternaActors[i]["spl__KebaInkCoreBancParam"]["NecessarySalmonRoe"].Data = GameData.Random.NextInt(3000);

                if (!alternaActors[i]["Gyaml"].Data.Contains("MissionGateway"))
                    continue;

                string kettleSceneName = alternaActors[i]["spl__MissionGatewayBancParam"][
                    "ChangeSceneName"
                ].Data;

                List<string> kettleNames =
                [
                    .. missionStages
                        .Where(ms => ms.MapType == MsnMapType.ChallengeStage)
                        .Select(ms => ms.SceneName),
                ]; // Sure. I guess

                if (kettleNames.Contains(kettleSceneName))
                {
                    int randomNumber = GameData.Random.NextInt(kettleNames.Count);
                    if (alternaActors[i]["spl__MissionGatewayBancParam"]["ChangeSceneName"].Data == "Msn_A01_01")
                        firstStageReplacement = kettleNames[randomNumber];
                    alternaActors[i]["spl__MissionGatewayBancParam"]["ChangeSceneName"].Data = kettleNames[randomNumber];
                    kettleNames.RemoveAt(randomNumber);
                }
            }

            alternaPack.Files[bcettIndex].Data = FileUtils.SaveByml(alternaLayoutRoot);

            GameData.CommitToFileSystem(
                $"/Pack/Scene/BigWorld.pack.zs",
                FileUtils.SaveSarc(alternaPack).CompressZSTD()
            );
        }

        // SmallWorld is here too. cause i dont care

        if (Options.GetOption("randomizeKettlesCrater"))
        {
            SarcFile craterPack = GameData.FileSystem.ParseSarc($"/Pack/Scene/SmallWorld.pack.zs");
            int craterBcettIndex = craterPack.GetSarcFileIndex($"Banc/SmallWorld.bcett.byml");

            dynamic craterLayoutRoot = FileUtils.ToByml(craterPack.Files[craterBcettIndex].Data).Root;
            dynamic craterActors = craterLayoutRoot["Actors"];

            for (int i = 0; i < craterActors.Length; i++)
            {
                if (!craterActors[i]["Gyaml"].Data.Contains("MissionGateway"))
                    continue;

                // All of this is hardcoded. While softcoding is awesome there is statistically 0 reason to not force this.
                List<string> craterStagesToRandomize = ["Msn_C_02", "Msn_C_03", "Msn_C_04"];

                if (craterStagesToRandomize.Contains(craterActors[i]["spl__MissionGatewayBancParam"]["ChangeSceneName"].Data))
                {
                    int randomNumber = GameData.Random.NextInt(3);
                    craterActors[i]["spl__MissionGatewayBancParam"]["ChangeSceneName"].Data = craterStagesToRandomize[randomNumber];
                    craterStagesToRandomize.RemoveAt(randomNumber);
                }
            }

            craterPack.Files[craterBcettIndex].Data = FileUtils.SaveByml(craterLayoutRoot);

            GameData.CommitToFileSystem(
                $"/Pack/Scene/SmallWorld.pack.zs",
                FileUtils.SaveSarc(craterPack).CompressZSTD()
            );
        }
    }
}
