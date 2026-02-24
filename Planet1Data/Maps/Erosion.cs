using LibNoise.Primitive;
using System;
using System.Collections.Generic;

namespace Cubic_MapGenerator
{
    public  class Erosion
    {
        public float[,] erosionData;
        Seed seed;

        SimplexPerlin perlinNoise;

        public Erosion(int[] mapSize, Tectonics tectonics, Temperature temperature, Seed seed)
        {
            this.seed = seed;
            erosionData = new float[mapSize[0], mapSize[1]];
            for (int x = 0; x < mapSize[0]; x++)
            {
                for (int y = 0; y < mapSize[1]; y++)
                {
                    erosionData[x, y] = -1f;
                }
            }

            perlinNoise = new SimplexPerlin(seed.ReturnIntValue(0, int.MaxValue), LibNoise.NoiseQuality.Standard);

            GenerateErosionData(tectonics, temperature);
        }

        private void GenerateErosionData(Tectonics tectonics, Temperature temperature)
        {
            GeneratePeakData(tectonics);

            int width = erosionData.GetLength(0);
            int height = erosionData.GetLength(1);
            int[,] tempData = temperature.GetTemperatureData();

            Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
            float[,] dist = new float[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    dist[x, y] = -1f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (erosionData[x, y] == 0f)
                    {
                        queue.Enqueue((x, y));
                        dist[x, y] = 0f;
                    }
                }
            }

            if (queue.Count == 0)
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        if (tectonics.boundary[x, y])
                        {
                            queue.Enqueue((x, y));
                            dist[x, y] = 0f;
                            erosionData[x, y] = 0f;
                        }
                    }
                }
            }

            if (queue.Count == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    int sx = seed.ReturnIntValue(0, width);
                    int sy = seed.ReturnIntValue(0, height);
                    queue.Enqueue((sx, sy));
                    dist[sx, sy] = 0f;
                    erosionData[sx, sy] = 0f;
                }
            }

            int[,] dirs = new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
            float maxDist = 0f;

            while (queue.Count > 0)
            {
                var (x, y) = queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nx = x + dirs[i, 0];
                    int ny = y + dirs[i, 1];

                    if (nx < 0 || ny < 0 || nx >= width || ny >= height)
                        continue;

                    if (dist[nx, ny] != -1f)
                        continue;

                    dist[nx, ny] = dist[x, y] + 1f;
                    queue.Enqueue((nx, ny));

                    if (dist[nx, ny] > maxDist)
                        maxDist = dist[nx, ny];
                }
            }

            if (maxDist <= 0f) maxDist = 1f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float baseErosion = dist[x, y] < 0f ? 1f : (float)Math.Pow(dist[x, y] / maxDist, 0.8f);
                    float temp = tempData[x, y];

                    float erosion = baseErosion;
                    if (temp > 27f)
                    {
                        float heatBoost = InverseLerp(27f, 40f, temp);
                        erosion = Lerp(baseErosion, 1f, heatBoost * baseErosion);
                    }
                    else if (temp > -15f)
                    {
                        float adjustedTemp = temp + 15f;
                        erosion = baseErosion + (adjustedTemp * 0.002f);
                    }

                    float noise = GetNoise(x, y, width, height);
                    erosion = Lerp(erosion, erosion * noise, 0.5f);

                    erosionData[x, y] = Clamp01(erosion);
                }
            }

            SmoothErosionData();
        }

        private float GetNoise(int x, int y, int width, int height)
        {
            float nx = (float)x / width - 0.5f;
            float ny = (float)y / height - 0.5f;
            return (float)(perlinNoise.GetValue(nx * 5, ny * 5, 0) * 0.5 + 0.5f); // 0..1
        }

        private void SmoothErosionData()
        {
            int width = erosionData.GetLength(0);
            int height = erosionData.GetLength(1);
            float[,] temp = new float[width, height];

            int[,] dirs = new int[,] { { 0, 0 }, { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    float sum = 0f;
                    int count = 0;
                    for (int i = 0; i < dirs.GetLength(0); i++)
                    {
                        int nx = x + dirs[i, 0];
                        int ny = y + dirs[i, 1];
                        if (nx >= 0 && ny >= 0 && nx < width && ny < height)
                        {
                            sum += erosionData[nx, ny];
                            count++;
                        }
                    }
                    temp[x, y] = sum / count;
                }
            }

            erosionData = temp;
        }

        private float Clamp01(float v)
        {
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
        }

        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        private float InverseLerp(float a, float b, float v)
        {
            if (Math.Abs(a - b) < 1e-6f) return 0f;
            return (v - a) / (b - a);
        }

        private void GeneratePeakData(Tectonics tectonics)
        {
            int seeds = 0;
            for (int y = 0; y < erosionData.GetLength(1); y++)
            {
                for (int x = 0; x < erosionData.GetLength(0); x++)
                {
                    if (tectonics.boundary[x, y] && tectonics.interactionType[x, y] == 2)
                    {
                        erosionData[x, y] = 0f;
                        seeds++;
                    }
                }
            }

            if (seeds == 0)
            {
                for (int y = 0; y < erosionData.GetLength(1); y++)
                {
                    for (int x = 0; x < erosionData.GetLength(0); x++)
                    {
                        if (tectonics.boundary[x, y])
                        {
                            erosionData[x, y] = 0f;
                        }
                    }
                }
            }
        }
    }
}
