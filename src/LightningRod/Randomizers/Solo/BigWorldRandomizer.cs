using LibHac.FsSystem;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Randomizers.Solo.BigWorld;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class BigWorldRandomizer
{
    public static List<MissionStage> missionStages = [];
    public static string firstStageReplacement = "Msn_A01_01";
    public static void Randomize()
    {
        Logger.Log("Starting Alterna randomizer!");

        dynamic missionMapInfo = GameData.FileSystem.ParseByml(
            $"/RSDB/MissionMapInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        for (int i = 0; i < missionMapInfo.Length; i++)
        {
            string stageName = missionMapInfo[i]["__RowId"].Data;
            if (stageName.Contains("StaffRoll")) // Staff roll is a special scene and does not have all this
            {
                missionStages.Add(new MissionStage("StaffRoll", MsnMapType.StaffRoll));
                continue;
            }

            string mapTypeString = missionMapInfo[i]["MapType"].Data;
            MsnMapType mapType = mapTypeString.ToEnum<MsnMapType>();

            missionStages.Add(new MissionStage(stageName, mapType));
        }

        BigWorldStageRandomizer.Randomize(); // MUST be first
        BigWorldMapInfoRandomizer.Randomize(); 
        BigWorldSceneRandomizer.Randomize();

        if (Options.GetOption("randomizeDigUpPointEggs"))
        {
            SarcFile lootAnchorPack = GameData.FileSystem.ParseSarc($"/Pack/Actor/WorldDigUpPoint.pack.zs");
            int lootAnchorIndex = lootAnchorPack.GetSarcFileIndex("Component/GameParameterTable/WorldDigUpPoint.game__GameParameterTable.bgyml");
            dynamic lootAnchor = FileUtils.ToByml(lootAnchorPack.Files[lootAnchorIndex].Data).Root;

            lootAnchor["GameParameters"]["spl__WorldDigUpPointParam"]["IkuraSpawnParam"]["IkuraValue"].Data = (int)(Options.GetOption("digUpPointEggCount") / 5);
            lootAnchorPack.Files[lootAnchorIndex].Data = FileUtils.SaveByml(lootAnchor);

            GameData.CommitToFileSystem(
                $"/Pack/Actor/WorldDigUpPoint.pack.zs",
                FileUtils.SaveSarc(lootAnchorPack).CompressZSTD()
            );
        }


    }

    public class MissionStage(string sceneName, MsnMapType worldType)
    {
        public string SceneName = sceneName;
        public MsnMapType MapType = worldType;
    }

    public enum MsnMapType
    {
        ChallengeStage, // All Alterna levels
        SmallWorldStage, // All Crater levels
        BigWorldBossStage, // Shiver, Frye, Big Man
        BigWorld, // Alterna main hub
        SmallWorld, // Crater main hub
        LastBoss, // Grizz phase 1 & 2
        LaunchPadWorld, // ?
        NormalStage, // Alterna mission parent
        ExStage, // After Alterna
        LaunchPadStage, // Rocket
        SmallWorldBossStage, // Octavio
        StaffRoll, // Not in game, used to prevent crash
    }

    public enum SupplyWeaponType
    {
        Normal = 0,
        Hero = 1,
        Special = 2,
        MainAndSpecial = 3,
    }
}
