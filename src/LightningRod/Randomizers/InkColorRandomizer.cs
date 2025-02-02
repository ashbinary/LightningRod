using LightningRod.Libraries.Byml;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class InkColorRandomizer
{
    public static void Randomize()
    {
        BymlArrayNode inkColorByml = GameData.FileSystem.ParseByml(
            $"/RSDB/TeamColorDataSet.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        string[] colorTypes = ["R", "G", "B"];

        if (Options.GetOption("randomizeInkColors"))
        {
            string[] teamNames = ["AlphaTeam", "BravoTeam", "CharlieTeam", "Neutral"];

            for (int i = 0; i < inkColorByml.Length; i++)
            {
                BymlHashTable? colorData = inkColorByml[i] as BymlHashTable;
                string colorTag = (colorData["Tag"] as BymlNode<string>).Data;

                if (
                    colorTag.ColorContains("Option", "randomizeInkColorLock")
                    || colorTag.ColorContains("Mission", "randomizeInkColorMsn")
                )
                    continue;

                for (int t = 0; t < teamNames.Length; t++)
                {
                    BymlHashTable? colorHashTable =
                        colorData[$"{teamNames[t]}Color"] as BymlHashTable;
                    for (int j = 0; j < colorTypes.Length; j++)
                    {
                        float inkColorValue =
                            GameData.Random.NextInt(255) + Options.GetOption("inkColorBias");
                        (colorHashTable[colorTypes[j]] as BymlNode<float>).Data =
                            Math.Clamp(inkColorValue, 0, 255) / 255;
                    }
                }
            }

            GameData.CommitToFileSystem(
                $"/RSDB/TeamColorDataSet.Product.{GameData.GameVersion}.rstbl.byml.zs",
                FileUtils.SaveByml(inkColorByml).CompressZSTD()
            );
        }

        if (Options.GetOption("randomizeInkColorSdodr"))
        {
            string[] sdodrTeams = ["Enemy", "Friend", "Neutral"];

            BymlArrayNode sdodrColorByml = GameData.FileSystem.ParseByml(
                $"/RSDB/Exam01Info.Product.{GameData.GameVersion}.rstbl.byml.zs"
            );

            for (int i = 0; i < sdodrColorByml.Length; i++)
            {
                dynamic colorData = sdodrColorByml[i];

                for (int t = 0; t < sdodrTeams.Length; t++)
                {
                    BymlHashTable? colorHashTable =
                        colorData[$"{sdodrTeams[t]}Color"]["TeamColor"]
                        ;
                    for (int j = 0; j < colorTypes.Length; j++)
                    {
                        float inkColorValue =
                            GameData.Random.NextInt(255) + Options.GetOption("inkColorBias");
                        (colorHashTable[colorTypes[j]] as BymlNode<float>).Data =
                            Math.Clamp(inkColorValue, 0, 255) / 255;
                    }
                }
            }

            GameData.CommitToFileSystem(
                $"/RSDB/Exam01Info.Product.{GameData.GameVersion}.rstbl.byml.zs",
                FileUtils.SaveByml(inkColorByml).CompressZSTD()
            );
        }
    }

    public static bool ColorContains(this string colorName, string lookupName, string optionName)
    {
        // i deaduzz do not know how this works bro
        return !Options.GetOption(optionName) && colorName.Contains(lookupName);
    }
}
