using System;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

namespace Cubic_MapGenerator
{
    public static class DataImageConverter
    {
        public static bool[,] ImageToData(int width, int height, List<int> selectedBlobs, Seed seed)
        {
            bool[,] data = new bool[width, height];
            string baseDir = AppContext.BaseDirectory;
            string projectDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", ".."));
            string path = "";

            if(width == 640)
            {
                int selectImage = seed.ReturnIntValue(1, 25);
                while (selectedBlobs.Contains(selectImage))
                {
                    if(selectImage <= selectedBlobs.Count)
                    {
                        selectImage++;
                    }
                    else
                    {
                        selectImage = 1;
                    }
                }
                path = Path.Combine(projectDir, "Blob", "BigBlob", $"big{selectImage}.png");
                //$"../Blob/BigBlob/big{selectImage}.png";
                selectedBlobs.Add(selectImage);
            }
            else if(width == 256)
            {
                int selectImage = seed.ReturnIntValue(1, 15);
                while (selectedBlobs.Contains(selectImage))
                {
                    if (selectImage <= selectedBlobs.Count)
                    {
                        selectImage++;
                    }
                    else
                    {
                        selectImage = 1;
                    }
                }
                path = Path.Combine(projectDir, "Blob", "SmallBlob", $"smol{selectImage}.png");
                //$"../Blob/BigBlob/big{selectImage}.png";
                selectedBlobs.Add(selectImage);
            } else
            {
                Console.WriteLine("Não foi possivel encontrar imagem do blob!");
                Console.ReadLine();
            }
            Bitmap img = new Bitmap(path);

            for(int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Color c = img.GetPixel(x % img.Width, y % img.Height);
                    double luminance = (0.2126 * c.R + 0.7152 * c.G + 0.0722 * c.B);

                    data[x, y] = luminance < 128;
                }
            }
            return data;
        }
    }
}
