using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WorldData
{
    /*
    public static WorldData instance { get; private set; }

    float[,] map;
    float toughWallThreshold;
    Vector2Int offset;

    List<Equipment> equipmentList;

    Dictionary<Vector2Int, BlockData> worldData;
    //TODO: Add max capacity and revert blocks to default state after a while

    public static void Create(float[,] map, float toughWallThreshold, Vector2Int offset)
    {
        if (instance != null)
        {
            Debug.LogError("MULTIPLE WORLD DATA HOLDERS GENERATED!");
            return;
        }

        new WorldData(map, toughWallThreshold, offset);
    }

    private WorldData(float[,] map, float toughWallThreshold, Vector2Int offset)
    {
        this.map = map;
        this.toughWallThreshold = toughWallThreshold;
        this.offset = offset;
        worldData = new Dictionary<Vector2Int, BlockData>();
        equipmentList = new List<Equipment>();

        instance = this;
    }

    public float[,] GetMap()
    {
        return map;
    }

    public Vector2Int WorldToArrayPos(int x, int y)
    {
        Vector2Int withoutOffset = new Vector2Int(x, y) - offset;
        //Debug.Log(x + ", " + y + ": array: " + new Vector2Int(map.GetLength(0) - withoutOffset.y, withoutOffset.x));
        return new Vector2Int(map.GetLength(0) - withoutOffset.y, withoutOffset.x);
    }

    public void DamageBlockAtWorldPos(int x, int y, float damage, Transform obj)
    {
        BlockData blockData = GetOrCreateBlockData(WorldToArrayPos(x, y));
        int blockHealthPercent = blockData.Damage(damage);
        Debug.Log("Block Damage at: (" + x + ", " + y + ") - health: " + blockHealthPercent);

        //Update block mesh
        if (blockHealthPercent == 0)
        {
            WorldGeneration.i.DestroyTile(obj.gameObject);
        }
        else
        {
            UpdateBlockMesh(obj, blockData);
        }

        //Call to sound and particle systems!
        //Call to UI text popups?
    }

    public void UpdateBlockMesh(Transform blockObj, BlockData blockData)
    {
        WorldGeneration.i.UpdateBlockMesh(blockObj, blockData.GetMesh());
    }

    public BlockData GetOrCreateBlockData(Vector2Int arrayPos)
    {
        if (worldData.TryGetValue(arrayPos, out BlockData block))
        {
            return block;
        }

        return CreateBlockData(arrayPos);
    }

    public void AddEquipment(Equipment equipment)
    {
        equipmentList.Add(equipment);
    }

    public List<Equipment> GetEquipmentList()
    {
        return equipmentList;
    }

    private BlockData CreateBlockData(Vector2Int arrayPos)
    {
        BlockData data;
        float mapData = map[arrayPos.x, arrayPos.y];
        if (mapData > 0) //Rock Height
        {
            data = new BlockData_Rock(100);
        }
        else if (mapData == (int)GenFeatures.Edge)
        {
            data = new BlockData_Rock(60);
        }
        else
        {
            Debug.LogError("No Block Type with dataID: " + mapData);
            return null;
        }

        worldData.Add(arrayPos, data);
        return data;
    }
    */
}
