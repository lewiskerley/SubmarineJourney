using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RockEdgeRule")]
public class RockEdgeRule : ScriptableObject
{
    public TileType up;
    public TileType down;
    public TileType left;
    public TileType right;
    public GameObject[] prefabs;

    public bool IsMatch(TileType up, TileType down, TileType left, TileType right)
    {
        return this.up.HasFlag(up) && this.down.HasFlag(down) && this.left.HasFlag(left) && this.right.HasFlag(right);
    }

    public GameObject GetObject()
    {
        if (prefabs.Length == 0)
        {
            Debug.LogError("No Prefab Provided for " + this);
        }
        return prefabs[UnityEngine.Random.Range(0, prefabs.Length)];
    }
}

[Flags]
public enum TileType
{
    Nothing = 0,
    Edge = 1,
    Wall = 2,
    //4
}