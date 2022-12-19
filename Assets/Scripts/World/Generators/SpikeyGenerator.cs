using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpikeyGenerator : IGenerator
{
    public int[,] Generate()
    {
        int[,] world = new int[21, 100];
        int floorLevel = 5;
        int ceilingLevel = 15;

        //Ocean Floor / Ceiling Gen:
        for (int j = 0; j < world.GetLength(0); j++)
        {
            if (j > floorLevel && j < ceilingLevel)
            {
                continue;
            }

            for (int i = 0; i < world.GetLength(1); i++)
            {
                world[j, i] = 1;
            }
        }

        for (int j = 0; j < world.GetLength(1); j++)
        {
            if (Random.Range(0f, 1f) < 0.2f) //Floor
            {
                float randomPercent = Random.Range(0f, 1f);
                int length = Mathf.RoundToInt(randomPercent * ((ceilingLevel - floorLevel) - 2));
                int angle = Random.Range(0, 180);
                int thickness = Random.Range(2, 5);

                world = AngledRod(world, j, floorLevel, angle, length, true, thickness);
            }

            if (Random.Range(0f, 1f) < 0.2f) //Ceiling
            {
                float randomPercent = Random.Range(0f, 1f);
                int length = Mathf.RoundToInt(randomPercent * ((ceilingLevel - floorLevel) - 2));
                int angle = Random.Range(0, 180);
                int thickness = Random.Range(2, 5);

                world = AngledRod(world, j, ceilingLevel, angle, length, false, thickness);
            }

        }

        return world;
    }

    // width = (thickness - 1)*2 + 1
    private int[,] AngledRod(int[,] world, int x, int y, float angle, int length, bool up, int thickness)
    {
        int dir = (up ? 1 : -1);
        float xVar = 1f / (Mathf.Sin(angle * Mathf.Deg2Rad));
        float xPos = x;
        int xPosRounded;
        int yPos = y;
        int xLength = length;

        for (int i = 0; i < length; i++)
        {
            xPos = x + (xVar * i);
            yPos = y + (i * dir);
            xPosRounded = Mathf.RoundToInt(xPos);

            if (yPos >= world.GetLength(0) || yPos < 0 || xPosRounded < (thickness - 1) || xPosRounded >= world.GetLength(1) - (thickness - 1))
            {
                return world;
            }

            for (int extra = 0; extra < thickness; extra++)
            {
                world[yPos, xPosRounded + extra] = 1;
                world[yPos, xPosRounded - extra] = 1;
            }

            xLength--;

            if (xLength == 0)
            {
                return world;
            }
        }

        return world;
    }
}
