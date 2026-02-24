using Cubic_MapGenerator;
using Cubic_MapGenerator.Maps;
using Cubic_MapGenerator.Planet1Data;
using Cubic_MapGenerator.Planet1Data.Maps;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json;

int[] mapSize = { 1920, 1080 };

//Stopwatch sw = Stopwatch.StartNew();

string seedString = (args.Length > 0) ? args[0] : "";

Seed seed = new Seed("whatawonderfulworld");

//
// ======================
//   MAPA 1: PLACAS
// ======================
//
//Console.WriteLine("Generating Tectonics Map...");
var taskTectonics = Task.Run(() =>
{
    return new Tectonics(
        plateCount: 15,
        mapSize: mapSize,
        seed
    );
});

var taskTemperature = Task.Run(() =>
{
    return new Temperature(mapSize, seed);
});

Tectonics tectonics = await taskTectonics;
Temperature temperature = await taskTemperature;
int[,] plateIndex = tectonics.GetPlateIndex();
//SavePlateIndexImage(plateIndex, "plates_map.png");
//Console.WriteLine("Tectonics Map Generated!");
//
// ======================
//   MAPA 2: TEMPERATURA
// ======================
//
//Console.WriteLine("Generating Temperature Map...");

//SaveTemperatureImage(temperature, "temperature_map.png");
//Console.WriteLine("Temperature Map Generated!");
//
// ======================
//   MAPA 3: EROSÃO
// ======================
//
//Console.WriteLine("Generating Erosion Map...");
var taskErosion = Task.Run(() =>
{
    return new Erosion(mapSize, tectonics, temperature, seed);
});

var taskContData = Task.Run(() =>
{
    return new ContinentalData(mapSize, seed);
});

Erosion erosion = await taskErosion;
ContinentalData contData = await taskContData;
//Console.WriteLine("Erosion Map Generated!");
//
// ======================
//   MAPA 4: CONTINENTES
// ======================
//
//Console.WriteLine("Generating Continents Map...");

//SaveContinentalImage(contData.continentalData, "continental_map.png");
//Console.WriteLine("Continents Map Generated!");
//
// ======================
//   MAPA 5: ALTITUDE (HeightMap Final)
// ======================
//
//Console.WriteLine("Generating height Map...");
HeightMap heightMap = await Task.Run(() =>
        new HeightMap(contData, erosion)
    );
//SaveHeightImage(heightMap.heightData, "height_map.png");
//Console.WriteLine("Height Map Generated!");
//
// ======================
//   MAPA 6: RIOS
// ======================
//
//Console.WriteLine("Generating Rivers Map...");
Rivers riversMap = await Task.Run(() =>
        new Rivers(contData.continentalData, heightMap.heightData, seed)
    );
//SaveRiversImage(
//    contData.continentalData,
//    riversMap.riverMap,
//    "rivers_map.png"
//);
//Console.WriteLine("Rivers Map Generated!");

//
// ======================
//   MAPA 7: Precipitação
// ======================
//

//Console.WriteLine("Generating Precipitation Map...");
Precipitation precipitationMap = await Task.Run(() =>
        new Precipitation(
            riversMap.riverMap,
            contData.continentalData,
            temperature.GetTemperatureData()
        )
    );
//SavePrecipitationImage(
//    precipitationMap.humidityMap,
//    "precipitation_map.png"
//);
//Console.WriteLine("Precipitation Map Generated!");

//
// ======================
//   MAPA 8: MAPA FINAL
// ======================
//
//Console.WriteLine("Generating Biomes...");
BiomeMap biome = await Task.Run(() =>
        new BiomeMap(
            contData.continentalData,
            erosion.erosionData,
            heightMap.heightData,
            precipitationMap.humidityMap,
            riversMap.riverMap,
            plateIndex,
            temperature.GetTemperatureData()
        )
    );
SaveFinalMapImage(
    biome.biomeData,
    "biome_map.png"
);
Console.WriteLine("Biome map Generated!");
//sw.Stop();
//Console.WriteLine($"Runtime: {sw.ElapsedMilliseconds} ms");



