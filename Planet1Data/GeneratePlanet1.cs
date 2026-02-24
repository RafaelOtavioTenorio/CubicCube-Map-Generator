using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cubic_MapGenerator.Planet1Data
{
    public class GeneratePlanet1
    {
        public List<List<int>> continentalData { get; set; }
        public List<List<float>> erosionData { get; set; }
        public List<List<float>> heightData { get; set; }
        public List<List<float>> humidityMap { get; set; }
        public List<List<int>> riverMap { get; set; }
        public List<List<int>> plateIndex { get; set; }
        public List<List<int>> temperatureData { get; set; }
        public List<List<string>> biomeData { get; set; }

        public GeneratePlanet1() { }
        public GeneratePlanet1(int[,] continentalData, float[,] erosionData, float[,] heightData, float[,] humidityMap, int[,] riverMap, int[,] plateIndex, int[,] temperatureData, string[,] biomeData)
        {
            this.continentalData = ToList2D(continentalData);
            this.erosionData = ToList2D(erosionData);
            this.heightData = ToList2D(heightData);
            this.humidityMap = ToList2D(humidityMap);
            this.riverMap = ToList2D(riverMap);
            this.plateIndex = ToList2D(plateIndex);
            this.temperatureData = ToList2D(temperatureData);
            this.biomeData = ToList2D(biomeData);
        }

        private List<List<int>> ToList2D(int[,] array)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            var res = new List<List<int>>(width);

            for(int x = 0; x < width; x++)
            {
                var row = new List<int>(height);
                for(int y = 0; y < height; y++)
                {
                    row.Add(array[x, y]);
                }

                res.Add(row);
            }
            return res;
        }
        private List<List<float>> ToList2D(float[,] array)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            var res = new List<List<float>>(width);

            for (int x = 0; x < width; x++)
            {
                var row = new List<float>(height);
                for (int y = 0; y < height; y++)
                {
                    row.Add(array[x, y]);
                }

                res.Add(row);
            }
            return res;
        }
        private List<List<string>> ToList2D(string[,] array)
        {
            int width = array.GetLength(0);
            int height = array.GetLength(1);
            var res = new List<List<string>>(width);

            for (int x = 0; x < width; x++)
            {
                var row = new List<string>(height);
                for (int y = 0; y < height; y++)
                {
                    row.Add(array[x, y]);
                }

                res.Add(row);
            }
            return res;
        }
    }
}
