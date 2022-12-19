using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PentlinGenerator : IGenerator
{
    private int _SEED;

    private int height = 300;
    private int width = 1000;
    [Range(1, 50)]
    private int caveMovementSmoothness = 50;
    [Range(1, 50)]
    private int caveLeftRightSmoothness = 20;

    private int maxLeftRight = 5;
    private static int gradientEstimationRange = 5; //Odd
    private int maxGradient = gradientEstimationRange - 1;

    private int viewTop = 80;
    private int viewHeight = 20;

    public PentlinGenerator(int seed)
    {
        if (gradientEstimationRange % 2 != 0) { gradientEstimationRange++; }
        _SEED = seed;
    }

    public int[,] Generate()
    {
        int[,] world = new int[height, width];

        Random.InitState(_SEED);


        //FIRST LAYER OF PERLIN NOISE:
        float perlinCaveOffset = Random.Range(-9999f, 9999f);
        float perlinCaveOffset2 = Random.Range(-9999f, 9999f);
        float perlinDoubleCaveOffset = Random.Range(-9999f, 9999f);
        float perlinDoubleCaveOffset2 = Random.Range(-9999f, 9999f);

        List<Vector2> viewPoints = new List<Vector2>();

        viewTop = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(0 / (float)caveMovementSmoothness + perlinCaveOffset, perlinCaveOffset2), 0, 1, 1, (height - 2) - viewHeight)); //viewTop should be viewHeight, but this looks AMAZING
        //world[viewTop, 0] = 1;
        viewPoints.Add(new Vector2(viewTop, 0));

        int lastViewTop = viewTop;
        //int i = 1;

        for (int x = 1; x < world.GetLength(1); x++)
        {
            //CAVE POSITION:
            viewTop = Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(x / (float)caveMovementSmoothness + perlinCaveOffset, perlinCaveOffset2), 0, 1, 1, (height - 2) - viewHeight)); //viewTop should be viewHeight, but this looks AMAZING

            if (lastViewTop < viewTop)
            {
                for (int y = lastViewTop + 1; y <= viewTop; y++)
                {
                    //world[y, x] = 1;
                    viewPoints.Add(new Vector2(y, x));
                    //i++;
                }
            }
            else if (lastViewTop > viewTop)
            {
                for (int y = lastViewTop - 1; y >= viewTop; y--)
                {
                    //world[y, x] = 1;
                    viewPoints.Add(new Vector2(y, x));
                    //i++;
                }
            }
            else
            {
                //world[viewTop, x] = 1;
                viewPoints.Add(new Vector2(viewTop, x));
                //i++;
            }

            lastViewTop = viewTop;
        }


        float[] gradients = new float[viewPoints.Count];

        //Gradients:
        int gradientRange = (int)((gradientEstimationRange - 1) / 2f);
        for (int n = gradientRange; n < viewPoints.Count - gradientRange; n++)
        {
            if (Mathf.Abs(viewPoints[n - gradientRange].y - viewPoints[n + gradientRange].y) == 0)
            {
                gradients[n] = maxGradient;
                continue;
            }
            gradients[n] = Mathf.Clamp(Mathf.Abs(viewPoints[n - gradientRange].x - viewPoints[n + gradientRange].x)
                / Mathf.Abs(viewPoints[n - gradientRange].y - viewPoints[n + gradientRange].y), -maxGradient, maxGradient);
        }

        List<Vector2> doublePerlinViewPoints = new List<Vector2>();
        doublePerlinViewPoints.Add(viewPoints[0]);

        //SECOND LAYER OF PERLIN NOISE (LEFT-RIGHT'ness):
        for (int n = 1; n < viewPoints.Count; n++)
        {
            //Add perlin noise on the NORMAL. Stronger noise based on steeper gradient
            Vector2 pushDirection = new Vector2(1, gradients[n]);
            float pushValueGradientEffector = RangeMap(Mathf.Abs(gradients[n]), 0, maxGradient, 0, 1);
            float pushValue = pushValueGradientEffector * Mathf.RoundToInt(RangeMap(Mathf.PerlinNoise(n / (float)caveLeftRightSmoothness + perlinDoubleCaveOffset, perlinDoubleCaveOffset2), 0, 1, -maxLeftRight, maxLeftRight));
            Vector2 newPoint = viewPoints[n] + (pushDirection * pushValue);
            newPoint = new Vector2(Mathf.RoundToInt(newPoint.x), Mathf.RoundToInt(newPoint.y));

            //TODO: Create full path
            doublePerlinViewPoints.Add(newPoint);
        }

        for (int x = 0; x < doublePerlinViewPoints.Count; x++)
        {
            if ((int)doublePerlinViewPoints[x].x >= 0 && (int)doublePerlinViewPoints[x].x < world.GetLength(0) &&
                (int)doublePerlinViewPoints[x].y >= 0 && (int)doublePerlinViewPoints[x].y < world.GetLength(1))
            {
                world[(int)doublePerlinViewPoints[x].x, (int)doublePerlinViewPoints[x].y] = 1;
            }
        }

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
