using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "NewBlockAtlas", menuName = "Blocks/Atlas")]
public class BlockAtlas : ScriptableObject
{
    public BlockClass rock;
    public BlockClass rockEdge; // Has no model

    public BlockClass stalagmiteBot;
    public BlockClass stalactiteTop;

    public BlockClass crystalTop;
    public BlockClass crystalBot;
    public BlockClass crystalLeft;
    public BlockClass crystalRight;
    public BlockClass crystalBack;
    public BlockClass crystalRootTop;
    public BlockClass crystalRootBot;
    public BlockClass crystalRootLeft;
    public BlockClass crystalRootRight;

    public BlockClass volcano;
}
