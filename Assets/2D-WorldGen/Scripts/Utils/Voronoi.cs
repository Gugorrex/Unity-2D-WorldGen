using Unity.Collections;
using Unity.Mathematics;

namespace _2D_WorldGen.Scripts.Utils
{
    public struct Voronoi
    {
        private static int2 GetClosestSeed(int2 currentPos, NativeArray<int2> voronoiSeeds)
        {
            return GetClosestSeed(currentPos, voronoiSeeds, out _);
        }
        
        private static int2 GetClosestSeed(int2 currentPos, NativeArray<int2> voronoiSeeds, out int2 secondClosestSeed)
        {
            var minDist = float.MaxValue;
            var minSeed = currentPos;
            var secMinDist = float.MaxValue;
            var secMinSeed = currentPos;
            
            foreach (var seed in voronoiSeeds)
            {
                var dist = SquareDistance(seed, currentPos);
                if (dist < minDist)
                {
                    secMinDist = minDist;
                    secMinSeed = minSeed;
                    minDist = dist;
                    minSeed = seed;
                } 
                else if (dist < secMinDist)
                {
                    secMinDist = dist;
                    secMinSeed = seed;
                }
            }

            secondClosestSeed = secMinSeed;
            return minSeed;
        }
        
        private static float SquareDistance(int2 a, int2 b)
        {
            var distX = math.abs(a.x - b.x);
            var distY = math.abs(a.y - b.y);
            return distX * distX + distY * distY;
        }
        
        /// <returns> seed in world coords </returns>
        private static int2 GetSeed(int2 voronoiCoords, int voronoiGridSize, int worldSeed, int typeCount, out int typeID)
        {
            var rand = new Random((uint)(worldSeed + voronoiCoords.GetHashCode()));
            typeID = rand.NextInt(typeCount);
            return rand.NextInt2(new int2(voronoiCoords.x * voronoiGridSize, voronoiCoords.y * voronoiGridSize),
                new int2((voronoiCoords.x + 1) * voronoiGridSize, (voronoiCoords.y + 1) * voronoiGridSize));
        }

        private static NativeHashMap<int2, int> GetNeighbourSeeds(int2 worldCoords, int voronoiGridSize, int worldSeed, int typeCount)
        {
            var minVoronoiWorldCoords = new int2((int)(worldCoords.x - 1.42 * voronoiGridSize),
                (int)(worldCoords.y - 1.42 * voronoiGridSize));
            var gridLoopStart = GridCoordsConverter.WorldToSubGridCoords(voronoiGridSize, minVoronoiWorldCoords, out _);
            var seedToTypeDict = new NativeHashMap<int2, int>(16, Allocator.Temp);
        
            for (var x = gridLoopStart.x; x < gridLoopStart.x + 4; x++)
            {
                for (var y = gridLoopStart.y; y < gridLoopStart.y + 4; y++)
                {
                    var voronoiCoords = new int2(x, y);
                    var seed = GetSeed(voronoiCoords, voronoiGridSize, worldSeed, typeCount, out var typeID);
                    seedToTypeDict.Add(seed, typeID);
                }
            }

            return seedToTypeDict;
        }

        public static int GetType(int2 worldCoords, int voronoiGridSize, int worldSeed, int typeCount)
        {
            var seedToTypeDict = GetNeighbourSeeds(worldCoords, voronoiGridSize, worldSeed, typeCount);
            var voronoiSeeds = seedToTypeDict.GetKeyArray(Allocator.Temp);
            var type = seedToTypeDict[GetClosestSeed(worldCoords, voronoiSeeds)];
            voronoiSeeds.Dispose();
            seedToTypeDict.Dispose();
            return type;
        }

        public static float GetBorderProximity(int2 worldCoords, int voronoiGridSize, int worldSeed, int typeCount)
        {
            var seedToTypeDict = GetNeighbourSeeds(worldCoords, voronoiGridSize, worldSeed, typeCount);
            var voronoiSeeds = seedToTypeDict.GetKeyArray(Allocator.Temp);
            var closestSeed = GetClosestSeed(worldCoords, voronoiSeeds, out var secondClosestSeed);
            voronoiSeeds.Dispose();
            seedToTypeDict.Dispose();
            
            // TODO evaluate performance impact
            // sqrt may be heavy in performance, however, in testing, without it the borders will vary too much from thin to thick
            return math.abs(math.sqrt(SquareDistance(closestSeed, worldCoords)) - math.sqrt(SquareDistance(secondClosestSeed, worldCoords)));
        }
    }
}