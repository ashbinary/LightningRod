using System.Drawing;
using LightningRod.Libraries.Byml;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Solo.Sdodr;

public static class SdodrLoadoutRandomizer
{
    // Loadout = palette & hacks before run
    public static void Randomize()
    {
        int paletteDefineIndex = SdodrRandomizer.singletonSarc.GetSarcFileIndex(
            $"Gyml/Singleton/spl__SdodrAbilityCustom__PaletteDefineTable.spl__SdodrAbilityCustom__PaletteDefineTable.bgyml"
        );
        dynamic paletteDefine = FileUtils.ToByml(
            SdodrRandomizer.singletonSarc.Files[paletteDefineIndex].Data
        ).Root;

        List<BymlHashPair> paletteTable = paletteDefine["Table"].Pairs;
        List<string> userNames = paletteTable.Select(pair => pair.Name).ToList();

        if (Options.GetOption("randomizeDefaultPalettes"))
        {
            paletteDefine["BeginningRewardPaletteKey"].Data = userNames[GameData.Random.NextInt(userNames.Count)];
            userNames.Remove(paletteDefine["BeginningRewardPaletteKey"].Data);

            paletteDefine["DefaultPaletteKey"].Data = userNames[GameData.Random.NextInt(userNames.Count)];
            userNames.Remove(paletteDefine["DefaultPaletteKey"].Data);
        }

        if (Options.GetOption("randomizePaletteKits"))
        {
            ColorChipType[] colorChipData = [.. Enum.GetValues(typeof(ColorChipType)).Cast<ColorChipType>()];
            foreach (BymlHashPair palette in paletteDefine["Table"].Pairs)
            {
                dynamic paletteData = palette.Value;

                if (Options.GetOption("randomizePaletteKits"))
                {
                    string randomSpecial = GameData.weaponNames.WeaponInfoSpecial.GetRandomIndex(WeaponType.Sdodr);
                    paletteData["SpecialWeapon"].Data = $"Work/Gyml/{randomSpecial}.spl__WeaponInfoSpecial.gyml";

                    string randomSub = GameData.weaponNames.WeaponInfoSub.GetRandomIndex(WeaponType.Sdodr);
                    paletteData["SubWeapon"].Data = $"Work/Gyml/{randomSub}.spl__WeaponInfoSub.gyml";
                }

                if (Options.GetOption("randomizePaletteTones"))
                {
                    // This is the most confusing thing ever. Why is it literally only functional like this.
                    (int First, int Second) randomTones;
                    randomTones.First = GameData.Random.NextInt(5);
                    randomTones.Second = GameData.Random.NextInt(5);
                    
                    paletteData["FreqFirstColorGroupType"].Data = colorChipData[randomTones.First].ToString();
                    paletteData["FreqSecondColorGroupType"].Data = colorChipData[randomTones.Second].ToString();
                }
            }  
        }

        SdodrRandomizer.singletonSarc.Files[paletteDefineIndex].Data = FileUtils.SaveByml(paletteDefine);
    
        int hackingConstantIndex = SdodrRandomizer.singletonSarc.GetSarcFileIndex(
            $"Gyml/Singleton/spl__SdodrHackingConstant.spl__SdodrHackingConstant.bgyml"
        );
        dynamic hackingConstant = FileUtils.ToByml(
            SdodrRandomizer.singletonSarc.Files[hackingConstantIndex].Data
        ).Root;

        BymlIterator hackIterator = new(2);

        hackIterator.ruleKeys.Add(
            key => key.Contains("ContentParams"),
            (key, table) => {}
        );

        hackIterator.ruleKeys.Add(
            key => key.Contains("JemHardMode"),
            (key, table) => {}
        );

        if (Options.GetOption("randomizeHackEffects"))
            hackIterator.ProcessBymlRoot(hackingConstant);

        foreach (BymlHashPair hackData in hackingConstant["ContentParams"].Pairs)
        {
            dynamic hackValue = hackData.Value;
            if (Options.GetOption("randomizeHackCost") && hackValue.ContainsKey("CostArray"))
                hackIterator.ProcessArray(hackValue["CostArray"]);
            if (Options.GetOption("randomizeHackEffects") && hackValue.ContainsKey("DisplayValue"))
                hackIterator.ProcessArray(hackValue["DisplayValue"]);
            if (Options.GetOption("unlockAllHacks") && hackValue.ContainsKey("UnlockCondition"))
            {
                for (int i = 0; i < hackValue.Pairs.Count; i++)
                    if (hackValue.Pairs[i].Name == "UnlockCondition")
                        hackValue.Pairs.RemoveAt(i);
            }   
        }

        SdodrRandomizer.singletonSarc.Files[hackingConstantIndex].Data = FileUtils.SaveByml(hackingConstant);
    }
}