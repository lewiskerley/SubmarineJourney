using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalBrownianGenerator : IGenerator
{
    private int _SEED;

    private int height = 200;
    private int width = 500;

    float perlinOffset;
    float perlinOffset2;

    public FractalBrownianGenerator(int seed)
    {
        _SEED = seed;

        Random.InitState(_SEED);
        perlinOffset = Random.Range(-9999f, 9999f);
        perlinOffset2 = Random.Range(-9999f, 9999f);
    }

    public int[,] Generate()
    {
        int[,] world = new int[height, width];
        float airUpperNoiseLimit = 0.55f;
        float airLowerNoiseLimit = 0.25f;

        for (int y = 0; y < world.GetLength(0); y++)
        {
            for (int x = 0; x < world.GetLength(1); x++)
            {
                float noise = FBM(x, y, 50);

                if (noise >= airLowerNoiseLimit && noise <= airUpperNoiseLimit)
                {
                    world[y, x] = 0;
                }
                else
                {
                    world[y, x] = 1;
                }
            }
        }

        world = CellularAutomata(world, 5);

        return world;
    }

    private float PerlinNoise(float x, float y, float scale)
    {
        return Mathf.PerlinNoise(x / (float)scale + perlinOffset, y / (float)scale + perlinOffset2);
    }
    private float PerlinNoiseAbs(float x, float y, float scale)
    {
        return Mathf.Abs(PerlinNoise(x, y, scale) - 0.5f) * 2;
    }

    private float octaves = 6;
    private float FBM(float x, float y, float scale)
    {
        float value = 0;
        float amp = 1f;

        for (int i = 0; i < octaves; i++)
        {
            value += amp * PerlinNoiseAbs(x, y, scale);
            x *= 2;
            y *= 2;
            amp *= 0.5f;
        }

        //Debug.Log(value);
        value *= value;

        return value;
    }


    private int[,] CellularAutomata(int[,] worldIn, int iterations)
    {
        if (iterations <= 0)
        {
            return worldIn;
        }

        int[,] worldOut = new int[worldIn.GetLength(0), worldIn.GetLength(1)];

        for (int i = 0; i < worldIn.GetLength(1); i++)
        {
            for (int j = 0; j < worldIn.GetLength(0); j++)
            {
                if (worldIn[j, i] == 1) //Solo / Duo Walls Removed
                {
                    if (NeighbourTypeCount(worldIn, i, j, 1) <= 1)
                    {
                        worldOut[j, i] = 0;
                    }
                    else
                    {
                        worldOut[j, i] = worldIn[j, i];
                    }
                }
                else if (worldIn[j, i] == 0) //Solo / Duo Air Pockets Filled
                {
                    if (NeighbourTypeCount(worldIn, i, j, 0) <= 1)
                    {
                        worldOut[j, i] = 1;
                    }
                    else
                    {
                        worldOut[j, i] = worldIn[j, i];
                    }
                }
            }
        }

        return CellularAutomata(worldOut, iterations - 1);
    }
    private int NeighbourTypeCount(int[,] world, int i, int j, int tile)
    {
        bool upLocked = j == 0;
        bool downLocked = j == world.GetLength(0) - 1;
        bool leftLocked = i == 0;
        bool rightLocked = i == world.GetLength(1) - 1;

        int c = 0;

        if (upLocked || downLocked)
        {
            c += 3;
            if (leftLocked || rightLocked)
            {
                c += 2;
            }
        }
        else if (leftLocked || rightLocked)
        {
            c += 3;
        }

        if (!upLocked && !leftLocked && world[j - 1, i - 1] == tile) { c++; }
        if (!leftLocked && world[j, i - 1] == tile) { c++; }
        if (!downLocked && !leftLocked && world[j + 1, i - 1] == tile) { c++; }
        if (!upLocked && world[j - 1, i] == tile) { c++; }
        if (!downLocked && world[j + 1, i] == tile) { c++; }
        if (!upLocked && !rightLocked && world[j - 1, i + 1] == tile) { c++; }
        if (!rightLocked && world[j, i + 1] == tile) { c++; }
        if (!downLocked && !rightLocked && world[j + 1, i + 1] == tile) { c++; }

        return c;
    }
}
