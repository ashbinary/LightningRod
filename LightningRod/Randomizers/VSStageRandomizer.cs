using LibHac.Fs.Fsa;
using LightningRod.Libraries.Byml;
using LightningRod.Libraries.Sarc;

namespace LightningRod.Randomizers;

public static class VSStageRandomizer
{
    public static void Randomize()
    {
        dynamic versusSceneInfo = GameData.FileSystem.ReadCompressedByml(
            $"/RSDB/VersusSceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );
        BymlArrayNode sceneInfo = GameData.FileSystem.ReadCompressedByml(
            $"/RSDB/SceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs"
        );

        Sarc paramPack = GameData.FileSystem.ReadCompressedSarc($"/Pack/Params.pack.zs");
        List<(string, Memory<byte>)> sarcBuilderFileList = [];

        foreach (Sarc.FileNode node in paramPack.FileNodes)
        { // Setup SARC builder data
            string fileName = paramPack.GetNodeFilename(node);
            if (fileName.StartsWith("Banc"))
                continue;
            sarcBuilderFileList.Add((fileName, paramPack.GetFileInSarc(fileName).AsMemory()));
        }

        foreach (dynamic versusScene in versusSceneInfo.Array)
        {
            string versusSceneName = versusScene["__RowId"].Data;

            Byml stageBanc = null;
            byte[] rawBancData = [];

            if (paramPack.GetNodeIndex($"Banc/{versusSceneName}.bcett.byml") >= 0)
            { // Index does exist
                rawBancData = paramPack.GetFileInSarc($"Banc/{versusSceneName}.bcett.byml");
            }
            else
            { // Index does not exist (Mincemeat in 610, Lemuria/Barnacle/Hammerhead in 800-810, Undertow in 710-810)
                Sarc actorPack = GameData.FileSystem.ReadCompressedSarc($"/Pack/Scene/{versusSceneName}.pack.zs");
                rawBancData = actorPack.GetFileInSarc($"Banc/{versusSceneName}.bcett.byml");
            }

            stageBanc = new Byml(new MemoryStream(rawBancData));

            RandomizerUtil.DebugPrint($"Accessing SARC {versusSceneName}...");
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
                            ((BymlArrayNode)actorData[positionType]).VSStageRandomizePositions(
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
                            positionNode.VSStageRandomizePositions(
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
                        string newStage = versusSceneInfo[GameData.Random.NextInt(versusSceneInfo.Length)][
                            "__RowId"
                        ].Data;
                        newStage = newStage.TrimEnd(['0', '1', '2', '3', '4', '5']);
                        RandomizerUtil.DebugPrint(
                            $"Checking for {newStage} in scene {((BymlNode<string>)scene["__RowId"]).Data}"
                        );
                        ((BymlNode<string>)scene[sceneInfoLabels[i]]).Data = newStage;
                    }
                    break;
                }
            }

            using MemoryStream bancStream = new();
            stageBanc.Save(bancStream);

            sarcBuilderFileList.Add(
                ($"Banc/{versusSceneName}.bcett.byml", bancStream.ToArray().AsMemory())
            );
        }

        RandomizerUtil.CreateFolder("Pack");
        RandomizerUtil.DebugPrint("VS Stage handling complete");

        if (Options.GetOption("mismatchedStages"))
        {
            GameData.CommitToFileSystem($"RSDB/SceneInfo.Product.{GameData.GameVersion}.rstbl.byml.zs", sceneInfo.ToBytes().CompressZSTDBytes());
        }

        if (Options.GetOption("tweakStageLayouts"))
        {
            GameData.CommitToFileSystem("Pack/Params.pack.zs", SarcBuilder.Build(sarcBuilderFileList).CompressZSTDBytes());
        }
    }
}
