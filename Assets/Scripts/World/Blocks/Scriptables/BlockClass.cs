using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewBlock", menuName = "Blocks/Block")]
public class BlockClass : ScriptableObject
{
    public string blockName; // Unused as of now.
    public GameObject blockPrefab;

    // TODO: List of prefabs and their health thresholds.
    //  World.cs holds list of editted blocks (health, location, BlockClass from atlas)
}
