using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FractalBrownianMotionNoise
{
    private int octaves;
    private float lacunarity;
    private float persistence;
    private float scale;

    public FractalBrownianMotionNoise(int octaves, float lacunarity, float persistence, float scale)
    {
        this.octaves = octaves;
        this.lacunarity = lacunarity;
        this.persistence = persistence;
        this.scale = scale;
    }

    public float GetNoise(float x, float y)
    {
        float amplitude = 1f;
        float frequency = 1f;
        float noiseValue = 0f;

        for (int i = 0; i < octaves; i++)
        {
            float sampleX = x / scale * frequency;
            float sampleY = y / scale * frequency;

            float noise = Mathf.PerlinNoise(sampleX, sampleY) * 2f - 1f;
            noiseValue += noise * amplitude;

            amplitude *= persistence;
            frequency *= lacunarity;
        }

        return noiseValue;
    }
}