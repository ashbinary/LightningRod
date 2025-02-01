using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Coop;
public static class CoopWeaponRandomizer
{
    public static void Randomize()
    {
        string singletonPath = GameData.IsNewerVersion(700)
            ? "/Pack/SingletonParam_v-700.pack.zs"
            : "/Pack/SingletonParam.pack.zs";
        SarcFile singletonSarc = GameData.FileSystem.ParseSarc(singletonPath);

        int ruleConfigIndex = singletonSarc.GetSarcFileIndex("Gyml/Singleton/spl__CoopRuleConfig.spl__CoopRuleConfig.bgyml");
        dynamic coopRuleConfig = FileUtils.ToByml(singletonSarc.Files[ruleConfigIndex].Data).Root;

        int specialAmount = 0;
        
        if (Options.GetOption("randomizeLoanedSpecials")) specialAmount = GameData.Random.NextInt(8) + 8;
        if (Options.GetOption("allSpecialsLoanable")) specialAmount = GameData.weaponNames.WeaponInfoSpecial.GetAllWeaponsOfType(WeaponType.Versus).Count;

        string[] coopConfigs = ["SpecialWeapon", "SpecialWeaponV6", "SpecialWeaponV6Prior"];
        foreach (string config in coopConfigs)
            coopRuleConfig["Common"][config].Array = new List<IBymlNode>();
        
        Console.WriteLine("im killing myself tonight deadass");
        List<string> weaponList = GameData.weaponNames.WeaponInfoSpecial.GetAllWeaponsOfType(WeaponType.Versus);
        Console.WriteLine("Fuck my stupid hawk tuah life");
        for (int i = 0; i < specialAmount; i++)
        {
            Console.WriteLine("im HAPPY!");
            string weaponData = "";

            if (Options.GetOption("randomizeLoanedSpecials")) {
                GameData.weaponNames.WeaponInfoSpecial.GetRandomIndex(WeaponType.Versus);
            } else if (Options.GetOption("allSpecialsLoanable")) {
                weaponData = weaponList[i];
            } else { 
                continue;
            }
            
            BymlNode<string> weaponNode = new BymlNode<string>(
                BymlNodeId.String, 
                $"Work/Gyml/{weaponData}.spl__WeaponInfoSpecial.gyml"
            );

            coopRuleConfig["Common"]["SpecialWeapon"].Array.Add(weaponNode);

            string v6Data = i % 2 == 0 ? "SpecialWeaponV6Prior" : "SpecialWeaponV6"; // just sort into even or odd
            coopRuleConfig["Common"][v6Data].Array.Add(weaponNode);
        }

        if (Options.GetOption("randomizeLoanedSub"))
            coopRuleConfig["Common"]["SubWeapon"].Data = GameData.weaponNames.WeaponInfoSub.GetRandomIndex(WeaponType.Versus);
        singletonSarc.Files[ruleConfigIndex].Data = FileUtils.SaveByml(coopRuleConfig);

        GameData.CommitToFileSystem(
            singletonPath,
            FileUtils.SaveSarc(singletonSarc).CompressZSTD()
        );
    }
}