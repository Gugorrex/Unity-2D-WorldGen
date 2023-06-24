# Unity-2D-WorldGen
a procedural, tile-based 2D world generation in Unity Engine. Including additional features such as a* pathfinding

## Demo

![](Assets/Docs/Resources/Demo/RiverIsTeBiSubtraction.png)

### Generation Tree
generation tree view created by generation tree window:

![](Assets/Docs/Resources/Demo/example-generation-tree.png)

- RiverIsTeBiSubtraction
  - IslandTerrainBiSubtraction
    - PerlinNoise
    - IslandCurveGraph
      - IslandPerlinNoise
  - RiverBiCurveGraph
    - RiverPerlinNoise

The generation tree shows the mathematical dependencies,
e.g. to create a ``RiverBiCurveGraph``, we first need to calculate ``RiverPerlinNoise``

### Island Perlin Noise

![](Assets/Docs/Resources/Demo/IslandPerlinNoise.png)

### Island Curve Graph

![](Assets/Docs/Resources/Demo/IslandCurveGraph.png)

### Perlin Noise

![](Assets/Docs/Resources/Demo/PerlinNoise.png)

### IslandTerrainBiSubtraction

![](Assets/Docs/Resources/Demo/IslandTerrainBiSubtraction.png)

### RiverPerlinNoise

![](Assets/Docs/Resources/Demo/RiverPerlinNoise.png)

### RiverBiCurveGraph
also known as perlin worms

![](Assets/Docs/Resources/Demo/RiverBiCurveGraph.png)

### RiverIsTeBiSubtraction

![](Assets/Docs/Resources/Demo/RiverIsTeBiSubtraction.png)

<hr>

### Voronoi Biomes

![](Assets/Docs/Resources/Demo/biomes.png)

### Basic Desert Biome

![](Assets/Docs/Resources/Demo/desert biome.png)