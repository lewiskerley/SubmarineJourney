public class BlockData_Rock : BlockData
{
    public BlockData_Rock(float maxHealth) : base(maxHealth)
    {
        //Extra Data Initialisation
    }

    public override int Damage(float damage)
    {
        health -= damage;
        if (health < 0)
        {
            health = 0;
            return 0;
        }

        return (int)((health / maxHealth) * 100);
    }
}