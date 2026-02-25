# CubicCube-Map-Generator
Large-Scale Earth-Like World Generator for Voxel 3D Games.

🌎 **Languages:** [English](README.md) | [Português](README.pt-BR.md)

The Cubic³ Map Generator is a procedural world generation system developed specifically for the game Cubic³, currently in development by Momentum.

Its purpose is to serve as the foundational world generation layer of the game, enabling the creation of massive, navigable, chunk-based maps, with earth-like continental structures, suitable for a voxel 3D game.

## Project Goal
This project was created to solve a core problem in the development of Cubic³:

How to generate an extremely large, continuous, and navigable voxel world without relying exclusively on noise-based techniques or Wave Function Collapse, while preserving global geographic coherence?

The generator operates at a global scale, producing a deterministic base world map that is later consumed by the voxel chunk generation system.

## Generation Pipeline

The generator operates through a configurable central pipeline, responsible for executing all stages of world generation.

### Pipeline Output Options

The main pipeline provides two mutually exclusive options, configurable at the entry point (Program.cs):

1. Image Export (Visualization)
* Generates images of the final map (and intermediate maps).
Used for:
* Visual debugging.
* Parameter tuning.
* Validation of continents, terrain, and climate.
* Uses direct data-to-pixel conversion.

Ideal during development and generator balancing.

2. Structural Export (For UnityEngine)
* Exports all generated maps as data structures.
* Designed for direct import into the Unity Engine.
* Does not rely on image conversion.
* Preserves full data precision.
* These data are consumed directly by the game's voxel chunk generation system.
* This is the production mode, used by Cubic³.

## Why Not Use Only Noise or WFC?

### Noise-Based Techniques (Perlin / Simplex / FBM)

* Excellent for local variation

Major drawbacks at global scale:
* Lack of well-defined continents
* Excessively chaotic terrain
* Poor long-distance navigability

### Wave Function Collapse

* High computational cost
* Does not scale well for large continuous worlds
* Limited control over macro-scale geography

Cubic³ required a macro-structure-first approach, defining continents, terrain, and climate before voxel-level generation.

## Technical Approach

### Planar Projection

Planar projection was chosen to facilitate the use of the map in chunk generation, also serving as initial data for later minimization.
Generating the map directly with cubic or spherical distortion was considered, but planar projection was chosen because it allows greater creative freedom for designers. Additionally, distortion can be easily applied later within the Unity Engine if necessary.

World modeling is performed using data lists containing:
* Altitude.
* Continental data.
* Temperature.

This approach:
* Facilitates spatial indexing.
* Scales well for large maps.
* Serves as a natural base for voxel chunking.

## Earth-Like Continents

### Cohesive Continental Masses
* Continents generated as continuous blobs.

Explicit control over:
* Scale.
* Distribution.
* Density.

### Simplified Tectonics

* Macro-scale plate simulation.

Natural creation of:
* Mountain ranges.
* Plains.
* Continental boundaries.

The goal is not visual randomness, but navigability and geographic identity.

## Performance and Optimization

The Cubic³ Map Generator was designed from the start with performance and scalability in mind, considering the direct impact world generation has on the pipeline of a voxel 3D game.

### Parallelization and Multithreading

Several stages of generation were parallelized using multithreading, allowing:
* Simultaneous execution of independent tasks
* Better utilization of multi-core CPUs
* Significant reduction in total generation time

Among the parallelized stages are:
* Height map processing
* Temperature calculation
* Erosion application

This approach ensures that the system scales well as map size increases.

### Performance Results

The generator was tested on multiple machines with different hardware configurations, presenting consistent generation times:

* Minimum time: ~8 seconds
* Maximum time: ~17 seconds

These values consider the complete generation of the global map, including continents, terrain, erosion, and climate data.

The achieved performance is suitable both for offline preprocessing and for integrated use within the game development pipeline.

## Generated Map Examples

Below are examples of maps produced by the Cubic³ Map Generator, used both for visual validation and for debugging and parameter tuning during development.

### Continental Map

![Mapa Continental](images/continental_map.png)

### Height Map

![Mapa de altitude](images/height_map.png)

### Tectonic Plates Map

![Mapa de Tectônicas](images/plates_map.png)

### Precipitation Map

![Mapa de Precipitação](images/precipitation_map.png)

### Rivers and Water Bodies Map

![Mapa de Rios](images/rivers_map.png)

### Temperature Map

![Mapa de Temperatura](images/temperature_map.png)

### Biome Map (Final)

![Mapa Final](images/biome_map.png)

Images are automatically generated by the system itself through the visual export mode of the pipeline.

## Macro → Micro (World → Chunks)

O sistema é dividido em duas camadas bem definidas:

### Camada Global (Realizada nessa aplicação)
* Define continentes.
* Altitude média.
* Temperatura.
* Clima base.

### Camada Local
* Chunks voxel consultam o mapa global.
* Geração determinística por seed.
* Continuidade garantida entre regiões.

Essa separação permite:
* Geração sob demanda.
* Baixo custo de memória.

## Erosão e Suavização
Após a geração inicial:
* Algoritmos de erosão reduzem artefatos.
* Criam transições naturais.
* Melhoram jogabilidade voxel.

## Sistema de Temperatura
A temperatura é calculada com base em:
* Latitude.
* Altitude.

Esses dados influenciam:
* Biomas futuros.
* Variação climática.
* Geração voxel contextual.

## Estrutura do Projeto
```text
Cubic³MapGenerator/
├── Program.cs                # Pipeline principal de geração
├── Seed.cs                   # Processamento de Seed para utilização na geração
│
├── Maps/
│   ├── Tectonics.cs          # Geração tectônica
│   ├── HeightMap.cs          # Altitude global
│   ├── Erosion.cs            # Suavização e erosão
│   ├── Temperature.cs        # Mapa de temperatura
│   ├── ContinentalData.cs    # Dados de massas continentais
│   ├── BiomeMap.cs           # Geração do mapa final de biomas
│   ├── Precipitation.cs      # Mapa de precipitação
│   └── Rivers.cs             # Geração de rios e corpos de água
│
├── ContinentBlob.cs          # Criação dos dados dos Blobs de continentes
├── DataImageConverter.cs     # Conversão de imagens em dados
└── Cubic³MapGenerator.csproj
```

## Tecnologias e Técnicas

* C# / .NET
* Geração Procedural
* Modelagem Geográfica
* Paralelização e Multithreading
* Simulação tectônica através de Voronoi
* Separação Macro vs Micro Geração
* Determinismo por seed

## Papel no Desenvolvimento do Cubic³
Este gerador:
* Define a geografia global do jogo
* Alimenta diretamente a Unity Engine
* Garante coerência entre chunks voxel
* Serve como base para sistemas futuros

Ele é parte essencial da engine de world generation do Cubic³.

## Nota Final
Este projeto foi desenvolvido sob medida para um jogo real em produção, priorizando escala, controle técnico e integração direta com a engine.
