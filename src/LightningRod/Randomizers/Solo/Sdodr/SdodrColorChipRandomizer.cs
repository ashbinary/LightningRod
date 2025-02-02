using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers.Solo.Sdodr;

public static class SdodrColorChipRandomizer
{
    public static void Randomize()
    {
        SarcFile bootupPack = GameData.FileSystem.ParseSarc(
            $"/Pack/Bootup.Nin_NX_NVN.pack.zs"
        );

        int tipDefineIndex = SdodrRandomizer.singletonSarc.GetSarcFileIndex(
            $"Gyml/Singleton/spl__SdodrAbilityCustom__TipDefineTable.spl__SdodrAbilityCustom__TipDefineTable.bgyml"
        );
        dynamic tipDefine = FileUtils.ToByml(
            SdodrRandomizer.singletonSarc.Files[tipDefineIndex].Data
        ).Root;

        BymlIterator chipIterator = new BymlIterator(3);

        if (Options.GetOption("randomizeColorChipStats"))
        {
            foreach (SarcContent bootupFile in bootupPack.Files)
            {
                if (!bootupFile.Name.Contains("SdodrContentParam")) continue;

                dynamic chipData = FileUtils.ToByml(bootupFile.Data).Root;
                chipIterator.ProcessBymlRoot(chipData);

                bootupFile.Data = FileUtils.SaveByml(chipData);
            }

            GameData.CommitToFileSystem(
                $"/Pack/Bootup.Nin_NX_NVN.pack.zs",
                FileUtils.SaveSarc(bootupPack).CompressZSTD()
            );
        }

        if (Options.GetOption("swapColorChips"))
        {
            ColorChipType[] colorChipData = [.. Enum.GetValues(typeof(ColorChipType)).Cast<ColorChipType>()];
            string[] dataPartTwo = ["A", "B", "C"];
            foreach (BymlHashPair tipData in tipDefine["Table"].Pairs)
            {
                int randomChip = GameData.Random.NextInt(5);
                int randomPart2 = GameData.Random.NextInt(2); // um.
                (tipData.Value as dynamic)["Color"].Data = $"{colorChipData[randomChip]}_{dataPartTwo[randomPart2]}";
            };

            SdodrRandomizer.singletonSarc.Files[tipDefineIndex].Data = FileUtils.SaveByml(tipDefine);
        }
    }
}