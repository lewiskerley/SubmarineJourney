using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewBlock", menuName = "Blocks/Block")]
public class BlockClass : ScriptableObject
{
    public string blockName; // Unused as of now.
    public int maxHealth = 100;
    public BlockModelThreshold[] models;

    public GameObject GetModel(float healthPercent)
    {
        if (healthPercent == 0) { return null; }

        for (int i = 0; i < models.Length; i++)
        {
            if (healthPercent >= models[i].lowerBound)
            {
                return models[i].model;
            }
        }
        return null;
    }
    public GameObject GetDefaultModel()
    {
        return GetModel(100);
    }
    //  World.cs holds list of editted blocks (location, BlockClass from atlas)
}
