using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoublePerlinGenerator : IGenerator
{
    public int[,] Generate()
    {
        float offsetX = Random.Range(0f, 99999f);
        float offsetX2 = Random.Range(0f, 99999f);
        float offsetY = Random.Range(0f, 99999f);
        float offsetY2 = Random.Range(0f, 99999f);

        int[,] world = new int[21, 1000];

        for (int i = 0; i < world.GetLength(0); i++)
        {
            for (int j = 0; j < world.GetLength(1); j++)
            {
                //Debug.Log(Mathf.PerlinNoise(i / 10f + offsetX, j / 10f + offsetY));
                if (Mathf.PerlinNoise(i / 10f + offsetX, j / 10f + offsetY) * Mathf.PerlinNoise(i / 10f + offsetX2, j / 10f + offsetY2) > 0.2f)
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
