using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WidthWormGenerator : IGenerator
{
    private int _SEED;

    private int height = 70;
    private int width = 1000;
    [Range(1, 50)]
    private int caveMovementSmoothness = 35;

    private int viewTop = 80;
    private int viewHeight = 20;

    [Range(1, 5)]
    public int roofReachDifficulty = 2; //2
    [Range(1, 9)]
    public int blockageDifficulty = 7; //7
    //[Range(0, 10)]
    //public int blockageDifficulty = 0;

    public WidthWormGenerator(int seed)
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
            //int perlinMaxHeight = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(j / 5f + perlinTopOffset, perlinTopOffset2), 0, 1, 0, height / 2f));
            //int perlinMinHeight = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(j / 5f + perlinBotOffset, perlinBotOffset2), 0, 1, height / 2f, height - 1));

            //int caveLine = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(j / 10f + perlinCavelineOffset, perlinCavelineOffset2), 0, 1, perlinMinHeight, perlinMaxHeight));


            //CAVE POSITION:
            viewTop = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(j / (float)caveMovementSmoothness + perlinCaveOffset, perlinCaveOffset2), 0, 1, 1, (height - 2) - viewHeight)); //viewTop should be viewHeight, but this looks AMAZING

            //TOP CAVELINE:
            int topCaveLine = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(j / 5f + perlinTopOffset, perlinTopOffset2), 0, 1, viewTop, viewTop + ((viewHeight - 1) * ((roofReachDifficulty + 5)/10f))));

            //BOT CAVELINE (RELATIVE):
            int botCaveLine = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(j / 10f + perlinBotOffset, perlinBotOffset2), 0, 1, Mathf.Clamp(topCaveLine - ((topCaveLine - viewTop) / (10 - blockageDifficulty)), viewTop, viewTop + (viewHeight - 1)), viewTop + viewHeight - 1));


            for (int i = 0; i < world.GetLength(0); i++)
            {
                if ((i > botCaveLine/* && i < botCaveLine + 35*/) || (i < topCaveLine/* && i > topCaveLine - 35*/))
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

        //WorldGeneration.i.Visualise(perlinCaveOffset, perlinCaveOffset2, height, viewHeight, caveMovementSmoothness);

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
}
