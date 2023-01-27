using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class World
{
    // What is stored:
    // A seed to re-generate world
    // Changes to be applied post generation
    // Player, Equipment and other entity positions

    public int SEED;
    [SerializeField] public MapRow[] MAP; // Heightmap with negative features: GenFeatures
    public Vector2Int worldOffset;
    public BlockAtlas worldAtlas { get; private set; }

    public Vector2 playerPosition;
    private Transform livePlayer;

    // 2 lists for each equipment type
    public List<Vector2> drillPositions;
    private List<Transform> liveDrills;

    public List<BlockEdit> worldEdits; // New / Changed / Removed

    public World(int seed, float[,] map, Vector2Int worldOffset)
    {
        this.SEED = seed;
        this.MAP = new MapRow[map.GetLength(0)];
        for (int i = 0; i < map.GetLength(0); i++)
        {
            MAP[i] = new MapRow(map.GetLength(1));
            for (int j = 0; j < map.GetLength(1); j++)
            {
                MAP[i].row[j] = map[i, j];
            }
        }
        this.worldOffset = worldOffset;
        this.worldEdits = new List<BlockEdit>();
    }
    public void SetBlockAtlas(BlockAtlas atlas)
    {
        worldAtlas = atlas;
    }
    public void AddPlayer(Vector2 pos)
    {
        playerPosition = pos;
    }
    public void AddLivePlayer(Transform pos)
    {
        livePlayer = pos;
    }
    public void AddEquipment(EquipmentItems type, Vector2 pos)
    {
        if (type == EquipmentItems.Drill)
        {
            if (drillPositions == null) { drillPositions = new List<Vector2>(); }
            drillPositions.Add(pos);
        }
    }
    public void AddLiveEquipment(EquipmentItems type, Transform pos)
    {
        if (type == EquipmentItems.Drill)
        {
            if (liveDrills == null) { liveDrills = new List<Transform>(); }
            liveDrills.Add(pos);
        }
    }
    public List<Equipment> GetEquipmentList()
    {
        List<Equipment> equipment = new List<Equipment>();

        // Drills:
        foreach (Transform drill in liveDrills)
        {
            equipment.Add(drill.GetComponent<Equipment>());
        }

        return equipment;
    }

    public void UpdateBlock(Vector2Int worldPosition, float healthChange)
    {
        BlockEdit data = GetOrCreateBlockData(WorldToArrayPos(worldPosition));
        data.healthPercent += (healthChange / data.block.maxHealth) * 100;

        if (data.healthPercent > 100)
        {
            data.healthPercent = 100;
        }
        else if (data.healthPercent < 0)
        {
            data.healthPercent = 0;
        }
    }
    public Vector2Int WorldToArrayPos(Vector2Int pos)
    {
        Vector2Int withoutOffset = pos - worldOffset;
        return new Vector2Int(MAP.Length - withoutOffset.y, withoutOffset.x);
    }
    private BlockEdit GetOrCreateBlockData(Vector2Int arrayPos)
    {
        if (TryGetBlockData(arrayPos, out BlockEdit blockData))
        {
            return blockData;
        }

        return CreateBlockData(arrayPos);
    }
    private BlockEdit CreateBlockData(Vector2Int arrayPos)
    {
        BlockEdit data = new BlockEdit(arrayPos, GetBlockType(arrayPos), 100);
        worldEdits.Add(data);
        return data;
    }

    public GameObject GetModel(Vector2Int arrayPos)
    {
        if (TryGetBlockData(arrayPos, out BlockEdit blockData))
        {
            return blockData.GetModel(); // There's a different model
        }

        //Debug.Log(arrayPos + ": " + MAP[arrayPos.x].row[arrayPos.y]);
        return GetBlockType(arrayPos).GetDefaultModel(); // Default model
    }

    public BlockClass GetBlockType(Vector2Int arrayPos)
    {
        float mapData = MAP[arrayPos.x].row[arrayPos.y];

        BlockClass blockType;
        if (mapData > 0) //Rock Height
        {
            blockType = worldAtlas.rock;
        }
        else if (mapData == (int)GenFeatures.Edge)
        {
            blockType = worldAtlas.rockEdge; // The world generator does not use the models from this rockEdge class
        }
        else if (mapData == (int)GenFeatures.Stalagmite)
        {
            blockType = worldAtlas.stalagmiteBot;
        }
        else if (mapData == (int)GenFeatures.Stalactite)
        {
            blockType = worldAtlas.stalactiteTop;
        }
        else if (mapData == (int)GenFeatures.CrystalUp)
        {
            blockType = worldAtlas.crystalTop;
        }
        else if (mapData == (int)GenFeatures.CrystalDown)
        {
            blockType = worldAtlas.crystalBot;
        }
        else if (mapData == (int)GenFeatures.CrystalLeft)
        {
            blockType = worldAtlas.crystalLeft;
        }
        else if (mapData == (int)GenFeatures.CrystalRight)
        {
            blockType = worldAtlas.crystalRight;
        }
        else if (mapData == (int)GenFeatures.CrystalBack)
        {
            blockType = worldAtlas.crystalBack;
        }
        else if (mapData == (int)GenFeatures.CrystalRootUp)
        {
            blockType = worldAtlas.crystalRootTop;
        }
        else if (mapData == (int)GenFeatures.CrystalRootDown)
        {
            blockType = worldAtlas.crystalRootBot;
        }
        else if (mapData == (int)GenFeatures.CrystalRootLeft)
        {
            blockType = worldAtlas.crystalRootLeft;
        }
        else if (mapData == (int)GenFeatures.CrystalRootRight)
        {
            blockType = worldAtlas.crystalRootRight;
        }
        else if (mapData == (int)GenFeatures.Volcano)
        {
            blockType = worldAtlas.volcano;
        }
        else
        {
            Debug.LogError("No Block Type with dataID: " + mapData + " at " + arrayPos);
            return null;
        }
        return blockType;
    }

    public bool TryGetBlockData(Vector2Int arrayPos, out BlockEdit blockData)
    {
        for (int i = 0; i < worldEdits.Count; i++)
        {
            if (worldEdits[i].arrayPos == arrayPos)
            {
                blockData = worldEdits[i];
                return true;
            }
        }
        blockData = null;
        return false;
    }

    public void SyncEntityPositions()
    {
        drillPositions = new List<Vector2>();

        // Player
        playerPosition = livePlayer.position;

        // Drills
        foreach (Transform t in liveDrills)
        {
            drillPositions.Add(t.position);
        }
    }
}

[System.Serializable]
public struct MapRow
{
    public float[] row;

    public MapRow(int length)
    {
        this.row = new float[length];
    }
}