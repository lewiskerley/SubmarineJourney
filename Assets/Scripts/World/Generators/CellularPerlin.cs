using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellularPerlin : IGenerator
{
    private int _SEED;

    private int height = 70;
    private int width = 1000;

    [Range(1, 50)]
    private int caveMovementSmoothness = 35;
    [Range(1, 5)]
    public int roofReachDifficulty = 2; //2
    [Range(1, 9)]
    public int blockageDifficulty = 7; //7

    private int viewTop = 80;
    private int viewHeight = 20;

    public CellularPerlin(int seed)
    {
        _SEED = seed;
    }

    public int[,] Generate()
    {
        int[,] world = new int[height, width];

        Random.InitState(_SEED);
        float perlinTopOffset = Random.Range(-9999f, 9999f);
        float perlinTopOffset2 = Random.Range(-9999f, 9999f);
        float perlinBotOffset = Random.Range(-9999f, 9999f);
        float perlinBotOffset2 = Random.Range(-9999f, 9999f);
        float perlinCaveOffset = Random.Range(-9999f, 9999f);
        float perlinCaveOffset2 = Random.Range(-9999f, 9999f);

        for (int j = 0; j < world.GetLength(1); j++)
        {
            //CAVE POSITION:
            viewTop = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(j / (float)caveMovementSmoothness + perlinCaveOffset, perlinCaveOffset2), 0, 1, 1, (height - 2) - viewHeight));
            viewTop = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(j / (float)caveMovementSmoothness + perlinCaveOffset, perlinCaveOffset2), 0, 1, 1, (height - 2) - viewHeight)); //viewTop should be viewHeight, but this looks AMAZING

            //TOP CAVELINE:
            int topCaveLine = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(j / 5f + perlinTopOffset, perlinTopOffset2), 0, 1, viewTop, viewTop + ((viewHeight - 1) * ((roofReachDifficulty + 5) / 10f))));

            //BOT CAVELINE (RELATIVE):
            int botCaveLine = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(j / 10f + perlinBotOffset, perlinBotOffset2), 0, 1, Mathf.Clamp(topCaveLine - ((topCaveLine - viewTop) / (10 - blockageDifficulty)), viewTop, viewTop + (viewHeight - 1)), viewTop + viewHeight - 1));


            for (int i = 0; i < world.GetLength(0); i++)
            {
                if ((i > botCaveLine) || (i < topCaveLine))
                {
                    if (i == (topCaveLine - 1) && (botCaveLine <= topCaveLine)) //Avoid solid walls
                    {
                        world[i, j] = 0;
                    }
                    else
                    {
                        world[i, j] = 1;
                    }
                }

            }
        }

        int[,] shape = CopyWorld(world);

        world = NoiseOverEmptySpace(world, 30);
        world = CellularAutomata(world, 3);
        world = CombineShape(world, shape);

        return world;
    }


    public static float RangeMap(float inputValue, float inMin, float inMax, float outMin, float outMax)
    {
        if (outMin > outMax) //Weird case
        {
            return outMax + (inputValue - inMin) * (outMin - outMax) / (inMax - inMin);
        }
        return outMin + (inputValue - inMin) * (outMax - outMin) / (inMax - inMin);
    }

    private int[,] NoiseOverEmptySpace(int[,] world, int fillPercent)
    {
        Random.InitState(_SEED + 1);

        for (int y = 0; y < world.GetLength(0); y++)
        {
            for (int x = 0; x < world.GetLength(1); x++)
            {
                if (world[y, x] == 0)
                {
                    world[y, x] = (Random.Range(0, 100) < fillPercent) ? 1 : 0;
                }
            }
        }

        return world;
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

    private int[,] CombineShape(int[,] curWorld, int[,] shape)
    {
        for (int i = 0; i < curWorld.GetLength(1); i++)
        {
            for (int j = 0; j < curWorld.GetLength(0); j++)
            {
                if (shape[j, i] == 1)
                {
                    curWorld[j, i] = 1;
                }
            }
        }

        return curWorld;
    }
    private int[,] CopyWorld(int[,] world)
    {
        int[,] newWorld = new int[world.GetLength(0), world.GetLength(1)];

        for (int i = 0; i < newWorld.GetLength(1); i++)
        {
            for (int j = 0; j < newWorld.GetLength(0); j++)
            {
                newWorld[j, i] = world[j, i];
            }
        }

        return newWorld;
    }
}
