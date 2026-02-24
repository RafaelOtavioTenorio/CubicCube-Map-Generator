using System;
using System.Collections.Generic;

namespace Cubic_MapGenerator.Maps
{
    internal class Precipitation
    {
        public float[,] humidityMap;
        private readonly int width;
        private readonly int height;
        private readonly int[,] riverMap;        // 0 = vazio, 1 = rio, 2 = rio forte
        private readonly int[,] continentalData; // 0 = Ocean, 1 = Continent, 2 = Coastline, 3 = ShallowSea
        private readonly int[,] temperatureData;

        private readonly float maxDistanceInfluence = 100f;

        public Precipitation(int[,] riverMap, int[,] continentalData, int[,] temperatureData)
        {
            this.riverMap = riverMap;
            this.continentalData = continentalData;
            this.temperatureData = temperatureData;

            width = riverMap.GetLength(0);
            height = riverMap.GetLength(1);
            humidityMap = new float[width, height];

            GenerateHumidityMap();
        }

        private void GenerateHumidityMap()
        {
            Queue<(int x, int y)> queue = new Queue<(int x, int y)>();
            float[,] distanceMap = new float[width, height];

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    distanceMap[x, y] = float.MaxValue;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (continentalData[x, y] == 0 || continentalData[x, y] == 3 || riverMap[x, y] > 0)
                    {
                        distanceMap[x, y] = 0f;
                        queue.Enqueue((x, y));
                    }
                }
            }

            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();
                for (int dir = 0; dir < 4; dir++)
                {
                    int nx = cx + dx[dir];
                    int ny = cy + dy[dir];
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        float newDist = distanceMap[cx, cy] + 1f;
                        if (newDist < distanceMap[nx, ny])
                        {
                            distanceMap[nx, ny] = newDist;
                            queue.Enqueue((nx, ny));
                        }
                    }
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (continentalData[x, y] == 0 || continentalData[x, y] == 3)
                    {
                        humidityMap[x, y] = 1f;
                        continue;
                    }

                    float baseHumidity = 1f - Math.Clamp(distanceMap[x, y] / maxDistanceInfluence, 0f, 1f);
                    float tempFactor = (temperatureData[x, y] + 30f) / 60f;
                    humidityMap[x, y] = Math.Clamp(baseHumidity * tempFactor, 0f, 1f);
                }
            }
        }
    }
}