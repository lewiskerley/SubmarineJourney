using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Tier 0 Drill
public class Drill : Equipment
{
    public LayerMask targetLayers;
    public float damagePerTick;

    public override EquipmentItems GetItemType()
    {
        return EquipmentItems.Drill;
    }

    public override void UseWithCooldown()
    {
        //Debug.DrawRay(curPlayerHolding.transform.position, curPlayerHolding.transform.up, Color.red, 0.2f);

        //Raycast ahead +1 degree clockwise for diags?
        if (!Physics.Raycast(curPlayerHolding.transform.position, curPlayerHolding.transform.up, out RaycastHit hit, 0.6f, targetLayers))
        {
            return;
        }

        Vector2 hitPos = hit.collider.transform.position;
        Vector2Int hitPosInt = new Vector2Int((int)hitPos.x, (int)hitPos.y);
        //WorldData.instance.DamageBlockAtWorldPos((int)hitPos.x, (int)hitPos.y, damagePerTick, hit.collider.transform);
        WorldGeneration.i.UpdateBlockHealth(hit.collider.transform, hitPosInt, -damagePerTick);
    }
}
