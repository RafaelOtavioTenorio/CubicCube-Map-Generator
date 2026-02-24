using System;
using System.Collections.Generic;

namespace Cubic_MapGenerator.Maps
{
    public class Rivers
    {
        public int[,] riverMap; // 0 = vazio, 1 = rio, 2 = rio forte
        private readonly int width;
        private readonly int height;
        private readonly int[,] continental;
        private readonly float[,] heightMap;
        private readonly int maxAttemptsPerSpawn = 300;
        private readonly float springHeightThreshold = 0.05f;
        private readonly int minSpringDistance = 35;
        private readonly int marginY = 120;

        private List<(int x, int y)> availableRiverEndings = new List<(int x, int y)>();
        private List<(int x, int y)> riverSprings = new List<(int x, int y)>();
        Seed seed;

        public Rivers(int[,] continentalData, float[,] heightData, Seed seed, int desiredRiverCount = -1)
        {
            this.seed = seed ?? throw new ArgumentNullException(nameof(seed));
            continental = continentalData ?? throw new ArgumentNullException(nameof(continentalData));
            heightMap = heightData ?? throw new ArgumentNullException(nameof(heightData));

            width = continental.GetLength(0);
            height = continental.GetLength(1);
            riverMap = new int[width, height];

            if (desiredRiverCount <= 0)
                desiredRiverCount = Math.Max(120, (width * height) / 9000);

            for (int x = 0; x < width; x++)
                for (int y = marginY; y < height - marginY; y++)
                    if (continental[x, y] == 3)
                        availableRiverEndings.Add((x, y));

            GenerateRivers(desiredRiverCount);
            StrengthenNetworks();
        }

        private void GenerateRivers(int riverCount)
        {
            var candidates = CollectHighLandCandidates(0.8f);
            Shuffle(candidates);

            int created = 0, idx = 0;

            while (created < riverCount && idx < candidates.Count)
            {
                var (sx, sy) = candidates[idx++];
                if (riverMap[sx, sy] != 0) continue;
                if (sy < marginY || sy >= height - marginY) continue;
                if (IsNearShallowSea(sx, sy, 3)) continue;
                if (!IsFarFromOtherSprings(sx, sy)) continue;

                CreateRiverFrom(sx, sy);
                riverSprings.Add((sx, sy));
                created++;
            }

            int attempts = 0;
            while (created < riverCount && attempts < maxAttemptsPerSpawn)
            {
                attempts++;

                int x = seed.ReturnIntValue(0, Math.Max(0, width - 1));
                int y = seed.ReturnIntValue(marginY, Math.Max(marginY, height - marginY - 1));

                if (x < 0 || x >= width || y < 0 || y >= height) continue; 
                if (continental[x, y] != 1) continue;
                if (heightMap[x, y] < springHeightThreshold) continue;
                if (riverMap[x, y] != 0) continue;
                if (!IsFarFromOtherSprings(x, y)) continue;

                CreateRiverFrom(x, y);
                riverSprings.Add((x, y));
                created++;
            }
        }

        private bool IsFarFromOtherSprings(int x, int y)
        {
            foreach (var (sx, sy) in riverSprings)
                if (Distance(x, y, sx, sy) < minSpringDistance)
                    return false;
            return true;
        }

        private List<(int x, int y)> CollectHighLandCandidates(float percentile)
        {
            var heights = new List<float>();
            for (int x = 0; x < width; x++)
                for (int y = marginY; y < height - marginY; y++)
                    if (continental[x, y] == 1)
                        heights.Add(heightMap[x, y]);

            if (heights.Count == 0) return new List<(int, int)>();

            heights.Sort();
            float cutoff = heights[Math.Clamp((int)(heights.Count * percentile), 0, heights.Count - 1)];

            var res = new List<(int, int)>();
            for (int x = 0; x < width; x++)
                for (int y = marginY; y < height - marginY; y++)
                    if (continental[x, y] == 1 && heightMap[x, y] >= cutoff)
                        res.Add((x, y));

            return res;
        }

        private void Shuffle<T>(List<T> list)
        {
            int n = list.Count;
            for (int i = 0; i < n - 1; i++)
            {
                int j = seed.ReturnIntValue(i, Math.Max(i, n - 1));
                (list[i], list[j]) = (list[j], list[i]);
            }
        }

