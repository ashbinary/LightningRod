using System.Runtime.CompilerServices;
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

        int missionConstantIndex = singletonSarc.GetSarcFileIndex(
            $"Gyml/Singleton/spl__MissionConstant.spl__MissionConstant.bgyml"
        );
        dynamic missionConstant = new Byml(
            new MemoryStream(singletonSarc.Files[missionConstantIndex].Data)
        ).Root;

        BymlArrayNode skillTree = missionConstant["PlayerSkillTree"]["SkillIconTable"];

        List<string> skillTreeTypes = skillTree
            .Array // is this dumb? yes. do i care? no
            .OfType<BymlHashTable>()
            .Where(option => option.ContainsKey("SkillType"))
            .Select(option => (option["SkillType"] as BymlNode<string>).Data)
            .ToList();

        for (int i = 0; i < skillTree.Length; i++)
        {
            if (!(skillTree.Array[i] as BymlHashTable).ContainsKey("SkillType"))
                continue;

            int randomNumber = GameData.Random.NextInt(skillTreeTypes.Count);
            ((skillTree.Array[i] as BymlHashTable)["SkillType"] as BymlNode<string>).Data =
                skillTreeTypes[randomNumber];
            skillTreeTypes.RemoveAt(randomNumber);
        }

        singletonSarc.Files[missionConstantIndex].Data = FileUtils.SaveByml(missionConstant);

        GameData.CommitToFileSystem(
            singletonPath,
            FileUtils.SaveSarc(singletonSarc).CompressZSTD()
        );
    }
}
