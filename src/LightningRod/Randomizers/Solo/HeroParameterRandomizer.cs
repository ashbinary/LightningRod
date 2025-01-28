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
        BymlIterator paramIterator = new(Options.GetOption("parameterSeverity"));

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
            weaponPack.Files[weaponPack.GetSarcFileIndex(weaponParamPath)].Data = FileUtils.SaveByml(weaponParamFile);

            GameData.CommitToFileSystem(
                $"/Pack/Actor/WmnG_{weaponName}.pack.zs",
                FileUtils.SaveSarc(weaponPack).CompressZSTD()
            );
        }
    }
}
