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

        if (Options.GetOption("randomizeInkColors"))
        {
            string[] teamNames = ["AlphaTeam", "BravoTeam", "CharlieTeam", "Neutral"];
            string[] colorTypes = ["R", "G", "B"];

            for (int i = 0; i < inkColorByml.Length; i++)
            {
                BymlHashTable? colorData = inkColorByml[i] as BymlHashTable;

                if (
                    !Options.GetOption("randomizeInkColorLock")
                    && (colorData["__RowId"] as BymlNode<string>).Data.Contains("Support")
                )
                    continue;

                for (int t = 0; t < teamNames.Length; t++)
                {
                    BymlHashTable? colorHashTable =
                        colorData[$"{teamNames[t]}Color"] as BymlHashTable;
                    for (int j = 0; j < colorTypes.Length; j++)
                    {
                        (colorHashTable[colorTypes[j]] as BymlNode<float>).Data =
                            GameData.Random.NextFloat();
                    }
                }
            }

            GameData.CommitToFileSystem(
                $"RSDB/TeamColorDataSet.Product.{GameData.GameVersion}.rstbl.byml.zs",
                FileUtils.SaveByml(inkColorByml).CompressZSTD()
            );
        }
    }
}