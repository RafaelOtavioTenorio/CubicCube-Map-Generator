using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cubic_MapGenerator.Planet1Data.Maps
{
    public class BiomeMap
    {
        int width;
        int height;
        public string[,] biomeData;
        List<String> biomes = new List<String>();
        public BiomeMap(int[,] continentalData, float[,] erosionData, float[,] heightData, float[,] humidityMap, int[,] riverMap, int[,] plateIndex, int[,] temperatureData)
        {
            biomes.Add("Ocean");
            biomes.Add("Beach");
            biomes.Add("Sea");
            biomes.Add("River");
            biomes.Add("Lake");

            biomes.Add("Plains");
            biomes.Add("Plains_Hills");
            biomes.Add("Hills");
            biomes.Add("Mountains");

            biomes.Add("Taiga");
            biomes.Add("Forest");
            biomes.Add("Jungle");

            biomes.Add("Desert");
            biomes.Add("Desert_Hills");

            biomes.Add("Tundra");
            biomes.Add("Tundra_Hills");

            width = continentalData.GetLength(0);
            height = continentalData.GetLength(1);
            biomeData = new string[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    biomeData[x, y] = "";
                }
            }

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    SetBiome(x, y, continentalData[x, y], erosionData[x, y], heightData[x, y], humidityMap[x, y], riverMap[x, y], plateIndex[x, y], temperatureData[x, y]);
                }
            }
        }

        private void SetBiome(int x, int y, int continentalData, float erosionData, float heightData, float humidityData, int riverData, int plateData, int temperatureData)
        {
            if (biomeData[x, y] != "")
                return;

            if (continentalData == 0)
            {
                biomeData[x, y] = "Ocean";
                return;
            }
            else if (continentalData == 2)
            {
                biomeData[x, y] = "Beach";
                return;
            }
            else if (continentalData == 3)
            {
                biomeData[x, y] = "Sea";
                return;
            }

            // rios
            if (riverData == 1 || riverData == 2)
            {
                biomeData[x, y] = "River";
                return;
            }

            // montanhas
            if (heightData >= 2.01f)
            {
                biomeData[x, y] = "Mountains";
                return;
            }

            // colinas
            if (heightData >= 1.01f && heightData <= 2f)
            {
                if (humidityData <= 0.5f)
                {
                    if (temperatureData > 23)
                        biomeData[x, y] = "Desert_Hills";
                    else if (temperatureData > 0)
                        biomeData[x, y] = "Plains_Hills";
                    else
                        biomeData[x, y] = "Tundra_Hills";
                }
                else
                {
                    biomeData[x, y] = "Hills";
                }
                return;
            }

            // planícies e florestas
            if (heightData >= 0f && heightData <= 1f)
            {
                if (humidityData > 0.6f)
                {
                    if (temperatureData > 23)
                        biomeData[x, y] = "Jungle";
                    else if (temperatureData < 0)
                        biomeData[x, y] = "Taiga";
                    else
                        biomeData[x, y] = "Forest";
                }
                else if (humidityData > 0.3f)
                {
                    if (temperatureData > 0)
                        biomeData[x, y] = "Plains";
                    else
                        biomeData[x, y] = "Tundra";
                }
                else // seco demais
                {
                    biomeData[x, y] = "Desert";
                }
                return;
            }

            // fallback para qualquer célula que ainda ficou vazia
            biomeData[x, y] = "Plains";
        }

    }
}
