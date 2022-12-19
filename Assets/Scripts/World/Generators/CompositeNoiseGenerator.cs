using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompositeNoiseGenerator : IGenerator
{
    private int _SEED;

    private int height = 70;
    private int width = 1000;

    float perlinOffset;
    float perlinOffset2;
    float perlin2Offset;
    float perlin2Offset2;

    public CompositeNoiseGenerator(int seed)
    {
        _SEED = seed;

        Random.InitState(_SEED);
        perlinOffset = Random.Range(-9999f, 9999f);
        perlinOffset2 = Random.Range(-9999f, 9999f);
        perlin2Offset = Random.Range(-9999f, 9999f);
        perlin2Offset2 = Random.Range(-9999f, 9999f);
    }

    public int[,] Generate()
    {
        int[,] world = new int[height, width];
        float airUpperNoiseLimit = 0.55f;
        float airLowerNoiseLimit = 0.45f;

        /*
        for (int y = 0; y < world.GetLength(0); y++)
        {
            for (int x = 0; x < world.GetLength(1); x++)
            {
                float noise = GetCompositeNoise(x, y);
                if (noise >= airLowerNoiseLimit && noise <= airUpperNoiseLimit)
                {
                    world[y, x] = 0;
                }
                else
                {
                    world[y, x] = 1;
                }
            }
        }*/

        world = NoiseMap(world, 60);
        world = CellularAutomata(world, 5);

        return world;
    }

    private float GetCompositeNoise(int x, int y)
    {
        return PerlinNoise(x, y, 50f);
    }

    private int[,] NoiseMap(int[,] world, int fillPercent)
    {
        Random.InitState(_SEED + 1);

        for (int y = 0; y < world.GetLength(0); y++)
        {
            for (int x = 0; x < world.GetLength(1); x++)
            {
                world[y, x] = (Random.Range(0, 100) < fillPercent) ? 1 : 0;
            }
        }

        return world;
    }

    private float PerlinNoise(float x, float y, float scale)
    {
        return Mathf.PerlinNoise(x / (float)scale + perlinOffset, y / (float)scale + perlinOffset2);
    }

    private float PerlinNoise2(float x, float y, float scale)
    {
        return Mathf.PerlinNoise(x / (float)scale + perlin2Offset, y / (float)scale + perlin2Offset2);
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
                if (NeighbourWallCount(worldIn, i, j) > 4)
                {
                    worldOut[j, i] = 1;
                }
                else
                {
                    worldOut[j, i] = 0;
                }
            }
        }

        return CellularAutomata(worldOut, iterations - 1);
    }

    private int NeighbourWallCount(int[,] world, int i, int j)
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

        if (!upLocked && !leftLocked && world[j - 1, i - 1] == 1) { c++; }
        if (!leftLocked && world[j, i - 1] == 1) { c++; }
        if (!downLocked && !leftLocked && world[j + 1, i - 1] == 1) { c++; }
        if (!upLocked && world[j - 1, i] == 1) { c++; }
        if (!downLocked && world[j + 1, i] == 1) { c++; }
        if (!upLocked && !rightLocked && world[j - 1, i + 1] == 1) { c++; }
        if (!rightLocked && world[j, i + 1] == 1) { c++; }
        if (!downLocked && !rightLocked && world[j + 1, i + 1] == 1) { c++; }

        return c;
    }
}
