using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibNoise.Primitive;

namespace Cubic_MapGenerator
{
    internal class ContinentBlob
    {
        private bool[,] blobData;
        private byte type;
        Seed seed;

        public ContinentBlob(byte typeBlob, List<int> selectedBig, List<int> selectedSmol, Seed seed)
        {
            this.seed = seed;
            this.type = typeBlob;
            if (typeBlob == 1 /*BIG BLOB*/)
            {

                blobData = DataImageConverter.ImageToData(640, 480, selectedBig, seed);

            }
            else if (typeBlob == 2 /*SMOL BLOB*/)
            {
                blobData = DataImageConverter.ImageToData(256, 144, selectedSmol, seed);
            }
            ContinentGenerator();
        }

        private void ContinentGenerator()
        {
            bool[,] auxData = (bool[,])blobData.Clone();


            var perlin = new SimplexPerlin(seed.ReturnIntValue(0, 2000000), LibNoise.NoiseQuality.Standard);

            for(int i = 0; i < 30; i++)
            {
                bool[,] isEdge = new bool[auxData.GetLength(0), auxData.GetLength(1)];

                for (int x = 0; x < isEdge.GetLength(0); x++)
                {
                    for (int y = 0; y < isEdge.GetLength(1); y++)
                    {
                        bool p = auxData[x, y];

                        if (x > 0 && x < auxData.GetLength(0) - 1 &&y > 0 && y < auxData.GetLength(1) - 1)
                        {
                            if (auxData[x + 1, y] != p ||
                                auxData[x - 1, y] != p ||
                                auxData[x, y + 1] != p ||
                                auxData[x, y - 1] != p)
                            {
                                isEdge[x, y] = true;
                            }
                        }

                        double noiseValue = perlin.GetValue(x, y);

                        if(i < 1)
                        {
                            if (isEdge[x, y])
                            {
                                if(type == 1)
                                {
                                    if (noiseValue < 0.18)
                                    {
                                        auxData[x, y] = false;
                                    }
                                }
                                if (type == 2)
                                {
                                    if (noiseValue < 0.15)
                                    {
                                        auxData[x, y] = false;
                                    }
                                }
                            }
                        }

                        if (isEdge[x, y])
                        {
                            if (noiseValue < 0)
                            {
                                auxData[x, y] = false;
                            }
                        }
                    }
                }
            }

            blobData = auxData;
        }

        public byte? GetType()
        {
            if (type == null || type == 0)
                return null;
            return type;
        }

        public bool[,] GetData()
        {
            return blobData;
        }

    }
}
