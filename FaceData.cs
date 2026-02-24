using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cubic_MapGenerator
{
    internal class FaceData
    {
        private bool[,] data;

        FaceData(bool[,] faceData)
        {
            data = faceData;
        }
    }
}
