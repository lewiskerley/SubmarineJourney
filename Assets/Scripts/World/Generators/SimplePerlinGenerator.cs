using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePerlinGenerator : IGenerator
{
    public int[,] Generate()
    {
        float offsetX = Random.Range(0f, 999f);
        float offsetY = Random.Range(0f, 999f);

        int[,] world = new int[21, 100];

        for (int i = 0; i < world.GetLength(0); i++)
        {
            for (int j = 0; j < world.GetLength(1); j++)
            {
                //Debug.Log(Mathf.PerlinNoise(i / 10f + offsetX, j / 10f + offsetY));
                if (Mathf.PerlinNoise(i / 10f + offsetX, j / 10f + offsetY) > 0.6f)
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
}
