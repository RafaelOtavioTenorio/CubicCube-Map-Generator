using System;
using System.Collections.Generic;
using System.Numerics;
using LibNoise.Primitive;

namespace Cubic_MapGenerator
{
    public class Tectonics
    {
        List<Vector2> sites = new List<Vector2>();
        int[,] plateIndex;

        public bool[,] boundary;
        public int[,] interactionType;

        Seed seed;

        private SimplexPerlin noiseMacro;

        public Tectonics(int plateCount, int[] mapSize, Seed seed)
        {
            int width = mapSize[0];
            int height = mapSize[1];
            this.seed = seed;

            noiseMacro = new SimplexPerlin(seed.ReturnIntValue(0, 2000000), LibNoise.NoiseQuality.Standard);

            GenerateSites(plateCount);
            RelaxSites(mapSize, 3);

            GeneratePlates(width, height);
            SmoothPlateBorders(3);

            GenerateBoundaries(width, height);
            GenerateInteractions(width, height, plateCount);
        }

        private void GenerateSites(int count)
        {
            for (int i = 0; i < count; i++)
            {
                float x = seed.ReturnFloatValue(0.0f, 1.0f);
                float y = seed.ReturnFloatValue(0.0f, 1.0f);
                sites.Add(new Vector2(
                    x,
                    y
                ));
            }
        }

        private void RelaxSites(int[] size, int iterations)
        {
            int w = size[0];
            int h = size[1];

            for (int iter = 0; iter < iterations; iter++)
            {
                List<Vector2>[] regions = new List<Vector2>[sites.Count];
                for (int i = 0; i < regions.Length; i++)
                    regions[i] = new List<Vector2>();

                for (int x = 0; x < w; x++)
                {
                    float fx = (float)x / w;

                    for (int y = 0; y < h; y++)
                    {
                        float fy = (float)y / h;

                        int best = 0;
                        float bestDist = float.MaxValue;

                        for (int i = 0; i < sites.Count; i++)
                        {
                            float dx = fx - sites[i].X;
                            float dy = fy - sites[i].Y;
                            float d = dx * dx + dy * dy;

                            if (d < bestDist)
                            {
                                bestDist = d;
                                best = i;
                            }
                        }

                        regions[best].Add(new Vector2(fx, fy));
                    }
                }

                for (int i = 0; i < sites.Count; i++)
                {
                    if (regions[i].Count == 0) continue;

                    Vector2 avg = Vector2.Zero;
                    foreach (var p in regions[i])
                        avg += p;

                    sites[i] = avg / regions[i].Count;
                }
            }
        }

        private Vector2 ApplyJitter(int x, int y, int width, int height)
        {
            float nx = (float)x / width;
            float ny = (float)y / height;

            float freq = 3.5f;
            float strength = 0.08f;

            float jx = (float)noiseMacro.GetValue(nx * freq, ny * freq, 0);
            float jy = (float)noiseMacro.GetValue(nx * freq + 100, ny * freq + 50, 0);

            return new Vector2(
                nx + jx * strength,
                ny + jy * strength
            );
        }

        private void GeneratePlates(int width, int height)
        {
            plateIndex = new int[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector2 jitter = ApplyJitter(x, y, width, height);

                    int bestIndex = 0;
                    float bestDist = float.MaxValue;

                    for (int i = 0; i < sites.Count; i++)
                    {
                        float dx = jitter.X - sites[i].X;
                        float dy = jitter.Y - sites[i].Y;
                        float dist = dx * dx + dy * dy;

                        if (dist < bestDist)
                        {
                            bestDist = dist;
                            bestIndex = i;
                        }
                    }

                    plateIndex[x, y] = bestIndex;
                }
            }
        }

        private void SmoothPlateBorders(int passes)
        {
            int w = plateIndex.GetLength(0);
            int h = plateIndex.GetLength(1);

            for (int k = 0; k < passes; k++)
            {
                int[,] copy = (int[,])plateIndex.Clone();

                for (int x = 1; x < w - 1; x++)
                {
                    for (int y = 1; y < h - 1; y++)
                    {
                        int center = copy[x, y];
                        int diff = 0;

                        if (copy[x + 1, y] != center) diff++;
                        if (copy[x - 1, y] != center) diff++;
                        if (copy[x, y + 1] != center) diff++;
                        if (copy[x, y - 1] != center) diff++;

                        if (diff >= 3)
                        {
                            plateIndex[x, y] = copy[
                                x + seed.ReturnIntValue(-1, 2),
                                y + seed.ReturnIntValue(-1, 2)
                            ];
                        }
                    }
                }
            }
        }

        private void GenerateBoundaries(int width, int height)
        {
            boundary = new bool[width, height];

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    int p = plateIndex[x, y];

                    if (plateIndex[x + 1, y] != p ||
                        plateIndex[x - 1, y] != p ||
                        plateIndex[x, y + 1] != p ||
                        plateIndex[x, y - 1] != p)
                    {
                        boundary[x, y] = true;
                    }
                }
            }
        }

        private void GenerateInteractions(int width, int height, int numPlates)
        {
            interactionType = new int[width, height];

            int[] move = new int[numPlates];
            for (int i = 0; i < numPlates; i++)
                move[i] = seed.ReturnIntValue(1, 5);

            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    if (!boundary[x, y])
                        continue;

                    Compare(x, y, x + 1, y, move);
                    Compare(x, y, x - 1, y, move);
                    Compare(x, y, x, y + 1, move);
                    Compare(x, y, x, y - 1, move);
                }
            }
        }

        private void Compare(int ax, int ay, int bx, int by, int[] move)
        {
            if (bx < 0 || by < 0 ||
                bx >= plateIndex.GetLength(0) ||
                by >= plateIndex.GetLength(1))
                return;

            if (!boundary[bx, by]) return;

            int A = plateIndex[ax, ay];
            int B = plateIndex[bx, by];

            int dA = move[A];
            int dB = move[B];

            int type;

            if (dA == dB)
                type = 1; // transformante
            else if (
                (dA == 1 && dB == 2) || (dA == 2 && dB == 1) ||
                (dA == 3 && dB == 4) || (dA == 4 && dB == 3)
            )
                type = 2; // convergente
            else
                type = 3; // divergente

            interactionType[ax, ay] = type;
            interactionType[bx, by] = type;
        }

        public int[,] GetPlateIndex() => plateIndex;
    }
}
