using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Solo.BigWorld;

public static class BigWorldWeaponRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting Alterna randomizer!");

        dynamic missionMapInfo = GameData.FileSystem.ParseByml($"/RSDB/MissionMapInfo.Product.{GameData.GameVersion}.rstbl.byml.zs");

        List<KeyValuePair<string, int>> msnScenes = [];
        
        for (int i = 0; i < missionMapInfo.Length; i++)
        {
            string sceneName = missionMapInfo[i]["__RowId"].Data;
            if (sceneName.Contains("_A")) msnScenes.Add(new KeyValuePair<string, int>(sceneName, i));

            if (sceneName.Contains("StaffRoll")) continue; // Staff roll is a special scene and does not have all this, so ignore

            dynamic oldOctaSupplyArray = null;

            if (missionMapInfo[i]["OctaSupplyWeaponInfoArray"].Length > 0) 
                oldOctaSupplyArray = missionMapInfo[i]["OctaSupplyWeaponInfoArray"];
            var newSupplyArray = new BymlArrayNode(); // Reset it
            
            newSupplyArray.AddNodeToArray(oldOctaSupplyArray[0]);
                
            for (int msn = 0; msn < 2; msn++)
            {
                dynamic msnNode = CreateNewMsnNode();
                msnNode["WeaponMain"].Data = $"Work/Gyml/{GameData.weaponNames.WeaponInfoMain.GetRandomIndex("Msn")}.spl__WeaponInfoMain.gyml";
                newSupplyArray.AddNodeToArray(msnNode);
            }

            missionMapInfo[i]["OctaSupplyWeaponInfoArray"].Array = newSupplyArray.Array;
        }

        GameData.CommitToFileSystem(
            $"/RSDB/MissionMapInfo.Product.{GameData.GameVersion}.rstbl.byml.zs",
            FileUtils.SaveByml((BymlArrayNode)missionMapInfo).CompressZSTD()
        );

        foreach (KeyValuePair<string, int> sceneData in msnScenes) // I HATE THIS GAME
        {
            var parsedScene = GameData.FileSystem.ParseSarc($"/Pack/Scene/{sceneData.Key}.pack.zs");
            var sceneBgymlData = parsedScene.GetSarcFileData($"SceneComponent/MissionMapInfo/{sceneData.Key}.spl__MissionMapInfo.bgyml");
            dynamic sceneBgyml = (BymlHashTable)new Byml(new MemoryStream(sceneBgymlData)).Root;

            sceneBgyml["OctaSupplyWeaponInfoArray"].Array = missionMapInfo[sceneData.Value]["OctaSupplyWeaponInfoArray"].Array;

            parsedScene.Files[parsedScene.GetSarcFileIndex($"SceneComponent/MissionMapInfo/{sceneData.Key}.spl__MissionMapInfo.bgyml")].Data = FileUtils.SaveByml(sceneBgyml);
            GameData.CommitToFileSystem(
                $"/Pack/Scene/{sceneData.Key}.pack.zs",
                FileUtils.SaveSarc(parsedScene).CompressZSTD()
            );
        }
    }

    public static BymlHashTable CreateNewMsnNode()
    {
        BymlHashTable newMsnNode = new();
        newMsnNode.AddHashPair("FirstReward", 1000, BymlNodeId.Int);
        newMsnNode.AddHashPair("IsRecommended", false, BymlNodeId.Bool);
        newMsnNode.AddHashPair("IsRepresentativeIconSecondary", false, BymlNodeId.Bool);
        newMsnNode.AddHashPair("SecondReward", 5000, BymlNodeId.Int);
        newMsnNode.AddHashPair("SpecialWeapon", "", BymlNodeId.String);
        newMsnNode.AddHashPair("SubWeapon", "", BymlNodeId.String);
        newMsnNode.AddHashPair("SupplyWeaponType", "Normal", BymlNodeId.String);
        newMsnNode.AddHashPair("WeaponMain", "", BymlNodeId.String);

        BymlHashTable dolphinMessage = new();
        dolphinMessage.AddHashPair("DevText", "Mod generated by LightningRod. https://github.com/ashbinary/LightningRod", BymlNodeId.String);
        dolphinMessage.AddHashPair("Label", "", BymlNodeId.String);

        BymlHashPair dolphinPair = new();
        dolphinPair.Name = "DolphinMessage";
        dolphinPair.Id = BymlNodeId.Hash;
        dolphinPair.Value = dolphinMessage;

        newMsnNode.Pairs.Add(dolphinPair);
        return newMsnNode;
    }
}