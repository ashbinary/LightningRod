using LightningRod.Libraries.Msbt;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;
using NintendoTools.Utils;

namespace LightningRod.Randomizers;

public static class MiscRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting miscellaneous randomizer!");

        SarcFile msbtSarc = GameData.FileSystem.ParseSarc(
            $"/Mals/USen.Product.{GameData.GameVersion}.sarc.zs"
        );
        List<MsbtMessage> dialogueText = [];

        foreach (SarcContent msbtData in msbtSarc.Files)
        {
            MsbtFile msbtFile = msbtData.Data.ParseMsbt();

            if (!msbtData.Name.Contains("LogicMsg") && !msbtData.Name.Contains("EventFlowMsg"))
                continue;

            foreach (MsbtMessage messageData in msbtFile.Messages)
                dialogueText.Add(messageData);
        }

        foreach (SarcContent msbtData in msbtSarc.Files)
        {
            MsbtFile msbtFile = msbtData.Data.ParseMsbt();

            if (
                (msbtData.Name.Contains("LogicMsg") || msbtData.Name.Contains("EventFlowMsg"))
                && true
            )
            {
                foreach (MsbtMessage messageData in msbtFile.Messages)
                {
                    int randomNumber = GameData.Random.NextInt(dialogueText.Count - 1);
                    messageData.Text = dialogueText[randomNumber].Text;
                    messageData.Tags = dialogueText[randomNumber].Tags;
                    dialogueText.RemoveAt(randomNumber);
                }
            }
            else
            {
                List<string> messageLabels = msbtFile.Messages.Select(t => t.Label).ToList();
                foreach (MsbtMessage messageData in msbtFile.Messages)
                {
                    int randomNumber = GameData.Random.NextInt(messageLabels.Count - 1);
                    messageData.Label = messageLabels[randomNumber];
                    messageLabels.RemoveAt(randomNumber);
                }
            }

            msbtData.Data = FileUtils.SaveMsbt(msbtFile);
        }

        MiscUtils.CreateFolder("Mals");

        GameData.CommitToFileSystem(
            $"Mals/USen.Product.{GameData.GameVersion}.sarc.zs",
            FileUtils.SaveSarc(msbtSarc).CompressZSTD()
        );
    }
}
