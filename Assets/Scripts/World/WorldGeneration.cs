using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class WorldGeneration : MonoBehaviour
{
    [Header("World Save/Load")]
    public static WorldGeneration i;
    private World worldData; // saved to [Application.persistentDataPath + "/worlds/"]
    public string worldSaveName;

    private const int maxMeshCountPerGroup = 200;

    [Header("World Settings")]
    public bool newSeed = false;
    //[Range(0, 9999999)] public int _SEED;

    public Vector2 playerSpawnPoint = new Vector2(-16, 36);
    public Vector2Int offset = new Vector2Int(-4, -3);
    public float baseHeight = 1;
    public float heightGrowth = 0.4f;
    public float heightLimit = 20;
    public float toughWallThreshold = 3;

    //public GameObject rockEdgeGenericPiece;
    [Header("Temp Visuals")]
    public Material wallMaterial;
    public Material toughWallMaterial;

    public RockEdgeRule[] rockEdgeRules;

    [Header("Drills")]
    public GameObject drillPrefab;

    [Header("Stalagmites / Stalactites")]
    public GameObject stalagmiteBotPrefab;
    public GameObject stalactiteTopPrefab;
    public float stalagmiteBotChance = 0.1f;
    public float stalactiteTopChance = 0.1f;
    public int stalSpawnMinWidth = 3;
    public int stalSpawnMinSpacing = 2;
    public Transform stalParent;

    [Header("Crystals")]
    public int blocksBetweenSingles = 30;
    public int blocksBetweenSinglesVariance = 5;
    public int blocksBetweenGroups = 200;
    public int blocksBetweenGroupsVariance = 30;
    public int groupEntryPathLength = 4;
    public int crystalsInGroup = 9;
    public GameObject crystalEdgeUp;
    public GameObject crystalEdgeDown;
    public GameObject crystalEdgeLeft;
    public GameObject crystalEdgeRight;
    public GameObject crystalRootUp;
    public GameObject crystalRootDown;
    public GameObject crystalRootLeft;
    public GameObject crystalRootRight;
    public GameObject crystalBack;
    public Transform crystalParent;

    [Header("Volcanoes")]
    public float volcanoTimeIdleToActive;
    public float volcanoTimeActivateToErruption;
    public GameObject volcano;
    public Transform volcanoParent;
    private List<Volcano> worldVolcanoes;

    [Header("Blockages")]
    public int blocksBetweenBlockage = 22;
    public int blocksBetweenBlockageVariance = 7;


    private void Awake()
    {
        if (i != null)
        {
            Debug.LogError("MULTIPLE WORLD GENERATORS IN PLAY!");
            return;
        }

        i = this;
    }

    void Start()
    {
        if (newSeed)
        {
            NewGame();
        }
        else
        {
            LoadGame();
        }

        //Debug.Log(worldData.GetPlayerPosition());
        //GameObject.FindObjectOfType<PlayerStateMachine>().transform.position = worldData.GetPlayerPosition(); // TODO: This should probably belong to the player somehow
        CreateWorld(new FractalBrownianGenerator(worldData.SEED));
        SpawnDrill();
        //StartCoroutine(VolcanoesLoop());
    }

    private void NewGame()
    {
        int newSeed = Random.Range(0, 9999999);
        this.worldData = new World(newSeed);

        Debug.Log("Successfully Created World! [Seed: " + worldData.SEED + "]");
    }
    private void LoadGame()
    {
        string fullPath = Path.Combine(Application.persistentDataPath + "/worlds/", worldSaveName);
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToLoad = "";
                using (FileStream stream = new FileStream(fullPath, FileMode.Open))
                {
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        dataToLoad = reader.ReadToEnd();
                    }
                }

                worldData = JsonUtility.FromJson<World>(dataToLoad);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error, cannot load data to file: " + fullPath + "\n" + e);
                // TODO: Have something in place rather than creating a new one. I.e interrupt.
            }
        }

        if (this.worldData == null)
        {
            Debug.LogWarning("No Saved World, Creating New World");
            NewGame();
            return;
        }

        Debug.Log("Successfully Loaded World! [Seed: " + worldData.SEED + "]");
    }
    private void SaveGame()
    {
        //worldData.UpdatePlayerPosition(GameObject.FindObjectOfType<PlayerStateMachine>().transform.position); // TODO: Update position (or player data) every few minutes

        string fullPath = Path.Combine(Application.persistentDataPath + "/worlds/", worldSaveName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
            string dataToStore = JsonUtility.ToJson(worldData, true);
            using (FileStream stream = new FileStream(fullPath, FileMode.Create))
            {
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    writer.Write(dataToStore);
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error, cannot save file to: " + fullPath + "\n" + e);
            return;
        }

        Debug.Log("Successfully Saved World! [Seed: " + worldData.SEED + "]");
    }
    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private GameObject GetRockEdgePiece(float[,] heightMap, int x, int y)
    {
        TileType up = GetTileType(heightMap, x, y - 1);
        TileType down = GetTileType(heightMap, x, y + 1);
        TileType left = GetTileType(heightMap, x - 1, y);
        TileType right = GetTileType(heightMap, x + 1, y);

        return GetRockEdgePiece(up, down, left, right);
    }

    private TileType GetTileType(float[,] heightMap, int x, int y)
    {
        TileType t;
        if (x < 0 || y < 0 || x >= heightMap.GetLength(1) || y >= heightMap.GetLength(0) || heightMap[y, x] == 0) { t = TileType.Nothing; }
        else if (heightMap[y, x] == (int)GenFeatures.Edge) { t = TileType.Edge; }
        else { t = TileType.Wall; }
        return t;
    }

    private GameObject GetRockEdgePiece(TileType up, TileType down, TileType left, TileType right)
    {
        for (int i = 0; i < rockEdgeRules.Length; i++)
        {
            RockEdgeRule r = rockEdgeRules[i];
            if (r.IsMatch(up, down, left, right))
            {
                return r.GetObject();
            }
        }

        Debug.LogError("No Rock Piece Found Matching:   (UP:" + up + ", DOWN:" + down + ", LEFT:" + left + ", RIGHT:" + right + ")");
        return null;
    }

    private void SpawnDrill()
    {
        //Temp Pos and Spawning
        GameObject drillObj = Instantiate(drillPrefab, new Vector3(-14, 35, 0), Quaternion.identity);
        //WorldData.instance.AddEquipment(drillObj.GetComponent<Equipment>()); TODO 1
    }

    private IEnumerator VolcanoesLoop()
    {
        WaitForSeconds delayBetween = new WaitForSeconds(volcanoTimeIdleToActive);
        WaitForSeconds delayDuring = new WaitForSeconds(volcanoTimeActivateToErruption);

        while (true)
        {
            yield return delayBetween;
            foreach (Volcano v in worldVolcanoes)
            {
                v.StartActiveEvent();
            }
            yield return delayDuring;
            foreach (Volcano v in worldVolcanoes)
            {
                v.StartErruptEvent();
            }
        }
    }


    private void CreateWorld(IGenerator generator)
    {
        int[,] worldArray = generator.Generate();
        worldArray = CreateTotalPathway(worldArray);
        //worldArray = CreateBlockages(worldArray);
        float[,] worldHeightMap = ConfigureHeightmap(worldArray);
        worldHeightMap = ConfigureCaveFeatures(worldHeightMap);

        Dictionary<string, Transform> meshHeightCurChunkParents = new Dictionary<string, Transform>();
        Dictionary<string, int> meshHeightChildCount = new Dictionary<string, int>();
        List<Transform> meshHeightParentsAll = new List<Transform>();

        worldVolcanoes = new List<Volcano>();

        for (int j = 0; j < worldArray.GetLength(1); j++)
        {
            for (int i = 0; i < worldArray.GetLength(0); i++)
            {
                float height = worldHeightMap[i, j];
                if (height == 0) { continue; }

                if (!meshHeightCurChunkParents.ContainsKey(height.ToString()))
                {
                    Transform t;
                    if (height == (int)GenFeatures.Edge) //Edge Piece
                    {
                        t = new GameObject("RockEdges").transform;
                        t.parent = transform;
                        t.gameObject.AddComponent<MeshRenderer>().sharedMaterial = wallMaterial;
                        t.gameObject.AddComponent<MeshFilter>();
                        //ADD TO MESHHEIGHTPARENTSALL IF YOU WANT TO MESH COMBINE THE EDGES
                        meshHeightCurChunkParents.Add(height.ToString(), t);
                        meshHeightChildCount.Add(height.ToString(), 0);
                    }
                    else
                    {
                        Transform newMesh = NewWallMeshParent(height);
                        meshHeightParentsAll.Add(newMesh);
                        meshHeightCurChunkParents.Add(height.ToString(), newMesh);
                        meshHeightChildCount.Add(height.ToString(), 0);
                    }
                }

                if (height == (int)GenFeatures.Edge) //Edge Piece
                {
                    GameObject prefab = GetRockEdgePiece(worldHeightMap, j, i);
                    if (prefab == null) { continue; }

                    GameObject obj = Instantiate(prefab, transform);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                    obj.transform.parent = meshHeightCurChunkParents[height.ToString()];
                    //obj.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0, 360));
                }
                else if (height == (int)GenFeatures.Stalagmite) //Stalagmite (BOT)
                {
                    GameObject obj = Instantiate(stalagmiteBotPrefab, stalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.Stalactite) //Stalactite (TOP)
                {
                    GameObject obj = Instantiate(stalactiteTopPrefab, stalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.CrystalUp) //Crystal (TOP)
                {
                    GameObject obj = Instantiate(crystalEdgeUp, crystalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.CrystalDown) //Crystal (BOT)
                {
                    GameObject obj = Instantiate(crystalEdgeDown, crystalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.CrystalLeft) //Crystal (LEFT)
                {
                    GameObject obj = Instantiate(crystalEdgeLeft, crystalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.CrystalRight) //Crystal (RIGHT)
                {
                    GameObject obj = Instantiate(crystalEdgeRight, crystalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.CrystalRootUp) //Crystal Root (TOP)
                {
                    GameObject obj = Instantiate(crystalRootUp, crystalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.CrystalRootDown) //Crystal Root (BOT)
                {
                    GameObject obj = Instantiate(crystalRootDown, crystalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.CrystalRootLeft) //Crystal Root (LEFT)
                {
                    GameObject obj = Instantiate(crystalRootLeft, crystalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.CrystalRootRight) //Crystal Root (RIGHT)
                {
                    GameObject obj = Instantiate(crystalRootRight, crystalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.CrystalBack) //Crystal (BACK)
                {
                    GameObject obj = Instantiate(crystalBack, crystalParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                }
                else if (height == (int)GenFeatures.Volcano) //Volcano
                {
                    GameObject obj = Instantiate(volcano, volcanoParent);
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                    worldVolcanoes.Add(obj.GetComponent<Volcano>());

                    obj.GetComponent<Volcano>().Initialise(j, worldArray.GetLength(0) - i);
                }
                else //Wall
                {
                    GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    //obj.GetComponent<MeshFilter>().sharedMesh = BlockResources.i.GetDefaultRockMesh(); TODO 2
                    //GameObject obj = Instantiate(BlockResources.i.GetDefaultRockMesh());
                    obj.transform.position = offset + new Vector2(j, worldArray.GetLength(0) - i);
                    obj.transform.parent = meshHeightCurChunkParents[height.ToString()];

                    int meshCount = meshHeightChildCount[height.ToString()] + 1;
                    meshHeightChildCount[height.ToString()] = meshCount;
                    if (meshCount >= maxMeshCountPerGroup)
                    {
                        Transform newMesh = NewWallMeshParent(height);
                        meshHeightParentsAll.Add(newMesh);
                        meshHeightCurChunkParents[height.ToString()] = newMesh;
                        meshHeightChildCount[height.ToString()] = 0;
                    }
                }
            }
        }

        //World data stored as an instance
        //WorldData.Create(worldHeightMap, toughWallThreshold, offset); TODO 3

        StartCoroutine(CombineAllMeshesNextFrame(meshHeightParentsAll));
    }

    IEnumerator CombineAllMeshesNextFrame(List<Transform> meshHeightParentsAll)
    {
        yield return new WaitForEndOfFrame();

        for (int i = 0; i < meshHeightParentsAll.Count; i++)
        {
            meshHeightParentsAll[i].gameObject.AddComponent<MeshCombiner>().Combine();
        }
    }

    private Dictionary<string, int> wallParentNamesCount = new Dictionary<string, int>();
    private Transform NewWallMeshParent(float height)
    {
        if (!wallParentNamesCount.ContainsKey("WallMesh-" + height))
        {
            wallParentNamesCount.Add("WallMesh-" + height, 1);
        }
        Transform t = new GameObject("WallMesh-" + (wallParentNamesCount["WallMesh-" + height]) + "-" + height).transform;
        wallParentNamesCount["WallMesh-" + height] += 1;
        t.parent = transform;
        t.localScale = new Vector3(1, 1, height);
        if (height < toughWallThreshold)
        {
            t.gameObject.AddComponent<MeshRenderer>().sharedMaterial = wallMaterial;
        }
        else
        {
            t.gameObject.AddComponent<MeshRenderer>().sharedMaterial = toughWallMaterial;
        }
        t.gameObject.AddComponent<MeshFilter>();
        return t;
    }

    // not in use
    private int[,] CreateBlockages(int[,] worldArray)
    {
        Random.InitState(worldData.SEED + 4);

        int nextBlockageIndex = blocksBetweenBlockage + Random.Range(-blocksBetweenBlockageVariance, blocksBetweenBlockageVariance + 1);
        for (int j = 0; j < worldArray.GetLength(1) - 50; j++)
        {
            if (j == nextBlockageIndex)
            {
                CreateBlockage1(worldArray, j, out int finishingIndex);
                nextBlockageIndex = finishingIndex + blocksBetweenBlockage + Random.Range(-blocksBetweenBlockageVariance, blocksBetweenBlockageVariance + 1);
                j = finishingIndex;
                continue;
            }
        }
        return worldArray;
    }
    private int[,] CreateBlockage1(int[,] worldArray, int startingIndex, out int finishingIndex)
    {
        int splitLength = 15;
        int splitLengthVariance = 3;
        int finalLength = splitLength + Random.Range(-splitLengthVariance, splitLengthVariance + 1);
        int endingIndex = startingIndex + finalLength;

        Vector2[] topside = new Vector2[finalLength + 1];
        Vector2[] botside = new Vector2[finalLength + 1];

        for (int j = startingIndex; j <= endingIndex; j++)
        {
            for (int i = 0; i < worldArray.GetLength(0); i++)
            {
                if (worldArray[i, j] == 0)
                {
                    topside[j - startingIndex] = new Vector2(i, j);
                    break;
                }
            }
            for (int i = worldArray.GetLength(0) - 1; i >= 0; i--)
            {
                if (worldArray[i, j] == 0)
                {
                    botside[j - startingIndex] = new Vector2(i, j);
                    break;
                }
            }
        }

        //Peaks
        Vector2 peakTop = new Vector2(-1, -1);
        Vector2 peakBot = new Vector2(int.MaxValue, int.MaxValue);
        for (int n = 0; n < finalLength; n++)
        {
            if (topside[n].x > peakTop.x)
            {
                peakTop = topside[n];
            }
            if (botside[n].x < peakBot.x)
            {
                peakBot = botside[n];
            }
        }

        //Closest to Peaks
        float peakTopDist = float.MaxValue;
        float peakBotDist = float.MaxValue;
        Vector2 peakTopPair = new Vector2(-1, -1);
        Vector2 peakBotPair = new Vector2(-1, -1);
        for (int n = 0; n < finalLength; n++)
        {
            if (Vector2.Distance(peakTop, botside[n]) < peakTopDist)
            {
                peakTopDist = Vector2.Distance(peakTop, botside[n]);
                peakTopPair = botside[n];
            }
            if (Vector2.Distance(peakBot, topside[n]) < peakBotDist)
            {
                peakBotDist = Vector2.Distance(peakTop, topside[n]);
                peakBotPair = topside[n];
            }
        }

        //Draw between Peak Pairs
        DrawLine(worldArray, peakBot, peakBotPair, 5, 1);
        DrawLine(worldArray, peakTop, peakTopPair, 5, 1);


        finishingIndex = endingIndex + 1;
        return worldArray;
    }

    private int[,] DrawLine(int[,] worldArray, Vector2 point1, Vector2 point2, int width, int fill)
    {
        Vector2 start;
        Vector2 end;
        if (point1.y < point2.y)
        {
            start = point1;
            end = point2;
        }
        else
        {
            start = point2;
            end = point1;
        }

        int y;
        float difY = (end.x - start.x) / (end.y - start.y);

        if (difY > 1)
        {
            //Switch:
            Vector2 startS = new Vector2(start.y, start.x);
            Vector2 endS = new Vector2(end.y, end.x);
            return DrawLine(worldArray, startS, endS, width, fill);
        }

        for (int j = (int)start.y; j <= (int)end.y; j++)
        {
            y = Mathf.RoundToInt(start.x + (difY * (j - (int)start.y)));

            if (y >= 0 && y < worldArray.GetLength(0) && j >= 0 && j < worldArray.GetLength(1))
            {
                worldArray[y, j] = fill;
            }

            for (int i = 0; i < width - 1; i++)
            {
                y = Mathf.RoundToInt(start.x + (difY * (j - (int)start.y)) + (0.5f * (i + 1)));
                if (y >= 0 && y < worldArray.GetLength(0) && j >= 0 && j < worldArray.GetLength(1))
                {
                    worldArray[y, j] = fill;
                }

                y = Mathf.RoundToInt(start.x + (difY * (j - (int)start.y)) + (-0.5f * (i + 1)));
                if (y >= 0 && y < worldArray.GetLength(0) && j >= 0 && j < worldArray.GetLength(1))
                {
                    worldArray[y, j] = fill;
                }
            }
        }

        return worldArray;
    }

    private int[,] CreateTotalPathway(int[,] worldArray)
    {
        for (int j = 0; j < worldArray.GetLength(1) - 1; j++)
        {
            bool foundPath = false;
            for (int i = 0; i < worldArray.GetLength(0); i++)
            {
                if (worldArray[i, j] == 0 && worldArray[i, j + 1] == 0)
                {
                    foundPath = true;
                    break;
                }
            }
            if (foundPath)
            {
                continue;
            }


            int nextAirTopIndex = -1;
            int nextAirBotIndex = -1;
            for (int i = 0; i < worldArray.GetLength(0); i++)
            {
                if (worldArray[i, j + 1] == 0)
                {
                    nextAirTopIndex = i;
                    break;
                }
            }
            for (int i = worldArray.GetLength(0) - 1; i >= 0; i--)
            {
                if (worldArray[i, j + 1] == 0)
                {
                    nextAirBotIndex = i;
                    break;
                }
            }

            int botToBotGap = 0;
            int topToBotGap = 0;
            int botToTopGap = 0;
            int topToTopGap = 0;
            int curAirBotIndex = -1;
            int curAirTopIndex = -1;
            if (nextAirTopIndex != -1)
            {
                for (int i = 0; i < worldArray.GetLength(0); i++)
                {
                    if (worldArray[i, j] == 0)
                    {
                        curAirBotIndex = i;
                        topToTopGap = Mathf.Abs(curAirBotIndex - nextAirTopIndex);
                        topToBotGap = Mathf.Abs(curAirBotIndex - nextAirBotIndex);
                        break;
                    }
                }
            }
            if (nextAirBotIndex != -1)
            {
                for (int i = worldArray.GetLength(0) - 1; i >= 0; i--)
                {
                    if (worldArray[i, j] == 0)
                    {
                        curAirTopIndex = i;
                        botToBotGap = Mathf.Abs(curAirTopIndex - nextAirBotIndex);
                        botToTopGap = Mathf.Abs(curAirTopIndex - nextAirTopIndex);
                        break;
                    }
                }
            }

            //TODO: Path through solid wall column (or leave them in?)
            if (botToBotGap != 0 && topToBotGap != 0 && botToTopGap != 0 && topToTopGap != 0) //If there's a jump between air gaps, create path.
            {
                if (botToBotGap < botToTopGap)
                {
                    if (botToBotGap < topToBotGap)
                    {
                        if (botToBotGap < topToTopGap)
                        {
                            //botToBotGap
                            worldArray = FillPath(worldArray, j + 1, curAirBotIndex, nextAirBotIndex);
                        }
                        else
                        {
                            //topToTopGap
                            worldArray = FillPath(worldArray, j + 1, curAirTopIndex, nextAirTopIndex);
                        }
                    }
                    else
                    {
                        if (topToBotGap < topToTopGap)
                        {
                            //topToBotGap
                            worldArray = FillPath(worldArray, j + 1, curAirTopIndex, nextAirBotIndex);
                        }
                        else
                        {
                            //topToTopGap
                            worldArray = FillPath(worldArray, j + 1, curAirTopIndex, nextAirTopIndex);
                        }
                    }
                }
                else
                {
                    if (botToTopGap < topToBotGap)
                    {
                        if (botToTopGap < topToTopGap)
                        {
                            //botToTopGap
                            worldArray = FillPath(worldArray, j + 1, curAirBotIndex, nextAirTopIndex);
                        }
                        else
                        {
                            //topToTopGap
                            worldArray = FillPath(worldArray, j + 1, curAirTopIndex, nextAirTopIndex);
                        }
                    }
                    else
                    {
                        if (topToBotGap < topToTopGap)
                        {
                            //topToBotGap
                            worldArray = FillPath(worldArray, j + 1, curAirTopIndex, nextAirBotIndex);
                        }
                        else
                        {
                            //topToTopGap
                            worldArray = FillPath(worldArray, j + 1, curAirTopIndex, nextAirTopIndex);
                        }
                    }
                }

            }
        }

        return worldArray;
    }

    private int[,] FillPath(int[,] worldArray, int j, int from, int to)
    {
        if (from < to)
        {
            worldArray[from, j] = 0;
            for (int n = 1; n <= to - from; n++)
            {
                worldArray[from + n, j] = 0;
            }
        }
        else
        {
            worldArray[from, j] = 0;
            for (int n = 1; n <= from - to; n++)
            {
                worldArray[from - n, j] = 0;
            }
        }

        return worldArray;
    }

    private float[,] ConfigureHeightmap(int[,] worldArray)
    {
        float[,] heights = new float[worldArray.GetLength(0), worldArray.GetLength(1)]; //By Default, no positive values are used.

        for (int i = 0; i < worldArray.GetLength(0); i++)
        {
            for (int j = 0; j < worldArray.GetLength(1); j++)
            {
                if (worldArray[i,j] == 1)
                {
                    heights[i, j] = -1;
                }
                else
                {
                    heights[i, j] = worldArray[i, j];
                }
            }
        }

        for (int i = 0; i < worldArray.GetLength(0); i++)
        {
            for (int j = 0; j < worldArray.GetLength(1); j++)
            {
                if (heights[i, j] == -1) //Wall
                {
                    if(AdjacentTo(i, j, heights, 0)) //Next to air
                    {
                        heights[i, j] = (int)GenFeatures.Edge; //Edge Piece
                    }
                }
            }
        }

        float curHighestRockHeight = (int)GenFeatures.Edge;
        bool foundWall = true;
        int iteration = 0;
        int nextToughRockJump = 2;

        while (foundWall)
        {
            foundWall = false;
            float nextHeight = baseHeight + (heightGrowth * iteration);
            if (Mathf.Approximately(nextHeight, toughWallThreshold))
            {
                iteration += 2; //Big leap to tough rock height
                nextHeight = baseHeight + (heightGrowth * iteration);

                iteration += 1;
                nextToughRockJump = 2; //Start
            }
            else if (nextHeight > toughWallThreshold)
            {
                iteration += nextToughRockJump;
                if (nextToughRockJump == 2)
                {
                    nextToughRockJump = 0;
                }
                else if(nextToughRockJump == 1)
                {
                    nextToughRockJump = 2;
                }
                else if (nextToughRockJump == 0)
                {
                    nextToughRockJump = 1;
                }
            }

            if (nextHeight >= heightLimit)
            {
                nextHeight = heightLimit;
            }

            for (int i = 0; i < worldArray.GetLength(0); i++)
            {
                for (int j = 0; j < worldArray.GetLength(1); j++)
                {
                    if (heights[i, j] == -1) //Wall
                    {
                        if (AdjacentToWithDiags(i, j, heights, curHighestRockHeight)) //Next to Recent Highest Piece
                        {
                            heights[i, j] = nextHeight; //Smooth Height Rock
                            foundWall = true;
                        }
                    }
                }
            }

            for (int i = 0; i < worldArray.GetLength(0); i++)
            {
                for (int j = 0; j < worldArray.GetLength(1); j++)
                {
                    if (heights[i, j] == nextHeight) //Wall
                    {
                        if (IsCornerPiece(i, j, heights)) //Next to Recent Highest Piece
                        {
                            heights[i, j] = nextHeight + (heightGrowth * 0.5f); //Smooth Height Rock
                        }
                        if (IsHigherCornerPiece(i, j, heights)) //Next to Recent Highest Piece
                        {
                            heights[i, j] = nextHeight - (heightGrowth * 0.5f); //Smooth Height Rock
                        }
                    }
                }
            }

            /*for (int i = 0; i < worldArray.GetLength(0); i++)
            {
                for (int j = 0; j < worldArray.GetLength(1); j++)
                {
                    if (heights[i, j] == nextHeight) //Wall
                    {
                        if (IsHigherCornerPiece(i, j, heights)) //Next to Recent Highest Piece
                        {
                            heights[i, j] = nextHeight - (heightGrowth * 0.5f); //Smooth Height Rock
                        }
                    }
                }
            }*/

            curHighestRockHeight = nextHeight;
            iteration++;
        }

        //Debug.Log(iteration);

        return heights;
    }

    private float[,] ConfigureCaveFeatures(float[,] worldArray)
    {
        worldArray = ConfigureStals(worldArray);
        worldArray = ConfigureCrystals(worldArray);
        worldArray = ConfigureVolcanoes(worldArray);

        return worldArray;
    }

    private float[,] ConfigureStals(float[,] worldArray) //CHANGE FOR NEW GENERATION! I.E MULTIPLE AIR POCKETS PER X
    {
        Random.InitState(worldData.SEED + 1);

        for (int j = 1; j < worldArray.GetLength(1) - 1; j++)
        {
            int airCount = 0;
            int startI = 0;

            for (int i = 0; i < worldArray.GetLength(0); i++)
            {
                if (worldArray[i, j] == (int)GenFeatures.Air)
                {
                    if (i == 0) //No more stals spawning on at y = 0
                    {
                        break;
                    }
                    if (airCount == 0)
                    {
                        startI = i;
                    }
                    airCount++;
                }
                else
                {
                    bool placed = false;
                    if (airCount >= stalSpawnMinWidth)
                    {
                        if (Random.Range(0f, 1f) < stalactiteTopChance)
                        {
                            if (startI > 0)
                            {
                                if (worldArray[startI - 1, j - 1] == (int)GenFeatures.Air || worldArray[startI - 1, j + 1] == (int)GenFeatures.Air)
                                {
                                    //Not a valid spot for a stal; skip
                                    startI = 0;
                                    airCount = 0;
                                    continue;
                                }
                                worldArray[startI - 1, j] = baseHeight - (heightGrowth / 2f);
                            }
                            worldArray[startI, j] = (int)GenFeatures.Stalactite;
                            placed = true;
                        }

                        if (Random.Range(0f, 1f) < stalagmiteBotChance)
                        {
                            if (worldArray[i, j - 1] == (int)GenFeatures.Air || worldArray[i, j + 1] == (int)GenFeatures.Air)
                            {
                                //Not a valid spot for a stal; skip
                                startI = 0;
                                airCount = 0;
                                if (placed)
                                {
                                    //Skip a few spaces right
                                    j += stalSpawnMinSpacing;
                                    break;
                                }
                                continue;
                            }
                            worldArray[i, j] = baseHeight - (heightGrowth / 2f);
                            worldArray[i - 1, j] = (int)GenFeatures.Stalagmite;
                            placed = true;
                        }
                    }

                    startI = 0;
                    airCount = 0;

                    if (placed)
                    {
                        //Skip a few spaces right
                        j += stalSpawnMinSpacing;
                        break;
                    }
                }
            }
        }

        return worldArray;
    }

    private float[,] ConfigureCrystals(float[,] worldArray)
    {
        Random.InitState(worldData.SEED + 2);

        int nextSingle = blocksBetweenSingles + Random.Range(-blocksBetweenSinglesVariance, blocksBetweenSinglesVariance + 1);
        int nextGroup = blocksBetweenGroups + Random.Range(-blocksBetweenGroupsVariance, blocksBetweenGroupsVariance + 1);

        for (int i = 1; i < (worldArray.GetLength(1) - 1) - 40; i++)
        {
            if (i >= nextSingle)
            {
                List<int> possibleEdges = new List<int>();
                for (int j = 1; j < worldArray.GetLength(0) - 1; j++)
                {
                    if (worldArray[j, i] == (int)GenFeatures.Edge)
                    {
                        possibleEdges.Add(j);
                    }
                }

                if (possibleEdges.Count == 0)
                {
                    nextSingle++;
                    continue;
                }
                else
                {
                    if (!PlaceCrystal(ref worldArray, i, possibleEdges[Random.Range(0, possibleEdges.Count)]))
                    {
                        nextSingle++;
                        continue;
                    }
                }

                //Successfully placed:
                nextSingle += blocksBetweenSingles + Random.Range(-blocksBetweenSinglesVariance, blocksBetweenSinglesVariance + 1);
            }

            /*
             * UNCOMMENT FOR CRYSTAL COVES
             * 
            if (i >= nextGroup)
            {
                List<int> possibleEdges = new List<int>();
                for (int j = 1; j < worldArray.GetLength(0) - 1; j++)
                {
                    if ((worldArray[j, i] == toughWallThreshold) && (j > 20 && j < worldArray.GetLength(0) - 21))
                    {
                        possibleEdges.Add(j);
                    }
                }

                if (possibleEdges.Count != 2 && possibleEdges.Count != 1)
                {
                    //Debug.Log("Interesting Generation, " + possibleEdges.Count + " tough wall thresholds?");
                    nextGroup++;
                    continue;
                }
                else
                {

                    if (!PlaceCrystalCove(ref worldArray, i, possibleEdges[Random.Range(0, possibleEdges.Count)]))
                    {
                        nextGroup++;
                        continue;
                    }
                }

                //Successfully placed:
                nextGroup += blocksBetweenGroups + Random.Range(-blocksBetweenGroupsVariance, blocksBetweenGroupsVariance + 1);
            }*/
        }

        return worldArray;
    }

    private float[,] ConfigureVolcanoes(float[,] worldArray)
    {
        for (int x = 1; x < worldArray.GetLength(1) - 1; x++)
        {
            for (int y = 1; y < worldArray.GetLength(0) - 1; y++)
            {
                if (worldArray[y, x] == (int)GenFeatures.Air) 
                {
                    if ((worldArray[y - 1, x - 1] == (int)GenFeatures.Air) &&
                        (worldArray[y, x - 1] == (int)GenFeatures.Air) &&
                        (worldArray[y + 1, x - 1] == (int)GenFeatures.Air) &&
                        (worldArray[y - 1, x + 1] == (int)GenFeatures.Air) &&
                        (worldArray[y, x + 1] == (int)GenFeatures.Air) &&
                        (worldArray[y + 1, x + 1] == (int)GenFeatures.Air) &&
                        (worldArray[y + 1, x] == (int)GenFeatures.Air) &&
                        (worldArray[y - 1, x] == (int)GenFeatures.Air)) //3x3 Air
                    {
                        worldArray[y, x] = (int)GenFeatures.Volcano; //TEMP: Only 1 volcano
                        return worldArray;
                    }
                }
            }
        }


        return worldArray;
    }

    //Assume up down left right in range of array
    private bool PlaceCrystal(ref float[,] worldArray, int x, int y)
    {
        //Prioritise LEFT and RIGHT since they are less common:
        if (worldArray[y, x - 1] == baseHeight && worldArray[y, x + 1] == (int)GenFeatures.Air)
        {
            worldArray[y, x] = (int)GenFeatures.CrystalLeft;
            worldArray[y, x - 1] = (int)GenFeatures.CrystalRootLeft;
            return true;
        }
        if (worldArray[y, x + 1] == baseHeight && worldArray[y, x - 1] == (int)GenFeatures.Air)
        {
            worldArray[y, x] = (int)GenFeatures.CrystalRight;
            worldArray[y, x + 1] = (int)GenFeatures.CrystalRootRight;
            return true;
        }
        //UP and DOWN:
        if (worldArray[y - 1, x] == baseHeight && worldArray[y + 1, x] == (int)GenFeatures.Air)
        {
            worldArray[y, x] = (int)GenFeatures.CrystalUp;
            worldArray[y - 1, x] = (int)GenFeatures.CrystalRootUp;
            return true;
        }
        if (worldArray[y + 1, x] == baseHeight && worldArray[y - 1, x] == (int)GenFeatures.Air)
        {
            worldArray[y, x] = (int)GenFeatures.CrystalDown;
            worldArray[y + 1, x] = (int)GenFeatures.CrystalRootDown;
            return true;
        }

        return false;
    }

    //Assume up down left right in range of array
    private bool PlaceCrystalCove(ref float[,] worldArray, int x, int y)
    {
        Random.InitState(worldData.SEED + 3);

        int leftCount = 0;
        int rightCount = 0;
        if (worldArray[y - 1, x - 1] >= toughWallThreshold) { leftCount++; }
        if (worldArray[y, x - 1] >= toughWallThreshold) { leftCount++; }
        if (worldArray[y + 1, x - 1] >= toughWallThreshold) { leftCount++; }
        if (worldArray[y - 1, x + 1] >= toughWallThreshold) { rightCount++; }
        if (worldArray[y, x + 1] >= toughWallThreshold) { rightCount++; }
        if (worldArray[y + 1, x + 1] >= toughWallThreshold) { rightCount++; }

        bool isUp = worldArray[y - 1, x] >= toughWallThreshold;
        bool isEqual = leftCount == rightCount;
        bool isLeft = leftCount > rightCount;

        if ((isUp && worldArray[y + 1, x] >= toughWallThreshold /*"and down", shouldnt be possible.*/) || (leftCount == 0) || (rightCount == 0) || y <= groupEntryPathLength || y >= worldArray.GetLength(0) - (groupEntryPathLength + 1))
        {
            return false;
        }

        worldArray[y, x] = toughWallThreshold - heightGrowth;
        worldArray[y, x + 1] = toughWallThreshold - heightGrowth;

        int dir = (isEqual ? 0 : (isLeft ? -1 : 1));
        float xPos = x;
        int yPos = y;
        int m = 1;

        if (isUp)
        {
            for (int n = 1; n <= groupEntryPathLength; n++)
            {
                yPos--;
                xPos += Mathf.Sin(135 * Mathf.Deg2Rad * dir);
                worldArray[yPos, Mathf.RoundToInt(xPos)] = toughWallThreshold - (heightGrowth * n);
                worldArray[yPos, Mathf.RoundToInt(xPos) + 1] = toughWallThreshold - (heightGrowth * n);
                if (n > groupEntryPathLength / 2)
                {
                    for (m = 2; m <= 2 + Random.Range(0, 4); m++)
                    {
                        TryAddInToughWall(worldArray, yPos, Mathf.RoundToInt(xPos) + m, toughWallThreshold + ((heightGrowth / 2f) * (-m - n)));
                    }
                }
            }

            int originX = Mathf.RoundToInt(xPos) + m - 1;
            List<Vector2> crystalSpots = new List<Vector2>();

            if (TryAddInToughWall(worldArray, yPos, x, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x, yPos)); }
            if (TryAddInToughWall(worldArray, yPos, x - 1, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x - 1, yPos)); }
            if (TryAddInToughWall(worldArray, yPos, x + 1, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x + 1, yPos)); }
            yPos--;

            for (int k = 0; k < 3; k++)
            {
                if (TryAddInToughWall(worldArray, yPos, x, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x, yPos)); }
                if (TryAddInToughWall(worldArray, yPos, x - 1, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x - 1, yPos)); }
                if (TryAddInToughWall(worldArray, yPos, x + 1, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x + 1, yPos)); }
                if (TryAddInToughWall(worldArray, yPos, x - 2, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x - 2, yPos)); }
                if (TryAddInToughWall(worldArray, yPos, x + 2, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x + 2, yPos)); }
                yPos--;
            }

            if (TryAddInToughWall(worldArray, yPos, x, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x, yPos)); }
            if (TryAddInToughWall(worldArray, yPos, x - 1, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x - 1, yPos)); }
            if (TryAddInToughWall(worldArray, yPos, x + 1, (int)GenFeatures.Air)) { crystalSpots.Add(new Vector2(x + 1, yPos)); }

            int timeout = 0;
            int crystalsInGroupTrue = crystalsInGroup;
            List<Vector2> crystalSpotsTaken = new List<Vector2>(crystalsInGroup);
            for (int k = 0; k < crystalsInGroup; k++)
            {
                Vector2 cPos = crystalSpots[Random.Range(0, crystalSpots.Count)];
                if (!crystalSpotsTaken.Contains(cPos))
                {
                    crystalSpotsTaken.Add(cPos);
                }
                else
                {
                    timeout++;
                    if (timeout > 500)
                    {
                        Debug.Log("Timeout on group crystals, locked at: " + k);
                        crystalsInGroupTrue = k;
                        break;
                    }
                    k--;
                    continue;
                }
            }
            for (int k = 0; k < crystalsInGroupTrue; k++)
            {
                Vector2 cPos = crystalSpotsTaken[k];
                worldArray[(int)cPos.y, (int)cPos.x] = (int)GenFeatures.CrystalBack; 
            }
        }
        else
        {
            for (int n = 1; n <= groupEntryPathLength; n++)
            {
                yPos++;
                xPos += Mathf.Sin(45 * Mathf.Deg2Rad * dir);
                worldArray[yPos, Mathf.RoundToInt(xPos)] = toughWallThreshold - (heightGrowth * n);
                worldArray[yPos, Mathf.RoundToInt(xPos) + 1] = toughWallThreshold - (heightGrowth * n);
                if (n > groupEntryPathLength / 2)
                {
                    for (m = 2; m <= 2 + Random.Range(0, 4); m++)
                    {
                        if (worldArray[yPos, Mathf.RoundToInt(xPos) + m] > toughWallThreshold)
                        {
                            worldArray[yPos, Mathf.RoundToInt(xPos) + m] = toughWallThreshold + ((heightGrowth / 2f) * (-m - n));
                        }
                    }
                }
            }

            int originX = Mathf.RoundToInt(xPos) + m - 1;
        }

        return true;
    }

    private bool TryAddInToughWall(float[,] array, int y, int x, float value)
    {
        if (y < 0 || y >= array.GetLength(0) || x < 0 || x >= array.GetLength(1))
        {
            return false;
        }
        if (array[y, x] <= toughWallThreshold)
        {
            return false;
        }
        array[y, x] = value;
        return true;
    }

    public void DestroyTile(GameObject block)
    {
        //Debug.Log("Destroyed: " + block.name + " " + block.transform.position);
        MeshCombiner chunk = block.GetComponentInParent<MeshCombiner>();
        Destroy(block);

        if (chunk != null) //Only recombine chunked objects (i.e rock edges are not chunked atm)
        {
            StartCoroutine(RecombineChunkNextFrame(chunk));
        }
    }
    public void UpdateBlockMesh(Transform blockObj, Mesh newMesh)
    {
        Vector3 oldPos = blockObj.position;
        Transform parent = blockObj.parent;

        MeshCombiner chunk = parent.GetComponentInParent<MeshCombiner>();

        //Update Mesh:
        /*Destroy(blockObj.gameObject);
        Instantiate(newMesh, oldPos, Quaternion.identity, parent);*/
        blockObj.GetComponent<MeshFilter>().sharedMesh = newMesh;

        if (chunk != null) //Only recombine chunked objects (i.e rock edges are not chunked atm)
        {
            StartCoroutine(RecombineChunkNextFrame(chunk));
        }
    }
    IEnumerator RecombineChunkNextFrame(MeshCombiner chunk)
    {
        yield return new WaitForEndOfFrame();
        chunk.Combine();
    }

    //======================== Helper functions for HeightMap ========================


    private bool AdjacentTo(int i, int j, float[,] world, float value)
    {
        if (i > 0)
        {
            if (world[i - 1, j] == value) //Left
            {
                return true;
            }
        }
        if (i < world.GetLength(0) - 1)
        {
            if (world[i + 1, j] == value) //Right
            {
                return true;
            }
        }

        if (j > 0)
        {
            if (world[i, j - 1] == value) //Down
            {
                return true;
            }
        }
        if (j < world.GetLength(1) - 1) //Up
        {
            if (world[i, j + 1] == value)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsHigherCornerPiece(int i, int j, float[,] world)
    {
        float value = world[i, j];

        if (i > 0)
        {
            //Debug.Log((world[i - 1, j] == value - heightGrowth) + ": " + world[i - 1, j] + " == " + (value - heightGrowth));
            if (Mathf.Approximately(world[i - 1, j], value - heightGrowth)) //Left
            {
                if (j > 0)
                {
                    if (Mathf.Approximately(world[i, j - 1], value - heightGrowth)) //Down
                    {
                        if (world[i - 1, j - 1] < value - heightGrowth || world[i - 1, j - 1] == 0 || world[i - 1, j - 1] == (int)GenFeatures.Edge) //Left Down
                        {
                            return true;
                        }
                    }
                }
                if (j < world.GetLength(1) - 1)
                {
                    if (Mathf.Approximately(world[i, j + 1], value - heightGrowth)) //Up
                    {
                        if (world[i - 1, j + 1] < value - heightGrowth || world[i - 1, j + 1] == 0 || world[i - 1, j + 1] == (int)GenFeatures.Edge) //Left Up
                        {
                            return true;
                        }
                    }
                }
            }
        }
        if (i < world.GetLength(0) - 1)
        {
            if (Mathf.Approximately(world[i + 1, j], value - heightGrowth)) //Right
            {
                if (j > 0)
                {
                    if (Mathf.Approximately(world[i, j - 1], value - heightGrowth)) //Down
                    {
                        if (world[i + 1, j - 1] < value - heightGrowth || world[i + 1, j - 1] == 0 || world[i + 1, j - 1] == (int)GenFeatures.Edge) //Right Down
                        {
                            return true;
                        }
                    }
                }
                if (j < world.GetLength(1) - 1)
                {
                    if (Mathf.Approximately(world[i, j + 1], value - heightGrowth)) //Up
                    {
                        if (world[i + 1, j + 1] < value - heightGrowth || world[i + 1, j + 1] == 0 || world[i + 1, j + 1] == (int)GenFeatures.Edge) //Right Up
                        {
                            return true;
                        }
                    }
                }
            }
        }


        return false;
    }

    private bool IsCornerPiece(int i, int j, float[,] world)
    {
        float value = world[i, j];

        if (i > 0)
        {
            if (world[i - 1, j] == value) //Left
            {
                if (j > 0)
                {
                    if (world[i, j - 1] == value) //Down
                    {
                        if (Mathf.Approximately(world[i - 1, j - 1], value - heightGrowth) || world[i - 1, j - 1] == 0 || world[i - 1, j - 1] == (int)GenFeatures.Edge) //Left Down
                        {
                            return true;
                        }
                    }
                }
                if (j < world.GetLength(1) - 1)
                {
                    if (world[i, j + 1] == value) //Up
                    {
                        if (Mathf.Approximately(world[i - 1, j + 1], value - heightGrowth) || world[i - 1, j + 1] == 0 || world[i - 1, j + 1] == (int)GenFeatures.Edge) //Left Up
                        {
                            return true;
                        }
                    }
                }
            }
        }
        if (i < world.GetLength(0) - 1)
        {
            if (world[i + 1, j] == value) //Right
            {
                if (j > 0)
                {
                    if (world[i, j - 1] == value) //Down
                    {
                        if (Mathf.Approximately(world[i + 1, j - 1], value - heightGrowth) || world[i + 1, j - 1] == 0 || world[i + 1, j - 1] == (int)GenFeatures.Edge) //Right Down
                        {
                            return true;
                        }
                    }
                }
                if (j < world.GetLength(1) - 1)
                {
                    if (world[i, j + 1] == value) //Up
                    {
                        if (Mathf.Approximately(world[i + 1, j + 1], value - heightGrowth) || world[i + 1, j + 1] == 0 || world[i + 1, j + 1] == (int)GenFeatures.Edge) //Right Up
                        {
                            return true;
                        }
                    }
                }
            }
        }


        return false;
    }

    private bool AdjacentToWithDiags(int i, int j, float[,] world, float value)
    {
        if (i > 0)
        {
            if (world[i - 1, j] == value) //Left
            {
                return true;
            }

            if (j > 0)
            {
                if (world[i - 1, j - 1] == value) //Left Down
                {
                    return true;
                }
            }
            if (j < world.GetLength(1) - 1)
            {
                if (world[i - 1, j + 1] == value) //Left Up
                {
                    return true;
                }
            }
        }
        if (i < world.GetLength(0) - 1)
        {
            if (world[i + 1, j] == value) //Right
            {
                return true;
            }

            if (j > 0)
            {
                if (world[i + 1, j - 1] == value) //Right Down
                {
                    return true;
                }
            }
            if (j < world.GetLength(1) - 1)
            {
                if (world[i + 1, j + 1] == value) //Right Up
                {
                    return true;
                }
            }
        }

        if (j > 0)
        {
            if (world[i, j - 1] == value) //Down
            {
                return true;
            }
        }
        if (j < world.GetLength(1) - 1) //Up
        {
            if (world[i, j + 1] == value)
            {
                return true;
            }
        }

        return false;
    }
}


public enum GenFeatures
{
    Air = 0,
    Edge = -2,
    Stalagmite = -3,
    Stalactite = -4,
    CrystalUp = -5,
    CrystalDown = -6,
    CrystalLeft = -7,
    CrystalRight = -8,
    CrystalRootUp = -9,
    CrystalRootDown = -10,
    CrystalRootLeft = -11,
    CrystalRootRight = -12,
    CrystalBack = -13,
    Volcano = -14
}