GeneratePlanet1 result = new GeneratePlanet1(
    contData.continentalData,
    erosion.erosionData,
    heightMap.heightData,
    precipitationMap.humidityMap,
    riversMap.riverMap,
    plateIndex,
    temperature.GetTemperatureData(),
    biome.biomeData
);

var options = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    WriteIndented = false
};

string json = JsonSerializer.Serialize(result);

Console.Write(json);

//
// ==================================================================
//                           SAVE METHODS
// ==================================================================
//

void SavePlateIndexImage(int[,] plates, string path)
{
    int width = plates.GetLength(0);
    int height = plates.GetLength(1);

    using Bitmap bmp = new Bitmap(width, height);

    HashSet<int> unique = new HashSet<int>();
    for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            unique.Add(plates[x, y]);

    Random rng = new Random();
    Dictionary<int, Color> palette = new Dictionary<int, Color>();
    foreach (int id in unique)
    {
        palette[id] = Color.FromArgb(
            rng.Next(40, 256),
            rng.Next(40, 256),
            rng.Next(40, 256)
        );
    }

    for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            bmp.SetPixel(x, y, palette[plates[x, y]]);

    bmp.Save(path, ImageFormat.Png);
}

void SaveTemperatureImage(Temperature temperature, string path)
{
    int[,] tempData = temperature.GetTemperatureData();
    int width = tempData.GetLength(0);
    int height = tempData.GetLength(1);

    int minTemp = temperature.lowerTemp;
    int maxTemp = temperature.higherTemp;

    using Bitmap bmp = new Bitmap(width, height);

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            float t = (float)(tempData[x, y] - minTemp) / (maxTemp - minTemp);
            t = Math.Clamp(t, 0f, 1f);

            Color color = LerpColor(Color.Blue, Color.Red, t);
            bmp.SetPixel(x, y, color);
        }
    }

    bmp.Save(path, ImageFormat.Png);
}


void SaveContinentalImage(int[,] data, string path)
{
    int width = data.GetLength(0);
    int height = data.GetLength(1);

    using Bitmap bmp = new Bitmap(width, height);

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            Color c = data[x, y] switch
            {
                0 => Color.DarkBlue,    // oceano profundo
                1 => Color.DarkGreen,   // continente
                2 => Color.LightGreen,  // costa
                3 => Color.Cyan,        // mar raso
                _ => Color.Magenta
            };

            bmp.SetPixel(x, y, c);
        }
    }

    bmp.Save(path, ImageFormat.Png);
}

void SaveHeightImage(float[,] data, string path)
{
    int width = data.GetLength(0);
    int height = data.GetLength(1);

    using Bitmap bmp = new Bitmap(width, height);

    for (int x = 0; x < width; x++)
        for (int y = 0; y < height; y++)
            bmp.SetPixel(x, y, ColorFromHeight(data[x, y]));

    bmp.Save(path, ImageFormat.Png);
}

void SaveRiversImage(int[,] continent, int[,] rivers, string path)
{
    int width = continent.GetLength(0);
    int height = continent.GetLength(1);

    using Bitmap bmp = new Bitmap(width, height);

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            int c = continent[x, y];
            int r = rivers[x, y];

            Color pixel = Color.Black; // fundo vazio (terra)

            if (c == 0) // oceano profundo
                pixel = Color.FromArgb(0, 0, 80);

            else if (c == 3) // mar raso
                pixel = Color.FromArgb(0, 120, 255);

            else if (r == 1) // rio comum
                pixel = Color.Blue;

            else if (r == 2) // rio forte
                pixel = Color.DarkBlue;

            else if (r == 3) // nascente
                pixel = Color.Cyan;

            bmp.SetPixel(x, y, pixel);
        }
    }

    bmp.Save(path, ImageFormat.Png);
}

