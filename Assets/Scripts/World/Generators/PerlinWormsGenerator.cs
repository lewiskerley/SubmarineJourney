using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PerlinWormsGenerator : IGenerator
{
    private List<Vector2> wormPath;
    private NoiseSettings noiseSettings = new NoiseSettings();
    private Vector2 currentDirection;
    private Vector2 currentPosition;
    //private float weight = 0.6f;

    public int[,] Generate()
    {
        int[,] world = SolidWorld();

        currentPosition = new Vector2(5, 10);
        currentDirection = Vector2.right;
        wormPath = new List<Vector2>();

        for (int i = 0; i < 900; i++)
        {
            Vector2 pos = Move();
            wormPath.Add(pos);
            Debug.Log(pos);
            if (Mathf.RoundToInt(pos.y) >= world.GetLength(0))
            {
                int continuationPoint = UnityEngine.Random.Range((wormPath.Count > 15 ? wormPath.Count - 15 : 0), wormPath.Count - 1);
                currentPosition = wormPath[continuationPoint];
                currentDirection = Vector2.right;

                wormPath = new List<Vector2>();
            }
            else if (Mathf.RoundToInt(pos.y) < 0)
            {
                int continuationPoint = UnityEngine.Random.Range((wormPath.Count > 15 ? wormPath.Count - 15 : 0), wormPath.Count - 1);
                currentPosition = wormPath[continuationPoint];
                currentDirection = Vector2.right;

                wormPath = new List<Vector2>();
            }
            else
            {
                world[Mathf.RoundToInt(pos.y), Mathf.RoundToInt(pos.x)] = 0;
            }
        }

        return world;
    }

    public Vector2 Move()
    {
        Vector3 dir = GetPerlinNoiseDirection();
        currentPosition += (Vector2)dir;
        return currentPosition;
    }

    private Vector3 GetPerlinNoiseDirection()
    {
        float noise = NoiseHelper.SumNoise(currentPosition.x, currentPosition.y, noiseSettings);
        float deg = NoiseHelper.RangeMap(noise, 0, 1, -90, 90);
        currentDirection = (Quaternion.AngleAxis(deg, Vector3.forward) * currentDirection).normalized;

        return currentDirection;
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


public class NoiseSettings
{
    [Min(1)]
    public int octaves = 3;
    [Min(0.001f)]
    public float startFrequency = 0.01f;
    [Min(0)]
    public float persistance = 0.5f;
}


public static class NoiseHelper
{

    public static float SumNoise(float x, float y, NoiseSettings noiseSettings)
    {
        float amplitude = 1;
        float frequency = noiseSettings.startFrequency;
        float noiseSum = 0;
        float amplitudeSum = 0;
        for (int i = 0; i < noiseSettings.octaves; i++)
        {
            noiseSum += amplitude * Mathf.PerlinNoise(x * frequency, y * frequency);
            amplitudeSum += amplitude;
            amplitude *= noiseSettings.persistance;
            frequency *= 2;

        }
        return noiseSum / amplitudeSum; // set range back to 0-1

    }

    public static float RangeMap(float inputValue, float inMin, float inMax, float outMin, float outMax)
    {
        return outMin + (inputValue - inMin) * (outMax - outMin) / (inMax - inMin);
    }

    public static List<Vector2Int> FindLocalMaxima(float[,] noiseMap)
    {
        List<Vector2Int> maximas = new List<Vector2Int>();
        for (int x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                var noiseVal = noiseMap[x, y];
                if (CheckNeighbours(x, y, noiseMap, (neighbourNoise) => neighbourNoise > noiseVal))
                {
                    maximas.Add(new Vector2Int(x, y));
                }

            }
        }
        return maximas;
    }

    public static List<Vector2Int> FindLocalMinima(float[,] noiseMap)
    {
        List<Vector2Int> minima = new List<Vector2Int>();
        for (int x = 0; x < noiseMap.GetLength(0); x++)
        {
            for (int y = 0; y < noiseMap.GetLength(1); y++)
            {
                var noiseVal = noiseMap[x, y];
                if (CheckNeighbours(x, y, noiseMap, (neighbourNoise) => neighbourNoise < noiseVal))
                {
                    minima.Add(new Vector2Int(x, y));
                }

            }
        }
        return minima;
    }

    static List<Vector2Int> directions = new List<Vector2Int>
    {
        new Vector2Int( 0, 1), //N
        new Vector2Int( 1, 1), //NE
        new Vector2Int( 1, 0), //E
        new Vector2Int(-1, 1), //SE
        new Vector2Int(-1, 0), //S
        new Vector2Int(-1,-1), //SW
        new Vector2Int( 0,-1), //W
        new Vector2Int( 1,-1)  //NW
    };

    private static bool CheckNeighbours(int x, int y, float[,] noiseMap, Func<float, bool> failCondition)
    {
        foreach (var dir in directions)
        {
            var newPost = new Vector2Int(x + dir.x, y + dir.y);

            if (newPost.x < 0 || newPost.x >= noiseMap.GetLength(0) || newPost.y < 0 || newPost.y >= noiseMap.GetLength(1))
            {
                continue;
            }

            if (failCondition(noiseMap[x + dir.x, y + dir.y]))
            {
                return false;
            }
        }
        return true;
    }

}

