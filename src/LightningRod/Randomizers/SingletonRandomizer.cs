using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class SingletonRandomizer
{
    public static void Randomize()
    {
        string singletonPath = GameData.IsNewerVersion(710)
            ? "/Pack/SingletonParam_v-700.pack.zs"
            : "/Pack/SingletonParam.pack.zs";
        SarcFile singletonSarc = GameData.FileSystem.ParseSarc(singletonPath);

        string[] versusConstantKeys = ["BeforeGame", "GameEnd", "Result"];
        int versusConstantIndex = singletonSarc.GetSarcFileIndex(
            $"Gyml/Singleton/spl__VersusConstant.spl__VersusConstant.bgyml"
        );
        dynamic versusConstant = new Byml(
            new MemoryStream(singletonSarc.Files[versusConstantIndex].Data)
        ).Root;

        for (int i = 0; i < versusConstantKeys.Length; i++)
            BymlIterator.IterateParams(versusConstant, versusConstantKeys[i]);

        singletonSarc.Files[versusConstantIndex].Data = FileUtils.SaveByml(versusConstant);

        GameData.CommitToFileSystem(
            singletonPath,
            FileUtils.SaveSarc(singletonSarc).CompressZSTD()
        );
    }
}
