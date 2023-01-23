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

    public int SEED { get; private set; }

    public World(int seed)
    {
        this.SEED = seed;
    }
}
