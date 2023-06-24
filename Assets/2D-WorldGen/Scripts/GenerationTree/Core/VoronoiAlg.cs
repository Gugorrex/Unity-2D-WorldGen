using System.Collections.Generic;
using _2D_WorldGen.Scripts.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "Voronoi", menuName = "2D World Gen/GenerationTree/Core/Voronoi", order = 0)]
    public class VoronoiAlg : GenerationAlgorithm
    {
        [Header("Voronoi Settings")] 
        public int gridSize;
        public int typeCount;
        public int offsetStrength;
        
        [BurstCompile(CompileSynchronously = true)]
        private struct VoronoiJob : IJobFor
        {
            [ReadOnly] public int2 ChunkCoords;
            [ReadOnly] public int ChunkSize;
            [ReadOnly] public int VoronoiGridSize;
            [ReadOnly] public int Seed;
            [ReadOnly] public int TypeCount;
            [ReadOnly] public int OffsetStrength;

            [ReadOnly] public NativeArray<float> OffsetsX;
            [ReadOnly] public NativeArray<float> OffsetsY;

            public NativeArray<float> VoronoiTypeMap;

            public void Execute(int i)
            {
                var x = i % ChunkSize;
                var y = i / ChunkSize;
                var offset = new int2((int)(OffsetsX[i] * OffsetStrength), (int)(OffsetsY[i] * OffsetStrength));
                var worldCoords = GridCoordsConverter.SubToWorldGridCoords(ChunkSize, ChunkCoords) + new int2(x, y) + offset;
                VoronoiTypeMap[i] = GetType(worldCoords, VoronoiGridSize, Seed, TypeCount) / (float)TypeCount; // TODO check 0..1 float vs 0,1,2... int
            }
            
            private static int2 GetClosestSeed(int2 currentPos, NativeArray<int2> voronoiSeeds)
            {
                var minDist = float.MaxValue;
                var minSeed = currentPos;
                foreach (var seed in voronoiSeeds)
                {
                    var dist = SquareDistance(seed, currentPos);
                    if (dist < minDist)
                    {
                        minDist = dist;
                        minSeed = seed;
                    }
                }
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

            private static int GetType(int2 currentWorldPos, int voronoiGridSize, int worldSeed, int typeCount)
            {
                var minVoronoiWorldCoords = new int2((int)(currentWorldPos.x - 1.42 * voronoiGridSize),
                    (int)(currentWorldPos.y - 1.42 * voronoiGridSize));
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
            
                var voronoiSeeds = seedToTypeDict.GetKeyArray(Allocator.Temp);
                var type = seedToTypeDict[GetClosestSeed(currentWorldPos, voronoiSeeds)];
                voronoiSeeds.Dispose();
                seedToTypeDict.Dispose();
                return type;
            }
        }

        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var job = new VoronoiJob
            {
                ChunkCoords = cycleData.ChunkCoords,
                ChunkSize = cycleData.ChunkSize,
                VoronoiGridSize = gridSize,
                Seed = cycleData.Seed,
                TypeCount = typeCount,
                OffsetStrength = offsetStrength,
                OffsetsX = inputs[0],
                OffsetsY = inputs[1],
                VoronoiTypeMap = output
            };
            return job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var job = new VoronoiJob
            {
                ChunkCoords = cycleData.ChunkCoords,
                ChunkSize = cycleData.ChunkSize,
                VoronoiGridSize = gridSize,
                Seed = cycleData.Seed,
                TypeCount = typeCount,
                OffsetStrength = offsetStrength,
                OffsetsX = inputs[0],
                OffsetsY = inputs[1],
                VoronoiTypeMap = output
            };
            job.Execute(index);
        }
    }
}