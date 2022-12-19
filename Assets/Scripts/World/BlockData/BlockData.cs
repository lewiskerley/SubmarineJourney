using UnityEngine;

public abstract class BlockData
{
    //Store all this into 1 float in the future
    protected float health;
    protected float maxHealth;
    public BlockData(float maxHealth)
    {
        this.maxHealth = maxHealth;
        health = maxHealth;
    }

    public float GetHealthPercent()
    {
        if (maxHealth == 0 || health == 0) { return 0; }
        return (health / maxHealth) * 100;
    }

    public abstract int Damage(float damage);

    public Mesh GetMesh()
    {
        return BlockResources.i.GetMesh(this);
    }
}