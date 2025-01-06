using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class BigWorldRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting Alterna randomizer!");

        SarcFile alternaPack = GameData.FileSystem.ParseSarc($"/Pack/Scene/BigWorld.pack.zs");
        Byml alternaLayout = new(
            new MemoryStream(
                alternaPack
                    .Files[alternaPack.GetSarcFileIndex($"/Pack/Scene/BigWorld.pack.zs")]
                    .Data
            )
        );
    }
}
