using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BlockResources : MonoBehaviour
{
    private static BlockResources _i;
    public static BlockResources i
    {
        get
        {
            if (_i == null) _i = Instantiate(Resources.Load<BlockResources>("BlockResources"));
            return _i;
        }
    }

    [Header("(Descending % and Lower Bound)")]
    [Header("Rock Mesh")]
    public HealthPercentBlockMesh[] ROCK_PERCENT_MESH;

    public Mesh GetMesh(BlockData data)
    {
        if (data is BlockData_Rock)
        {
            return GetMeshBasedOnHealthPercent(data.GetHealthPercent(), ROCK_PERCENT_MESH);
        }

        return null;
    }
    public Mesh GetDefaultRockMesh()
    {
        return ROCK_PERCENT_MESH[0].mesh;
    }

    public Mesh GetMeshBasedOnHealthPercent(float healthPercent, HealthPercentBlockMesh[] meshes)
    {
        if (healthPercent > meshes[0].lowerboundPercent)
        {
            return meshes[0].mesh;
        }

        for (int i = 0; i < meshes.Length; i++)
        {
            if (healthPercent < meshes[i].lowerboundPercent)
            {
                if (i + 1 == meshes.Length)
                {
                    Debug.LogError("No Mesh for health percent " + healthPercent + "% in " + meshes + " array!");
                    return null;
                }
                return meshes[i + 1].mesh;
            }
        }

        Debug.LogError("No Mesh for health percent " + healthPercent + "% in " + meshes + " array!");
        return null;
    }
}

[System.Serializable]
public struct HealthPercentBlockMesh
{
    public int lowerboundPercent;
    public Mesh mesh;
}