using System.Globalization;
using System.Net;
using System.Runtime.CompilerServices;
using LibHac.Fs;
using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Solo;

public static class HeroParameterRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting parameter randomizer!");

        List<string> missionWeapons = GameData.weaponNames.WeaponInfoMain.GetAllWeaponsOfType(WeaponType.Mission);
        SarcFile paramPack = GameData.FileSystem.ParseSarc($"/Pack/Params.pack.zs");
        BymlIterator paramIterator = new(Options.GetOption("parameterSeverity"));

        if (Options.GetOption("randomizeMsnParameters"))
        {
            foreach (string weaponName in missionWeapons)
            {
                SarcFile weaponPack = GameData.FileSystem.ParseSarc(
                    $"/Pack/Actor/WmnG_{weaponName}.pack.zs"
                );

                dynamic weaponActor = FileUtils.ToByml(weaponPack.GetSarcFileData($"Actor/WmnG_{weaponName}.engine__actor__ActorParam.bgyml")).Root;

                string actorParentPath = weaponActor["$parent"].Data;
                actorParentPath = actorParentPath.Compile();
                dynamic weaponActorParent = FileUtils.ToByml(weaponPack.GetSarcFileData(actorParentPath)).Root;

                string weaponParamPath = weaponActorParent["Components"]["GameParameterTable"].Data;
                weaponParamPath = weaponParamPath.Compile();
                if (weaponPack.GetSarcFileIndex(weaponParamPath) == -1) continue; // WHY DO THEY HAVE STRINGER AND STAMPER IN THE PARAM PACK WHY WHY WHY
                dynamic weaponParamFile = FileUtils.ToByml(weaponPack.GetSarcFileData(weaponParamPath)).Root;

                weaponParamFile = paramIterator.ProcessBymlRoot(weaponParamFile);
                paramPack.Files.Add(new SarcContent() { Name = weaponParamPath, Data = FileUtils.SaveByml(weaponParamFile) });
            }
        }

        if (Options.GetOption("randomizeSdodrParameters"))
        {
            string singletonPath = GameData.IsNewerVersion(700)
                ? "/Pack/SingletonParam_v-700.pack.zs"
                : "/Pack/SingletonParam.pack.zs";
            SarcFile singletonSarc = GameData.FileSystem.ParseSarc(singletonPath);

            dynamic paletteTable = FileUtils.ToByml(singletonSarc.GetSarcFileData($"Gyml/Singleton/spl__SdodrAbilityCustom__PaletteDefineTable.spl__SdodrAbilityCustom__PaletteDefineTable.bgyml")).Root;
            foreach (BymlHashPair palette in paletteTable["Table"].Pairs)
            {
                string mainWeaponPath = (palette.Value as dynamic)["MainWeapon"].Data;
                string actorName = "WmnG_" + mainWeaponPath.Split("Work/Gyml/")[1].Split(".spl__")[0];
                mainWeaponPath.Compile();

                SarcFile weaponPack = GameData.FileSystem.ParseSarc(
                    $"/Pack/Actor/{actorName}.pack.zs"
                );

                dynamic weaponActor = FileUtils.ToByml(weaponPack.GetSarcFileData($"Actor/{actorName}.engine__actor__ActorParam.bgyml")).Root;

                string actorParentPath = weaponActor["$parent"].Data;
                actorParentPath = actorParentPath.Compile();
                dynamic weaponActorParent = FileUtils.ToByml(weaponPack.GetSarcFileData(actorParentPath)).Root;

                string weaponParamPath = weaponActorParent["Components"]["GameParameterTable"].Data;
                weaponParamPath = weaponParamPath.Compile();
                if (weaponPack.GetSarcFileIndex(weaponParamPath) == -1) continue; // WHY DO THEY HAVE STRINGER AND STAMPER IN THE PARAM PACK WHY WHY WHY
                dynamic weaponParamFile = FileUtils.ToByml(weaponPack.GetSarcFileData(weaponParamPath)).Root;

                weaponParamFile = paramIterator.ProcessBymlRoot(weaponParamFile);
                paramPack.Files.Add(new SarcContent() { Name = weaponParamPath, Data = FileUtils.SaveByml(weaponParamFile) });
            }
        }

        GameData.CommitToFileSystem(
            $"/Pack/Params.pack.zs",
            FileUtils.SaveSarc(paramPack).CompressZSTD()
        );
    }
}
