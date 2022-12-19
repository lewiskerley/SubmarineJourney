using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoublePerlinFateBallGenerator : IGenerator
{
    private int xStart = 10;
    private int yStart = 5;

    private int yTop = 40;
    private int yBot = 0;

    private int yEquil = 30;
    private float yStartVel = 0;

    private float fatesGravityAbs = 1;

    public int[,] Generate()
    {
        int[,] world = DoublePerlinGen(0.3f);
        //int[,] world = SolidWorld();

        world = UnleashTheFateBall(world);

        return world;
    }

    private int[,] UnleashTheFateBall(int[,] world)
    {
        int yPrev = yStart;
        int xPos = xStart;
        int yPos = yStart;
        float yVelocity = yStartVel;
        float fatesGravity = fatesGravityAbs;

        int turnOutGravityForIterations = 0;

        for (int i = 0; i < 900; i++)
        {
            fatesGravityAbs = Random.Range(0.2f, 1.2f);

            if (Random.Range(0f, 1f) < 0.05f)
            {
                turnOutGravityForIterations = Random.Range(2, 8);
                yVelocity /= 2f;
            }

            if (Mathf.RoundToInt(yPrev) < Mathf.RoundToInt(yPos))
            {
                for (int n = yPrev; n <= yPos; n++)
                {
                    FateBallDestruct(world, xPos, n);
                }
            }
            else
            {
                for (int n = yPrev; n >= yPos; n--)
                {
                    FateBallDestruct(world, xPos, n);
                }
            }

            xPos++;
            yPrev = yPos;
            yPos += Mathf.RoundToInt(yVelocity);
            if (yPos > yTop)
            {
                yPos = yTop;
                yVelocity = -fatesGravityAbs;
            }
            else if(yPos < yBot)
            {
                yPos = yBot;
                yVelocity = fatesGravityAbs;
            }

            if (turnOutGravityForIterations <= 0) //Gravity only if in wall
            {
                fatesGravity = yPos > yEquil ? -fatesGravityAbs : fatesGravityAbs;
                yVelocity += fatesGravity;
            }
            else
            {
                turnOutGravityForIterations--;
            }
        }

        return world;
    }

    private int[,] FateBallDestruct(int[,] world, int x, int y)
    {
        world[y, x] = 0;
        if (x > 0) { world[y, x - 1] = 0; }
        if (x < world.GetLength(1) - 1) { world[y, x + 1] = 0; }
        if (y < world.GetLength(0) - 2) { world[y + 1, x] = 0; world[y + 2, x] = 0; }
        if (y > 1) { world[y - 1, x] = 0; world[y - 2, x] = 0; }

        return world;
    }

    private int[,] DoublePerlinGen(float threshHold)
    {
        float offsetX = Random.Range(0f, 99999f);
        float offsetX2 = Random.Range(0f, 99999f);
        float offsetY = Random.Range(0f, 99999f);
        float offsetY2 = Random.Range(0f, 99999f);

        int[,] world = new int[(yTop + 1) - yBot, 1000];

        for (int i = 0; i < world.GetLength(0); i++)
        {
            for (int j = 0; j < world.GetLength(1); j++)
            {
                //Debug.Log(Mathf.PerlinNoise(i / 10f + offsetX, j / 10f + offsetY));
                if (Mathf.PerlinNoise(i / 10f + offsetX, j / 10f + offsetY) * Mathf.PerlinNoise(i / 10f + offsetX2, j / 10f + offsetY2) > threshHold)
                {
                    world[i, j] = 0;
                }
                else
                {
                    world[i, j] = 1;
                }
            }
        }

        return world;
    }

    private int[,] SolidWorld()
    {
        int[,] world = new int[21, 1000];

        for (int i = 0; i < world.GetLength(0); i++)
        {
            for (int j = 0; j < world.GetLength(1); j++)
            {
                world[i, j] = 1;
            }
        }

        return world;
    }
}