        private bool IsNearShallowSea(int x, int y, int radius)
        {
            for (int dx = -radius; dx <= radius; dx++)
                for (int dy = -radius; dy <= radius; dy++)
                {
                    int nx = (x + dx + width) % width;
                    int ny = y + dy;
                    if (ny < marginY || ny >= height - marginY) continue;
                    if (continental[nx, ny] == 3) return true;
                }
            return false;
        }

        private void CreateRiverFrom(int startX, int startY)
        {
            int x = startX;
            int y = startY;
            var visited = new HashSet<int>();
            int maxIterations = width * height;
            int iterations = 0;

            riverMap[x, y] = 3;
            riverSprings.Add((x, y));

            if (availableRiverEndings.Count == 0) return;

            (int targetX, int targetY) = availableRiverEndings[0];
            float minDist = Distance(x, y, targetX, targetY);
            foreach (var s in availableRiverEndings)
            {
                float d = Distance(x, y, s.x, s.y);
                if (d < minDist)
                {
                    minDist = d;
                    targetX = s.x;
                    targetY = s.y;
                }
            }

            while (iterations++ < maxIterations)
            {
                if (iterations > 1)
                    riverMap[x, y] = Math.Min(2, riverMap[x, y] + 1);

                visited.Add(x * height + y);

                if (x == targetX && y == targetY)
                    return;

                (int nx, int ny, bool lowered) = BestDownhillNeighbor(x, y);
                if (lowered)
                {
                    if (ny >= marginY && ny < height - marginY)
                    {
                        x = nx; y = ny;
                        continue;
                    }
                }

                var candidates = new List<(int nx, int ny, float score)>();
                for (int dx = -1; dx <= 1; dx++)
                    for (int dy = -1; dy <= 1; dy++)
                    {
                        if (dx == 0 && dy == 0) continue;
                        int nx2 = (x + dx + width) % width;
                        int ny2 = y + dy;
                        if (ny2 < marginY || ny2 >= height - marginY) continue;
                        if (visited.Contains(nx2 * height + ny2)) continue;
                        float score = heightMap[nx2, ny2] + Distance(nx2, ny2, targetX, targetY);
                        candidates.Add((nx2, ny2, score));
                    }

                if (candidates.Count == 0) return;

                candidates.Sort((a, b) => a.score.CompareTo(b.score));
                int maxChoice = Math.Min(3, candidates.Count - 1);
                int choiceIndex = seed.ReturnIntValue(0, maxChoice);
                x = candidates[choiceIndex].nx;
                y = candidates[choiceIndex].ny;
            }
        }


        private (int x, int y, bool lowered) BestDownhillNeighbor(int x, int y)
        {
            float bestH = heightMap[x, y];
            int bestX = x, bestY = y;
            int[] dxs = { 1, -1, 0, 0 };
            int[] dys = { 0, 0, 1, -1 };

            for (int i = 0; i < 4; i++)
            {
                int nx = (x + dxs[i] + width) % width;
                int ny = y + dys[i];
                if (ny < marginY || ny >= height - marginY) continue;
                if (continental[nx, ny] != 1) continue;
                if (heightMap[nx, ny] < bestH)
                {
                    bestH = heightMap[nx, ny];
                    bestX = nx;
                    bestY = ny;
                }
            }

            return (bestX, bestY, !(bestX == x && bestY == y));
        }

        private float Distance(int x1, int y1, int x2, int y2)
        {
            int dx = Math.Min(Math.Abs(x2 - x1), width - Math.Abs(x2 - x1));
            int dy = y2 - y1;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        private void StrengthenNetworks()
        {
            for (int x = 0; x < width; x++)
                for (int y = marginY; y < height - marginY; y++)
                {
                    if (riverMap[x, y] == 0) continue;
                    int count = 0;
                    for (int dx = -1; dx <= 1; dx++)
                        for (int dy = -1; dy <= 1; dy++)
                        {
                            if (dx == 0 && dy == 0) continue;
                            int nx = (x + dx + width) % width;
                            int ny = y + dy;
                            if (ny < marginY || ny >= height - marginY) continue;
                            if (riverMap[nx, ny] > 0) count++;
                        }
                    if (count >= 6) riverMap[x, y] = 2;
                }
        }
    }
}
