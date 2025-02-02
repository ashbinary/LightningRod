using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Randomizers.Solo.Sdodr;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class SdodrRandomizer
{
    public static SarcFile singletonSarc = new();
    public static void Randomize()
    {
        string singletonPath = GameData.IsNewerVersion(700)
            ? "/Pack/SingletonParam_v-700.pack.zs"
            : "/Pack/SingletonParam.pack.zs";
        singletonSarc = GameData.FileSystem.ParseSarc(singletonPath);

        SdodrLoadoutRandomizer.Randomize();
        SdodrConstantRandomizer.Randomize();
        SdodrColorChipRandomizer.Randomize();

        GameData.CommitToFileSystem(
            singletonPath,
            FileUtils.SaveSarc(singletonSarc).CompressZSTD()
        );

        // if (Options.GetOption("randomizeAgent4Kits"))
        // {
        //     SarcFile rivalSarc = GameData.FileSystem.ParseSarc("Pack/Actor/RivalAppearSequencerSdodr.pack.zs");
        //     int rivalFileIndex = rivalSarc.GetSarcFileIndex("Component/GameParameterTable/RivalAppearSequencerSdodr.game__GameParameterTable.bgyml");
        //     dynamic rivalFile = FileUtils.ToByml(rivalSarc.Files[rivalFileIndex].Data).Root;

        //     foreach (BymlHashTable rivalWeaponSet in rivalFile["spl__RivalAppearSequencerParamSdodr"]["WeponSets"])
        //     {
        //         dynamic rivalWeapon = rivalWeaponSet;
        //         if (rivalWeaponSet.ContainsKey("Main"))
        //             rivalWeapon["Sub"].Data = GameData.weaponNames.WeaponInfoMain.GetRandomIndex("");
        //     }
        // }
    }
}

public enum ColorChipType
{
    Auto = 0,
    Continuity = 1,
    Fire = 2,
    Luck = 3,
    Move = 4,
    Range = 5,
    Default = 99
}
