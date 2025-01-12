using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
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
        BymlArrayNode weaponSub = GameData.FileSystem.ParseByml(
            $"/RSDB/WeaponInfoSub.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );
        BymlArrayNode weaponSpecial = GameData.FileSystem.ParseByml(
            $"/RSDB/WeaponInfoSpecial.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        List<string> MainBanList = ["_Coop", "_Msn", "_Sdodr", "RivalLv1", "RivalLv2"];
        List<string> SubBanList = ["_Mission", "_Rival", "_Sdodr", "SalmonBuddy"];
        List<string> SpecialBanList = ["_Mission", "Sdodr", "_Rival", "_Coop"];

        if (!Options.GetOption("heroSubSelection"))
            SubBanList.Add("_Hero");
        if (!Options.GetOption("coopSplatBomb"))
            SubBanList.Add("_Coop");
        if (Options.GetOption("allSubWeapons"))
            SubBanList = [""];

        if (!Options.GetOption("heroModeSuperLanding"))
            SpecialBanList.Add("SuperLanding");
        if (!Options.GetOption("useRainmaker"))
            SpecialBanList.Add("Gachihoko");
        if (!Options.GetOption("useIkuraShoot"))
            SpecialBanList.Add("IkuraShoot");
        if (Options.GetOption("useAllSpecials"))
            SpecialBanList = [""];

        Logger.Log($"Banned Subs: {string.Join(", ", SubBanList)}");
        Logger.Log($"Banned Specials: {string.Join(", ", SpecialBanList)}");

        int minPFS = Options.GetOption("include170To220p") ? 170 : 180;
        int maxPFS = Options.GetOption("include170To220p") ? 220 : 210;

        List<string> subList = [];
        List<string> specialList = [];

        for (int i = 0; i < weaponSub.Length; i++)
        {
            string RowId = ((weaponSub[i] as BymlHashTable)["__RowId"] as BymlNode<string>).Data;
            GameData.weaponNames.WeaponInfoSub.Add(RowId);
            if (!SubBanList.Any(RowId.Contains))
                subList.Add(RowId);
        }

        for (int i = 0; i < weaponSpecial.Length; i++)
        {
            string RowId = (
                (weaponSpecial[i] as BymlHashTable)["__RowId"] as BymlNode<string>
            ).Data;
            GameData.weaponNames.WeaponInfoSpecial.Add(RowId);
            if (!SpecialBanList.Any(RowId.Contains))
                specialList.Add(RowId);
        }

        Dictionary<string, int> periscopeIndexes = [];

        for (int i = 0; i < weaponMain.Length; i++)
        {
            BymlHashTable? mainData = weaponMain[i] as BymlHashTable;

            GameData.weaponNames.WeaponInfoMain.Add((mainData["__RowId"] as BymlNode<string>).Data);

            if (MainBanList.Any((mainData["__RowId"] as BymlNode<string>).Data.Contains))
                continue;

            int pfs = GameData.Random.NextInt(maxPFS - minPFS) + minPFS + 5; // better odds towards 220 (sorry machine mains)
            if (!Options.GetOption("noPFSIncrementation"))
                pfs = pfs / 10 * 10;

            ((BymlNode<int>)mainData["SpecialPoint"]).Data = pfs;

            (mainData["SubWeapon"] as BymlNode<string>).Data =
                $"Work/Gyml/{subList[GameData.Random.NextInt(subList.Count)]}.spl__WeaponInfoSub.gyml";
            (mainData["SpecialWeapon"] as BymlNode<string>).Data =
                $"Work/Gyml/{specialList[GameData.Random.NextInt(specialList.Count)]}.spl__WeaponInfoSpecial.gyml";

            if (
                Options.GetOption("matchPeriscopeKits")
                && (mainData["__RowId"] as BymlNode<string>).Data.Contains("Charger")
            )
            {
                periscopeIndexes.Add((mainData["__RowId"] as BymlNode<string>).Data, i);
            }
        }

        if (Options.GetOption("matchPeriscopeKits"))
        {
            foreach (string key in periscopeIndexes.Keys)
            {
                if (key.Contains("Scope"))
                {
                    (
                        (weaponMain[periscopeIndexes[key]] as BymlHashTable)["SubWeapon"]
                        as BymlNode<string>
                    ).Data = (
                        (weaponMain[periscopeIndexes[key.Replace("Scope", "")]] as BymlHashTable)[
                            "SubWeapon"
                        ] as BymlNode<string>
                    ).Data;
                    (
                        (weaponMain[periscopeIndexes[key]] as BymlHashTable)["SpecialWeapon"]
                        as BymlNode<string>
                    ).Data = (
                        (weaponMain[periscopeIndexes[key.Replace("Scope", "")]] as BymlHashTable)[
                            "SpecialWeapon"
                        ] as BymlNode<string>
                    ).Data;
                }
            }
        }

        if (Options.GetOption("randomizeKits"))
            GameData.CommitToFileSystem(
                $"/RSDB/WeaponInfoMain.Product.{GameData.GameVersion}.rstbl.byml.zs",
                FileUtils.SaveByml(weaponMain).CompressZSTD()
            );
    }
}
