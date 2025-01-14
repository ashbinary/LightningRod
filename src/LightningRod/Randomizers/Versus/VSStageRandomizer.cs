using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;
using LightningRod.Utilities;

namespace LightningRod.Randomizers;

public static class VSStageRandomizer
{
    public static void Randomize()
    {
        Logger.Log("Starting versus stage randomizer!");

        dynamic versusSceneInfo = GameData.FileSystem.ParseByml(
            $"/RSDB/VersusSceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );
        dynamic sceneInfo = GameData.FileSystem.ParseByml(
            $"/RSDB/SceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        SarcFile paramPack = GameData.FileSystem.ParseSarc($"/Pack/Params.pack.zs");
        List<string> versusSceneRowIds = [];

        foreach (dynamic versusScene in versusSceneInfo.Array)
        {
            string versusSceneName = versusScene["__RowId"].Data;
            versusSceneRowIds.Add(versusSceneName);

            if (!Options.GetOption("tweakStageLayouts"))
                continue;
            byte[] rawBancData;

            if (paramPack.GetSarcFileIndex($"Banc/{versusSceneName}.bcett.byml") != -1) // Index does exist
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

            Byml stageBanc = new Byml(new MemoryStream(rawBancData));

            BymlHashTable stageBancRoot = (BymlHashTable)stageBanc.Root;
            BymlArrayNode stageActors = (BymlArrayNode)stageBancRoot.Values[0]; // get Actors

            List<string> positionData = [];

            if (Options.GetOption("tweakStageLayoutPos"))
                positionData.Add("Translate");
            if (Options.GetOption("tweakStageLayoutSiz"))
                positionData.Add("Scale");
            if (Options.GetOption("tweakStageLayoutRot"))
                positionData.Add("Rotate");

            RandomizeStageActors(ref stageActors, positionData);

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

        string[] sceneInfoLabels = ["StageIconBanner", "StageIconL", "StageIconS"];

        if (Options.GetOption("mismatchedStages") && GameData.IsNewerVersion(120)) //SceneInfo was changed in 1.2.0 to make this possible
        {
            for (int i = 0; i < sceneInfo.Length; i++)
            {
                if (!versusSceneRowIds.Contains(sceneInfo.Array[i]["__RowId"].Data))
                    continue;

                for (int label = 0; label < 3; label++)
                {
                    string randomScene = versusSceneRowIds[
                        GameData.Random.NextInt(versusSceneRowIds.Count)
                    ]
                        .TrimEnd(['0', '1', '2', '3', '4', '5']);
                    sceneInfo.Array[i][sceneInfoLabels[label]].Data = randomScene;
                }
            }

            GameData.CommitToFileSystem(
                $"/RSDB/SceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs",
                FileUtils.SaveByml((BymlArrayNode)sceneInfo).CompressZSTD()
            );
        }

        if (Options.GetOption("tweakStageLayouts"))
        {
            GameData.CommitToFileSystem(
                "/Pack/Params.pack.zs",
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

    public static void RandomizeStageActors(ref BymlArrayNode actorList, List<string> positionData)
    {
        foreach (BymlHashTable actorData in actorList.Array)
        {
            if ((actorData["Name"] as BymlNode<string>).Data == "StartPos")
                continue;
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
    }
}
