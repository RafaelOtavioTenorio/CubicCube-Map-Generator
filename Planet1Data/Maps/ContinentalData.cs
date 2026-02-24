using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Cubic_MapGenerator
{
    public class ContinentalData
    {
        public int[,] continentalData;
        //0 = Ocean
        //1 = Continent
        //2 = Coastline
        //3 = ShallowSea
        List<Vector2> continentCenters;

        List<int> selectedBig = new List<int>();
        List<int> selectedSmol = new List<int>();

        Seed seed;

        public ContinentalData(int[] mapSize, Seed seed)
        {
            this.seed = seed;
            continentalData = new int[mapSize[0], mapSize[1]];
            continentCenters = new List<Vector2>();
            GetContinentCenters(mapSize);

            for (int x = 0; x < continentalData.GetLength(0); x++)
            {
                for (int y = 0; y < continentalData.GetLength(1); y++)
                {
                    continentalData[x, y] = -1;
                }
            }

            PlaceContinentalData();
        }

        private void PlaceContinentalData()
        {
            int continentCounter = 21;

            while (continentCounter > 0)
            {
                byte type;

                if (continentCounter > 1)
                {
                    int selectType = seed.ReturnIntValue(1, 100);
                    if(selectType <= 70)
                    {
                        type = 1;
                    } else
                    {
                        type = 2;
                    }




                    type = (byte)seed.ReturnIntValue(1, 3);
                    ContinentBlob blob = new ContinentBlob(1, selectedBig, selectedSmol, seed);

                    if (blob.GetType() == 1)
                        continentCounter -= 2;
                    else
                        continentCounter--;

                    int randomIndex = seed.ReturnIntValue(0, continentCenters.Count -1);
                    Vector2 selectedCenter = continentCenters[randomIndex];
                    continentCenters.RemoveAt(randomIndex);

                    bool[,] blobData = blob.GetData();
                    int blobW = blobData.GetLength(0);
                    int blobH = blobData.GetLength(1);

                    int startX = (int)selectedCenter.X - blobW / 2;
                    int startY = (int)selectedCenter.Y - blobH / 2;

                    for (int i = 0; i < blobW; i++)
                    {
                        for (int j = 0; j < blobH; j++)
                        {
                            int mapX = startX + i;
                            int mapY = startY + j;

                            int width = continentalData.GetLength(0);
                            int height = continentalData.GetLength(1);

                            mapX = (mapX % width + width) % width;

                            if (mapY < 0 || mapY >= height)
                                continue;

                            int blobValue = blobData[i, j] ? 1 : 0;
                            continentalData[mapX, mapY] = Math.Max(continentalData[mapX, mapY], blobValue);

                        }
                    }
                }
                else
                {
                    type = 2;
                    ContinentBlob blob = new ContinentBlob(type, selectedBig, selectedSmol, seed);
                    continentCounter--;

                    int randomIndex = seed.ReturnIntValue(0, continentCenters.Count -1);
                    Vector2 selectedCenter = continentCenters[randomIndex];
                    continentCenters.RemoveAt(randomIndex);

                    bool[,] blobData = blob.GetData();
                    int blobW = blobData.GetLength(0);
                    int blobH = blobData.GetLength(1);

                    int startX = (int)selectedCenter.X - blobW / 2;
                    int startY = (int)selectedCenter.Y - blobH / 2;

                    for (int i = 0; i < blobW; i++)
                    {
                        for (int j = 0; j < blobH; j++)
                        {
                            int mapX = startX + i;
                            int mapY = startY + j;

                            int width = continentalData.GetLength(0);
                            int height = continentalData.GetLength(1);

                            mapX = (mapX % width + width) % width;

                            if (mapY < 0 || mapY >= height)
                                continue;

                            int blobValue = blobData[i, j] ? 1 : 0;
                            continentalData[mapX, mapY] = Math.Max(continentalData[mapX, mapY], blobValue);

                        }
                    }
                }
                for (int x = 0; x < continentalData.GetLength(0); x++)
                {
                    for (int y = 0; y < continentalData.GetLength(1); y++)
                    {
                        if (continentalData[x, y] == -1)
                            continentalData[x, y] = 0;
                    }
                }
            }

            //Minimum size
            RemoveSmallIslands(75);
            RemoveUnwantedIslands();
            //Number iterations
            SmoothCoastlines(3);
            GenerateCoastline();
            //Distance to fill
            FillNearCoastlines(8);
            GenerateCoastline();
            GenerateShallowSea(1, 7);


        }
        private void GetContinentCenters(int[] mapSize)
        {
            int xAxis = (mapSize[0] / 6);
            int yAxis = (mapSize[1] / 4);

            for(int x = 0; x < 7; x++)
            {
                for(int y = 0; y < 5; y++)
                {
                    if ((y == 0) || (y == 4))
                    {
                        if(x == 3)
                            continentCenters.Add(new Vector2(mapSize[0] / 2, y));
                    } else
                    {
                        continentCenters.Add(new Vector2(xAxis * x, yAxis * y));
                    }
                    
                }
            }
        }

        private void RemoveSmallIslands(int minSize)
        {
            int width = continentalData.GetLength(0);
            int height = continentalData.GetLength(1);

            bool[,] visited = new bool[width, height];
            Queue<(int x, int y)> queue = new Queue<(int, int)>();
            List<(int x, int y)> blob = new List<(int, int)>();

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (continentalData[x, y] != 1 || visited[x, y])
                        continue;

                    blob.Clear();
                    queue.Clear();

                    queue.Enqueue((x, y));
                    visited[x, y] = true;
                    blob.Add((x, y));

                    while (queue.Count > 0)
                    {
                        var (cx, cy) = queue.Dequeue();

                        for (int i = 0; i < 4; i++)
                        {
                            int nx = cx + dx[i];
                            int ny = cy + dy[i];

                            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                                continue;

                            if (!visited[nx, ny] && continentalData[nx, ny] == 1)
                            {
                                visited[nx, ny] = true;
                                queue.Enqueue((nx, ny));
                                blob.Add((nx, ny));
                            }
                        }
                    }

                    if (blob.Count < minSize)
                    {
                        foreach (var (bx, by) in blob)
                        {
                            continentalData[bx, by] = 0;
                        }
                    }
                }
            }
        }
        private void SmoothCoastlines(int iterations = 3)
        {
            int width = continentalData.GetLength(0);
            int height = continentalData.GetLength(1);

            int[,] temp = new int[width, height];

            int[] dx = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] dy = { -1, -1, -1, 0, 0, 1, 1, 1 };

            for (int iter = 0; iter < iterations; iter++)
            {
                Array.Copy(continentalData, temp, continentalData.Length);

                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        int waterCount = 0;
                        int landCount = 0;

                        for (int i = 0; i < 8; i++)
                        {
                            int nx = x + dx[i];
                            int ny = y + dy[i];

                            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                                continue;

                            if (continentalData[nx, ny] == 1) landCount++;
                            else waterCount++;
                        }

                        if (continentalData[x, y] == 1)
                        {
                            if (landCount <= 2)
                                temp[x, y] = 0;

                            else if (waterCount >= 6)
                                temp[x, y] = 0;
                        }
                        else
                        {
                            if (landCount >= 6)
                                temp[x, y] = 1;
                        }
                    }
                }

                Array.Copy(temp, continentalData, temp.Length);
            }
        }

        private void GenerateCoastline()
        {
            int width = continentalData.GetLength(0);
            int height = continentalData.GetLength(1);

            int[,] temp = (int[,])continentalData.Clone();

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (continentalData[x, y] != 1)
                        continue;

                    bool isCoast = false;

                    for (int i = 0; i < 4; i++)
                    {
                        int nx = x + dx[i];
                        int ny = y + dy[i];

                        if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                            continue;

                        if (continentalData[nx, ny] == 0)
                        {
                            isCoast = true;
                            break;
                        }
                    }

                    if (isCoast)
                        temp[x, y] = 2;
                }
            }

            continentalData = temp;
        }

        private void GenerateShallowSea(int distMin = 1, int distMax = 5)
        {
            int width = continentalData.GetLength(0);
            int height = continentalData.GetLength(1);

            int[,] dist = new int[width, height];
            Queue<(int x, int y)> queue = new();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    dist[x, y] = -1;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (continentalData[x, y] == 2)
                    {
                        queue.Enqueue((x, y));
                        dist[x, y] = 0;
                    }
                }
            }

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            while (queue.Count > 0)
            {
                var (cx, cy) = queue.Dequeue();

                for (int i = 0; i < 4; i++)
                {
                    int nx = cx + dx[i];
                    int ny = cy + dy[i];

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    if (dist[nx, ny] != -1)
                        continue;

                    dist[nx, ny] = dist[cx, cy] + 1;

                    if (continentalData[nx, ny] == 0)
                        queue.Enqueue((nx, ny));
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (continentalData[x, y] == 0 &&
                        dist[x, y] >= distMin && dist[x, y] <= distMax)
                    {
                        continentalData[x, y] = 3;
                    }
                }
            }
        }

        private void RemoveUnwantedIslands()
        {
            int width = continentalData.GetLength(0);
            int height = continentalData.GetLength(1);

            bool[,] visited = new bool[width, height];
            Queue<(int x, int y)> q = new();
            List<(int x, int y)> bloco = new();

            int[] dx = { 1, -1, 0, 0 };
            int[] dy = { 0, 0, 1, -1 };

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (continentalData[x, y] != 2 || visited[x, y])
                        continue;

                    bool temTerra = false;
                    bloco.Clear();
                    q.Clear();

                    q.Enqueue((x, y));
                    visited[x, y] = true;
                    bloco.Add((x, y));

                    while (q.Count > 0)
                    {
                        var (cx, cy) = q.Dequeue();

                        for (int i = 0; i < 4; i++)
                        {
                            int nx = cx + dx[i];
                            int ny = cy + dy[i];

                            if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                                continue;

                            if (continentalData[nx, ny] == 1)
                                temTerra = true;

                            if (!visited[nx, ny] && continentalData[nx, ny] == 2)
                            {
                                visited[nx, ny] = true;
                                q.Enqueue((nx, ny));
                                bloco.Add((nx, ny));
                            }
                        }
                    }

                    if (!temTerra)
                    {
                        foreach (var (bx, by) in bloco)
                            continentalData[bx, by] = 3;

                        FillIslandInterior(bloco);
                    }
                }
            }
        }

        private void FillIslandInterior(List<(int x, int y)> costa)
        {
            int width = continentalData.GetLength(0);
            int height = continentalData.GetLength(1);

            bool[,] borda = new bool[width, height];

            foreach (var (x, y) in costa)
                borda[x, y] = true;

            foreach (var (cx, cy) in costa)
            {
                int[] dx = { 1, -1, 0, 0 };
                int[] dy = { 0, 0, 1, -1 };

                for (int i = 0; i < 4; i++)
                {
                    int nx = cx + dx[i];
                    int ny = cy + dy[i];

                    if (nx < 0 || nx >= width || ny < 0 || ny >= height)
                        continue;

                    if (continentalData[nx, ny] == 0)
                        continentalData[nx, ny] = 3;
                }
            }
        }

        private void FillNearCoastlines(int maxDist = 20)
        {
            int width = continentalData.GetLength(0);
            int height = continentalData.GetLength(1);

            List<(int x, int y)> costas = new List<(int, int)>();

            for (int x = 0; x < width; x++)
                for (int y = 0; y < height; y++)
                    if (continentalData[x, y] == 2)
                        costas.Add((x, y));

            for (int i = 0; i < costas.Count; i++)
            {
                for (int j = i + 1; j < costas.Count; j++)
                {
                    var (x1, y1) = costas[i];
                    var (x2, y2) = costas[j];

                    int dx = x1 - x2;
                    int dy = y1 - y2;

                    int dist2 = dx * dx + dy * dy;

                    if (dist2 <= maxDist * maxDist)
                    {
                        FillBresenhamLine(x1, y1, x2, y2);
                    }
                }
            }
        }

        private void FillBresenhamLine(int x0, int y0, int x1, int y1)
        {
            int dx = Math.Abs(x1 - x0);
            int dy = Math.Abs(y1 - y0);
            int sx = (x0 < x1) ? 1 : -1;
            int sy = (y0 < y1) ? 1 : -1;
            int err = dx - dy;

            while (true)
            {
                if (x0 >= 0 && x0 < continentalData.GetLength(0) &&
                    y0 >= 0 && y0 < continentalData.GetLength(1))
                {
                    continentalData[x0, y0] = 1;
                }

                if (x0 == x1 && y0 == y1)
                    break;

                int e2 = 2 * err;
                if (e2 > -dy)
                {
                    err -= dy;
                    x0 += sx;
                }
                if (e2 < dx)
                {
                    err += dx;
                    y0 += sy;
                }
            }
        }
    }
}