void SavePrecipitationImage(float[,] data, string path)
{
    int width = data.GetLength(0);
    int height = data.GetLength(1);

    using Bitmap bmp = new Bitmap(width, height);

    for (int x = 0; x < width; x++)
    {
        for (int y = 0; y < height; y++)
        {
            float t = Math.Clamp(data[x, y], 0f, 1f);

            Color color;
            if (t < 0.25f) 
                color = LerpColor(Color.Red, Color.Orange, t / 0.25f);
            else if (t < 0.5f) 
                color = LerpColor(Color.Orange, Color.Yellow, (t - 0.25f) / 0.25f);
            else if (t < 0.75f) 
                color = LerpColor(Color.Yellow, Color.Green, (t - 0.5f) / 0.25f);
            else 
                color = LerpColor(Color.Green, Color.Blue, (t - 0.75f) / 0.25f);

            bmp.SetPixel(x, y, color);
        }
    }

    bmp.Save(path, ImageFormat.Png);
}



void SaveFinalMapImage(string[,] biomeData, string path)
{
    int width = biomeData.GetLength(0);
    int height = biomeData.GetLength(1);

    using (Bitmap bitmap = new Bitmap(width, height))
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                string biome = biomeData[x, y];
                Color color = GetBiomeColor(biome);
                bitmap.SetPixel(x, y, color);
            }
        }

        bitmap.Save(path, ImageFormat.Png);
    }
}

Color GetBiomeColor(string biome)
{
    return biome switch
    {
        "Ocean"        => Color.FromArgb(0, 70, 200),
        "Sea"          => Color.FromArgb(0, 90, 230),
        "Beach"        => Color.FromArgb(230, 220, 170),
        "River"        => Color.FromArgb(30, 120, 255),
        "Lake"         => Color.FromArgb(20, 90, 210),

        "Plains"       => Color.FromArgb(100, 200, 80),
        "Plains_Hills" => Color.FromArgb(90, 170, 70),
        "Mountains"        => Color.FromArgb(100, 100, 60),
        "Hills"    => Color.FromArgb(120, 120, 120),

        "Taiga"        => Color.FromArgb(60, 120, 100),
        "Jungle"       => Color.FromArgb(20, 160, 20),
        "Forest"       => Color.FromArgb(20, 200, 70),

        "Desert"       => Color.FromArgb(230, 200, 80),
        "Desert_Hills" => Color.FromArgb(210, 180, 60),

        "Tundra"       => Color.FromArgb(180, 180, 200),
        "Tundra_Hills" => Color.FromArgb(160, 160, 190),

        _  => Color.White,
    };
}



//
// ==================================================================
//                           COLOR FUNCTIONS
// ==================================================================
//

Color ColorFromHeight(float h)
{
    if (h < 0f)
    {
        float t = Math.Clamp((-h) / 1f, 0f, 1f);

        return LerpColor(
            Color.FromArgb(0, 120, 255),  // azul claro (raso)
            Color.FromArgb(0, 0, 80),     // azul escuro (profundo)
            t
        );
    }

    // 0 – 1   → Planícies
    if (h <= 1f)
        return LerpColor(
            Color.FromArgb(20, 100, 20),
            Color.FromArgb(110, 255, 110),
            h / 1f
        );

    // 1 – 2   → Colinas
    if (h <= 2f)
        return LerpColor(
            Color.FromArgb(110, 255, 110),
            Color.FromArgb(200, 180, 60),
            h - 1f
        );

    // 2 – 3   → Montanhas
    if (h <= 3f)
        return LerpColor(
            Color.FromArgb(200, 180, 60),
            Color.FromArgb(255, 255, 255),
            (h - 2f)
        );

    return Color.White;
}

Color LerpColor(Color a, Color b, float t)
{
    t = Math.Clamp(t, 0f, 1f);
    int r = (int)(a.R + (b.R - a.R) * t);
    int g = (int)(a.G + (b.G - a.G) * t);
    int bC = (int)(a.B + (b.B - a.B) * t);
    return Color.FromArgb(r, g, bC);
}
