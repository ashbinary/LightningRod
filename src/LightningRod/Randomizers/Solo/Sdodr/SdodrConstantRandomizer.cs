using System.Net.Http.Headers;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Byml.Writer;
using LightningRod.Utilities;
using Newtonsoft.Json;

namespace LightningRod.Randomizers.Solo.Sdodr;

public static class SdodrConstantRandomizer
{
    // All held in SdodrConstant (lockers & events)
    public static void Randomize()
    {
        int sdodrConstantIndex = SdodrRandomizer.singletonSarc.GetSarcFileIndex(
            $"Gyml/Singleton/spl__SdodrConstant.spl__SdodrConstant.bgyml"
        );
        dynamic sdodrConstant = FileUtils.ToByml(
            SdodrRandomizer.singletonSarc.Files[sdodrConstantIndex].Data
        ).Root;

        BymlIterator constantIterator = new BymlIterator(2);

        if (Options.GetOption("randomizeEventChance"))
            sdodrConstant["ChooseStage"]["FloorEventRate"].Array = 
                RandomizeArrayWithConstraint(sdodrConstant["ChooseStage"]["FloorEventRate"], ref constantIterator);

        if (Options.GetOption("randomizeDangerChance"))
            sdodrConstant["EvilEvent"]["ProbabilityTable"].Array = 
                RandomizeArrayWithConstraint(sdodrConstant["EvilEvent"]["ProbabilityTable"], ref constantIterator);

        if (Options.GetOption("randomizeDangerCombos"))
            constantIterator.ProcessHashTable(sdodrConstant["EvilEvent"]["MaskTable"]);

        // sdodrConstant["CoinLocker"]["RewardArray"].Array
        List<IBymlNode> lockerData = new();

        foreach (BymlHashTable tableData in sdodrConstant["CoinLocker"]["RewardArray"].Array)
        {
            BymlHashTable newTable = new();
            foreach (BymlHashPair tablePair in tableData.Pairs)
            {
                newTable.Pairs.Add(
                    new BymlHashPair()
                    {
                        Name = tablePair.Name,
                        Id = tablePair.Id,
                        Value = tablePair.Value
                    }
                );
            }
            lockerData.Add(newTable);
        }
        
        for (int i = 0; i < sdodrConstant["CoinLocker"]["RewardArray"].Length; i++)
        {
            var sdodrLockerValue = sdodrConstant["CoinLocker"]["RewardArray"].Array[i];
            if (Options.GetOption("randomizeLockerOrder"))
            {
                int randomLocker = GameData.Random.NextInt(lockerData.Count);
                sdodrLockerValue = (BymlHashTable)lockerData[randomLocker];
                lockerData.RemoveAt(randomLocker);
            }

            if (Options.GetOption("randomizeLockerJem"))
            {
                // Since Jem is the only int here, it's okay to do this! Dumb? A little.
                if (sdodrLockerValue.ContainsKey("JemNum"))
                    sdodrLockerValue["JemNum"].Data = GameData.Random.NextInt(100);
            }
        }

        SdodrRandomizer.singletonSarc.Files[sdodrConstantIndex].Data = FileUtils.SaveByml(sdodrConstant);
    }

    public static List<IBymlNode> RandomizeArrayWithConstraint(BymlArrayNode arrayNode, ref BymlIterator iterator)
    {
        iterator.ProcessArray(arrayNode);
        foreach (BymlNode<float> node in arrayNode.Array)
            node.Data %= 1;
        return arrayNode.Array;
    }

}