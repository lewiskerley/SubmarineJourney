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
    public Vector2 playerPosition;
    private Transform livePlayer;

    // 2 lists for each equipment type
    public List<Vector2> drillPositions;
    private List<Transform> liveDrills;

    public World(int seed)
    {
        this.SEED = seed;
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
