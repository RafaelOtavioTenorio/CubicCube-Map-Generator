using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibNoise;
using LibNoise.Primitive;

namespace Cubic_MapGenerator
{
    public class Temperature
    {
        int[,] temperatureData;
        public int lowerTemp;
        public int higherTemp;
        Seed seed;

        public Temperature(int[] mapSize, Seed seed)
        {
            this.seed = seed;
            GenerateTemperatureMap(mapSize[0], mapSize[1]);
        }

        private void GenerateTemperatureMap(int width, int height)
        {
            int[,] data = new int[width, height];
            Stack<int> stack = new Stack<int>();
            int equator = height / 2;
            int start = 0;
            double tempRange = equator / 60;
            var perlin = new SimplexPerlin(seed.ReturnIntValue(0, 2000000), NoiseQuality.Standard);


            for (int y = 0; y < height; y++){
                int aux;
                if(y < equator)
                {
                    aux = (int)Math.Floor(Convert.ToDouble(y / tempRange));
                    for (int x = 0; x < width; x++)
                    {
                        if((aux - 30) <= 30)
                        {
                            data[x, y] = aux - 30 + GetTemperatureNoise(perlin, x, y);
                            
                        } else
                        {
                            data[x, y] = 30 + GetTemperatureNoise(perlin, x, y);
                        }
                        
                    }
                }
                else if(y < equator)
                {
                    //start == 0;
                    aux = (int)Math.Floor(Convert.ToDouble(start / tempRange));
                    for (int x = 0; x < width; x++)
                    {
                        if ((aux - 30) <= 30)
                        {
                            stack.Push(aux - 30 + GetTemperatureNoise(perlin, x, y));
                        }
                        else
                        {
                            stack.Push(30 + GetTemperatureNoise(perlin, x, y));
                        }
                    }
                } 
                else
                {
                    start++;
                    aux = (int)Math.Floor(Convert.ToDouble(start / tempRange));
                    for (int x = 0; x < width; x++)
                    {
                        if ((aux - 30) <= 30)
                        {
                            stack.Push(aux - 30 + GetTemperatureNoise(perlin, x, y));
                        }
                        else
                        {
                            stack.Push(30 + GetTemperatureNoise(perlin, x, y));
                        }
                    }
                }
            }
            for(int y = equator; y < height; y++)
            {
                for(int x = 0; x < width; x++)
                {
                    data[x,y] = stack.Pop();
                }
            }

            temperatureData = data;

            lowerTemp = data[0, 0];
            higherTemp = data[0, 0];

            foreach (int value in data)
            {

                if (value < lowerTemp)
                    lowerTemp = value;

                if(value > higherTemp)
                    higherTemp = value;
            }
        }

        private int GetTemperatureNoise(SimplexPerlin perlin, int x, int y)
        {
            double noiseValue = perlin.GetValue(x, y);
            double normalized = (noiseValue + 1.0) / 2.0 * 6.0;
            int category = (int)Math.Round(normalized);
            int finalValue = category - 3;
            return finalValue;
        }

        public int[,] GetTemperatureData()
        {
            return temperatureData;
        }
    }
}
