using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Utilities;
using LightningRod.Libraries.Sarc;

namespace LightningRod.Randomizers;

public static class VSStageRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting versus stage randomizer!");

        dynamic versusSceneInfo = GameData.FileSystem.ParseByml(
            $"/RSDB/VersusSceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );
        BymlArrayNode sceneInfo = GameData.FileSystem.ParseByml(
            $"/RSDB/SceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        SarcFile paramPack = GameData.FileSystem.ParseSarc($"/Pack/Params.pack.zs");

        foreach (dynamic versusScene in versusSceneInfo.Array)
        {
            string versusSceneName = versusScene["__RowId"].Data;

            Byml stageBanc = null;
            byte[] rawBancData = [];

            if (paramPack.GetSarcFileData($"Banc/{versusSceneName}.bcett.byml") != null) // Index does exist
            {
                rawBancData = paramPack.GetSarcFileData($"Banc/{versusSceneName}.bcett.byml");
            }
            else // Index does not exist (Mincemeat in 610, Lemuria/Barnacle/Hammerhead in 800-810, Undertow in 710-810)
            {
                Logger.Log($"Unable to find Versus Scene {versusSceneName} in Params.pack.zs");
                SarcFile actorPack = GameData.FileSystem.ParseSarc(
                    $"/Pack/Scene/{versusSceneName}.pack.zs"
                );
                rawBancData = actorPack.GetSarcFileData($"Banc/{versusSceneName}.bcett.byml");
            }

            stageBanc = new Byml(new MemoryStream(rawBancData));

            BymlHashTable stageBancRoot = (BymlHashTable)stageBanc.Root;
            BymlArrayNode stageActors = (BymlArrayNode)stageBancRoot.Values[0]; // get Actors

            List<string> positionData = [];

            if (Options.GetOption("tweakStageLayoutPos"))
                positionData.Add("Translate");
            if (Options.GetOption("tweakStageLayoutSiz"))
                positionData.Add("Scale");
            if (Options.GetOption("tweakStageLayoutRot"))
                positionData.Add("Rotate");

            foreach (BymlHashTable actorData in stageActors.Array)
            {
                if (Options.GetOption("tweakStageLayouts"))
                {
                    foreach (string positionType in positionData)
                    {
                        if (actorData.ContainsKey(positionType))
                        {
                            ((BymlArrayNode)actorData[positionType]).RandomizePositions(
                                (1.0f, (float)Options.GetOption("tweakLevel") / 100),
                                GameData.Random.NextFloatArray(3)
                            );
                        }
                        else
                        {
                            BymlArrayNode positionNode = new BymlArrayNode();
                            (float, float) dataPoints = positionType.Contains("Scale")
                                ? (1.0f, (float)Options.GetOption("tweakLevel") / 100)
                                : (0.0f, (float)Options.GetOption("tweakLevel") / 100);
                            positionNode.RandomizePositions(
                                dataPoints,
                                GameData.Random.NextFloatArray(3)
                            );
                            actorData.AddNode(BymlNodeId.Array, positionNode, positionType);
                        }
                    }
                }
            }

            string[] sceneInfoLabels = ["StageIconBanner", "StageIconL", "StageIconS"];
            BymlArrayNode newSceneInfo = new();

            if (Options.GetOption("mismatchedStages"))
            {
                foreach (BymlHashTable scene in sceneInfo.Array)
                {
                    if (((BymlNode<string>)scene["__RowId"]).Data != versusSceneName)
                        continue;

                    for (int i = 0; i < sceneInfoLabels.Length; i++)
                    {
                        string newStage = versusSceneInfo[
                            GameData.Random.NextInt(versusSceneInfo.Length)
                        ]["__RowId"].Data;
                        newStage = newStage.TrimEnd(['0', '1', '2', '3', '4', '5']);
                        ((BymlNode<string>)scene[sceneInfoLabels[i]]).Data = newStage;
                    }
                    break;
                }
            }

            if (paramPack.GetSarcFileIndex($"Banc/{versusSceneName}.bcett.byml") == -1)
                paramPack.Files.Add(
                    new SarcContent()
                    {
                        Name = $"Banc/{versusSceneName}.bcett.byml",
                        Data = FileUtils.SaveByml(stageBanc.Root),
                    }
                );
            else
                paramPack
                    .Files[paramPack.GetSarcFileIndex($"Banc/{versusSceneName}.bcett.byml")]
                    .Data = FileUtils.SaveByml(stageBanc.Root);
        }

        MiscUtils.CreateFolder("Pack");

        if (Options.GetOption("mismatchedStages"))
        {
            GameData.CommitToFileSystem(
                $"RSDB/SceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs",
                FileUtils.SaveByml(sceneInfo).CompressZSTD()
            );
        }

        if (Options.GetOption("tweakStageLayouts"))
        {
            GameData.CommitToFileSystem(
                "Pack/Params.pack.zs",
                FileUtils.SaveSarc(paramPack).CompressZSTD()
            );
        }
    }

    public static BymlArrayNode RandomizePositions(
        this BymlArrayNode node,
        (float startPoint, float changePoint) dataPoints,
        float[] randInfo
    )
    {
        float basePoint = dataPoints.startPoint - dataPoints.changePoint;
        float modPoint = dataPoints.changePoint * 2;

        bool isNewNode = false;

        if (node.Length < 1)
            isNewNode = true;

        // RNG is handled as (random * 0.02) + 0.99 (example)
        // provides random of 0.99 - 1.01
        // (1, 0.01) in dataPoints does this
        for (int i = 0; i < randInfo.Length; i++)
        {
            if (isNewNode)
                node.AddNodeToArray(
                    new BymlNode<float>(
                        BymlNodeId.Float,
                        (float)((randInfo[i] * modPoint) + basePoint)
                    )
                );
            else
                (node[i] as BymlNode<float>).Data =
                    (float)((randInfo[i] * modPoint) + basePoint)
                    * (node[i] as BymlNode<float>).Data;
        }
        return node;
    }
}
