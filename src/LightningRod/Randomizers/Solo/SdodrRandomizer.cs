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

        GameData.CommitToFileSystem(
            singletonPath,
            FileUtils.SaveSarc(singletonSarc).CompressZSTD()
        );
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
