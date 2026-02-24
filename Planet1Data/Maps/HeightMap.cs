using System;
using System.Collections.Generic;

namespace Cubic_MapGenerator.Maps
{
    public class HeightMap
    {
        public float[,] heightData; // positivo -> terra (0..~3), negativo -> oceano (0..-1)

        public HeightMap(ContinentalData continentalData, Erosion erosion)
        {
            int[,] auxContinentData = continentalData.continentalData;
            float[,] auxErosionData = erosion.erosionData;

            int width = auxContinentData.GetLength(0);
            int height = auxContinentData.GetLength(1);

            heightData = new float[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    heightData[x, y] = 0f;

            float minE = float.MaxValue, maxE = float.MinValue;
            bool hasLandErosion = false;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool isLand = auxContinentData[x, y] == 1 || auxContinentData[x, y] == 2;
                    if (!isLand) continue;
                    float v = auxErosionData[x, y];
                    if (float.IsNaN(v) || float.IsInfinity(v)) continue;
                    if (v < minE) minE = v;
                    if (v > maxE) maxE = v;
                    hasLandErosion = true;
                }
            }

            if (!hasLandErosion)
            {
                minE = 0f;
                maxE = 1f;
            }
            if (Math.Abs(maxE - minE) < 1e-6f)
            {
                maxE = minE + 1f;
            }

            float[,] oceanDist = ComputeDistanceToLand(auxContinentData);

            float maxOcean = 0f;
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (oceanDist[x, y] > maxOcean) maxOcean = oceanDist[x, y];

            if (maxOcean < 1e-6f) maxOcean = 1f;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool isLand = auxContinentData[x, y] == 1 || auxContinentData[x, y] == 2;
                    if (isLand)
                    {
                        float e = auxErosionData[x, y];
                        if (float.IsNaN(e) || float.IsInfinity(e)) e = minE;
                        float en = (e - minE) / (maxE - minE);
                        en = Clamp01(en);

                        float elevation = MapElevationFromErosion(en);

                        heightData[x, y] = elevation;
                    }
                    else
                    {
                        float dnorm = oceanDist[x, y] / maxOcean;
                        dnorm = Clamp01(dnorm);
                        float maxOceanDepth = 1.0f;
                        heightData[x, y] = -(dnorm * maxOceanDepth);
                    }
                }
            }
            SmoothHeight(heightData, 2, 1.0f);

        }

        private float MapElevationFromErosion(float en)
        {
            float inv = 1f - en;

            float v;
            if (inv <= 0.05f)
            {
                v = Lerp(0.00f, 0.06f, inv / 0.05f);
            }
            else if (inv <= 0.40f)
            {
                float t = (inv - 0.05f) / (0.35f);
                v = Lerp(0.06f, 0.30f, t);
            }
            else if (inv <= 0.75f)
            {
                float t = (inv - 0.40f) / (0.35f);
                v = Lerp(0.30f, 1.0f, t);
            }
            else
            {
                float t = (inv - 0.75f) / 0.25f;
                v = Lerp(1.0f, 3.0f, t);
            }

            return v;
        }

        private float[,] ComputeDistanceToLand(int[,] continental)
        {
            int width = continental.GetLength(0);
            int height = continental.GetLength(1);
            float[,] dist = new float[width, height];
            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    dist[x, y] = -1f;

            Queue<(int x, int y)> q = new Queue<(int x, int y)>();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    bool isLand = continental[x, y] == 1 || continental[x, y] == 2;
                    if (isLand)
                    {
                        q.Enqueue((x, y));
                        dist[x, y] = 0f;
                    }
                }
            }

            int[,] dirs = new int[,] { { 1, 0 }, { -1, 0 }, { 0, 1 }, { 0, -1 } };
            while (q.Count > 0)
            {
                var (cx, cy) = q.Dequeue();
                for (int i = 0; i < 4; i++)
                {
                    int nx = cx + dirs[i, 0];
                    int ny = cy + dirs[i, 1];
                    if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                    if (dist[nx, ny] != -1f) continue;
                    dist[nx, ny] = dist[cx, cy] + 1f;
                    q.Enqueue((nx, ny));
                }
            }

            return dist;
        }

        private void SmoothHeight(float[,] map, int iterations = 1, float strength = 1.0f)
        {
            int width = map.GetLength(0);
            int height = map.GetLength(1);

            for (int it = 0; it < iterations; it++)
            {
                float[,] tmp = new float[width, height];
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        float sum = 0f;
                        int count = 0;
                        for (int ox = -1; ox <= 1; ox++)
                        {
                            for (int oy = -1; oy <= 1; oy++)
                            {
                                int nx = x + ox;
                                int ny = y + oy;
                                if (nx < 0 || ny < 0 || nx >= width || ny >= height) continue;
                                sum += map[nx, ny];
                                count++;
                            }
                        }
                        float avg = sum / Math.Max(1, count);
                        tmp[x, y] = Lerp(map[x, y], avg, 0.5f * strength);
                    }
                }
                for (int x = 0; x < width; x++)
                    for (int y = 0; y < height; y++)
                        map[x, y] = tmp[x, y];
            }
        }

        private float Clamp01(float v)
        {
            if (v < 0f) return 0f;
            if (v > 1f) return 1f;
            return v;
        }

        private float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }
    }
}
