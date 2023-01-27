using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class BlockEdit
{
    public Vector2Int arrayPos;
    public BlockClass block;
    public float healthPercent;

    public BlockEdit(Vector2Int arrayPos, BlockClass block, float healthPercent)
    {
        this.arrayPos = arrayPos;
        this.block = block;
        this.healthPercent = healthPercent;
    }

    public GameObject GetModel()
    {
        return block.GetModel(healthPercent);
    }
    public void SetHealthPercent(float SetHealthPercent)
    {
        this.healthPercent = SetHealthPercent;
    }
}
