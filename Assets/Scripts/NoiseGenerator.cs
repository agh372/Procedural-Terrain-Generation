using SimplexNoise;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class NoiseGenerator
{
    public static float[,,] GenerateNoiseMap(int height, int width, float scale, int seed, int octaves, float persistance, float lacunarity, Vector2 offset)
    {
        float[,,] map = new float[width,height,width];      

        float halfWidth = width / 2f;
        float halfHeight = height / 2f;

        for (int x = 0; x < width; x++)
        {
            float noiseX = (float)x / 20;

            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < width; z++)
                {
                    float noiseZ = (float)z / 20;

                   
                    float amplitude = 1;
                    float frequency = 1;
                    float noiseHeight = 0;


                    float perlinValue = Mathf.PerlinNoise(noiseX, noiseZ) * 2 - 1;
                    noiseHeight += perlinValue * amplitude;

                    amplitude *= persistance;
                    frequency *= lacunarity;

                    noiseHeight += (10f - (float)y) / 10;

                    if (noiseHeight > 0.2f)
                        map[x, y, z] = 1;

                }
            }
        }


        return map;
    }

}
