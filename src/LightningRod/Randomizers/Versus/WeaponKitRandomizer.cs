using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Byml.Writer;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class WeaponKitRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting weapon kit randomizer!");

        BymlArrayNode weaponMain = GameData.FileSystem.ParseByml(
            $"/RSDB/WeaponInfoMain.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );
        dynamic weaponSub = GameData.FileSystem.ParseByml(
            $"/RSDB/WeaponInfoSub.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );
        dynamic weaponSpecial = GameData.FileSystem.ParseByml(
            $"/RSDB/WeaponInfoSpecial.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        List<string> MainBanList = ["_Coop", "_Msn", "_Sdodr", "RivalLv1", "RivalLv2"];
        List<string> SubBanList = ["_Mission", "_Rival", "_Sdodr", "SalmonBuddy", "Splash_Coop"];
        List<string> SpecialBanList = ["_Mission", "Sdodr", "_Rival", "IkuraShoot", "_Coop"];

        if (!Options.GetOption("heroSubSelection"))
            SubBanList.Add("_Hero");
        if (!Options.GetOption("coopSplatBomb"))
            SubBanList.Add("_Big_Coop");
        if (Options.GetOption("allSubWeapons"))
            SubBanList = [""];

        if (!Options.GetOption("heroModeSuperLanding"))
            SpecialBanList.Add("SuperLanding");
        if (!Options.GetOption("useRainmaker"))
            SpecialBanList.Add("Gachihoko");
        if (Options.GetOption("useAllSpecials"))
            SpecialBanList = [""];

        Logger.Log($"Banned Subs: {string.Join(", ", SubBanList)}");
        Logger.Log($"Banned Specials: {string.Join(", ", SpecialBanList)}");

        int minPFS = Options.GetOption("minimumPFS");
        int maxPFS = Options.GetOption("maximumPFS");

        List<string> subList = ParseThroughFileAndBan(ref weaponSub, ref GameData.weaponNames.WeaponInfoSub, ref SubBanList);
        List<string> specialList = ParseThroughFileAndBan(ref weaponSpecial, ref GameData.weaponNames.WeaponInfoSpecial, ref SpecialBanList);

        Dictionary<string, int> periscopeIndexes = [];

        for (int i = 0; i < weaponMain.Length; i++)
        {
            dynamic? mainData = weaponMain[i];
            string mainType = mainData["Type"].Data;

            GameData.weaponNames.WeaponInfoMain.Add(
                new Weapon(mainData["__RowId"].Data, mainType.ToEnum<WeaponType>())
            );

            if (MainBanList.Contains(mainData["__RowId"].Data))
                continue;

            // Skew towards right more so max isn't literally impossible
            int pfs = GameData.Random.NextInt(maxPFS - minPFS) + minPFS + 5;

            if (!Options.GetOption("noPFSIncrementation"))
                pfs = pfs / 10 * 10;

            mainData["SpecialPoint"].Data = pfs;

            mainData["SubWeapon"].Data =
                $"Work/Gyml/{subList[GameData.Random.NextInt(subList.Count)]}.spl__WeaponInfoSub.gyml";
            mainData["SpecialWeapon"].Data =
                $"Work/Gyml/{specialList[GameData.Random.NextInt(specialList.Count)]}.spl__WeaponInfoSpecial.gyml";

            if (
                Options.GetOption("matchPeriscopeKits")
                && mainData["__RowId"].Data.Contains("Charger")
            )
            {
                periscopeIndexes.Add(mainData["__RowId"].Data, i);
            }

            weaponMain.Array[i] = (BymlHashTable)mainData;
        }

        if (Options.GetOption("matchPeriscopeKits"))
        {
            string[] overwriteKeys = ["SubWeapon", "SpecialWeapon", "SpecialPoint"];
            foreach (string key in periscopeIndexes.Keys)
            {
                if (!key.Contains("Scope")) continue;
                string altKey = key.Replace("Scope", "");

                foreach (string indexKey in overwriteKeys)
                {
                    ((dynamic)weaponMain[periscopeIndexes[key]])[indexKey].Data = (
                        (dynamic)weaponMain[periscopeIndexes[altKey]]
                    )[indexKey].Data;
                }
            }
        }

        if (Options.GetOption("randomizeKits"))
            GameData.CommitToFileSystem(
                $"/RSDB/WeaponInfoMain.Product.{GameData.GameVersion}.rstbl.byml.zs",
                FileUtils.SaveByml(weaponMain).CompressZSTD()
            );
    }

    public static List<string> ParseThroughFileAndBan(ref dynamic weaponList, ref List<Weapon> weaponData, ref List<string> banList)
    {
        List<string> passedList = [];

        for (int i = 0; i < weaponList.Length; i++)
        {
            string weaponRowId = weaponList[i]["__RowId"].Data;
            string weaponType = weaponList[i]["Type"].Data;

            if (weaponRowId.Contains("Sdodr"))
            {
                weaponData.Add(
                    new Weapon(weaponRowId, WeaponType.Sdodr)
                );
            }
            else
            {
                weaponData.Add(
                    new Weapon(weaponRowId, weaponType.ToEnum<WeaponType>())
                );
            }

            if (!banList.Any(weaponRowId.Contains))
                passedList.Add(weaponRowId);
        }

        return passedList;
    }
}
