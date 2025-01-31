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

        SarcFile newParamPack = new();

        foreach (SarcContent paramFileSarc in paramPack.Files)
        {
            string paramFileName = paramFileSarc.Name;
            if ((!paramFileName.StartsWith("Component/GameParameterTable/Weapon")) ||
                (paramFileName.Contains("Msn") && !Options.GetOption("randomizeMsnParameters")) ||
                (paramFileName.Contains("Coop") && !Options.GetOption("randomizeLoanedParams")))
                continue;

            BymlHashTable paramFile;
            if (paramFileName.Contains("Coop") && !paramFileName.Contains("Bear"))
            {
                string paramFileParsed = paramFileName.Replace("_Coop", "");
                paramFile = (BymlHashTable)FileUtils.ToByml(paramPack.GetSarcFileData(paramFileParsed)).Root;
            }
            else
            {
                paramFile = (BymlHashTable)FileUtils.ToByml(paramFileSarc.Data).Root;
            }

            if (!paramFile.ContainsKey("GameParameters")) continue;
            paramFile = paramIterator.ProcessBymlRoot(paramFile);

            if (Options.GetOption("randomizeWeaponWeight"))
            {
                if ((paramFile["GameParameters"] as BymlHashTable).ContainsKey("MainWeaponSetting")) 
                {
                    dynamic mainWeaponSetting = (paramFile["GameParameters"] as BymlHashTable)["MainWeaponSetting"];
                    foreach (string weaponStat in weaponWeightStats)
                    {
                        string weaponWeight = weaponWeights[GameData.Random.NextInt(weaponWeights.Length)];
                        mainWeaponSetting = ((BymlHashTable)mainWeaponSetting).SetIfExistsElseAdd(weaponStat, weaponWeight);
                    }
                }
            }

            newParamPack.Files.Add(new SarcContent()
            {
                Name = paramFileName,
                Data = FileUtils.SaveByml(paramFile),
            });
        }

        foreach (SarcContent newContent in newParamPack.Files)
        {
            if (paramPack.GetSarcFileIndex(newContent.Name) != -1)
                paramPack.Files[paramPack.GetSarcFileIndex(newContent.Name)] = newContent;
            else
                paramPack.Files.Add(newContent);
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
