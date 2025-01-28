using System.Collections;
using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class ParameterRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting parameter randomizer!");
        SarcFile paramPack = GameData.FileSystem.ParseSarc($"/Pack/Params.pack.zs");

        BymlIterator paramIterator = new(Options.GetOption("parameterSeverity"));

        paramIterator.ruleKeys.Add(
            key => key.Contains("InkConsume"),
            (key, table) => table.SetNode(
                key,
                new BymlNode<float>(BymlNodeId.String, GameData.Random.NextFloat(0.99f))
            )
        );

        string[] weaponWeights = ["Slow", "Mid", "Fast"];
        string[] weaponWeightStats = ["WeaponSpeedType", "WeaponAccType"];

        foreach (SarcContent paramFileSarc in paramPack.Files)
        {
            if (!paramFileSarc.Name.StartsWith("Component/GameParameterTable/Weapon"))
                continue;
            if (paramFileSarc.Name.Contains("Msn") && !Options.GetOption("randomizeMsnParameters"))
                continue;
            BymlHashTable paramFile = (BymlHashTable)
                FileUtils.ToByml(paramFileSarc.Data).Root;

            if (!paramFile.ContainsKey("GameParameters")) continue;
            paramFile = paramIterator.ProcessBymlRoot(paramFile);

            if ((paramFile["GameParameters"] as BymlHashTable).ContainsKey("MainWeaponSetting")) 
            {
                dynamic mainWeaponSetting = (paramFile["GameParameters"] as BymlHashTable)["MainWeaponSetting"];
                foreach (string weaponStat in weaponWeightStats)
                {
                    string weaponWeight = weaponWeights[GameData.Random.NextInt(weaponWeights.Length)];
                    mainWeaponSetting = ((BymlHashTable)mainWeaponSetting).SetIfExistsElseAdd(weaponStat, weaponWeight);
                }
            }

            paramFileSarc.Data = FileUtils.SaveByml(paramFile);
        }

        GameData.CommitToFileSystem(
            "/Pack/Params.pack.zs",
            FileUtils.SaveSarc(paramPack).CompressZSTD()
        );
    }

    public static BymlHashTable SetIfExistsElseAdd(this BymlHashTable hashTable, string key, string value)
    {
        if (hashTable.ContainsKey(key)) (hashTable[key] as BymlNode<string>).Data = value;
        else hashTable.AddHashPair(key, value, BymlNodeId.String);
        return hashTable;
    }
}
