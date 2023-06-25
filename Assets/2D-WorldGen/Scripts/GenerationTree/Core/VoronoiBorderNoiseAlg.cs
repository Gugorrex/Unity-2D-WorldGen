using System.Collections.Generic;
using _2D_WorldGen.Scripts.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "VoronoiBorderNoise", menuName = "2D World Gen/GenerationTree/Core/VoronoiBorderNoise", order = 0)]
    public class VoronoiBorderNoiseAlg : GenerationAlgorithm
    {
        [Header("Voronoi Settings")] 
        public int gridSize;
        public int typeCount;
        public float borderZone;
        public float offsetStrength;
        
        [BurstCompile(CompileSynchronously = true)]
        private struct VoronoiBorderNoiseJob : IJobFor
        {
            [ReadOnly] public int2 ChunkCoords;
            [ReadOnly] public int ChunkSize;
            [ReadOnly] public int VoronoiGridSize;
            [ReadOnly] public int Seed;
            [ReadOnly] public int TypeCount;
            [ReadOnly] public float BorderZone;
            [ReadOnly] public float OffsetStrength;
            
            [ReadOnly] public NativeArray<float> OffsetsX;
            [ReadOnly] public NativeArray<float> OffsetsY;
            
            public NativeArray<float> VoronoiBorderNoise;

            public void Execute(int i)
            {
                var x = i % ChunkSize;
                var y = i / ChunkSize;
                var offset = new int2((int)(OffsetsX[i] * OffsetStrength), (int)(OffsetsY[i] * OffsetStrength));
                var worldCoords = GridCoordsConverter.SubToWorldGridCoords(ChunkSize, ChunkCoords) + new int2(x, y) + offset;
                var borderProximity = Voronoi.GetBorderProximity(worldCoords, VoronoiGridSize, Seed, TypeCount);
                var relativeBorderProximity = borderProximity / BorderZone;
                VoronoiBorderNoise[i] =
                    math.clamp(relativeBorderProximity, 0, 1);
            }
        }

        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var job = new VoronoiBorderNoiseJob
            {
                ChunkCoords = cycleData.ChunkCoords,
                ChunkSize = cycleData.ChunkSize,
                VoronoiGridSize = gridSize,
                Seed = cycleData.Seed,
                TypeCount = typeCount,
                VoronoiBorderNoise = output,
                BorderZone = borderZone,
                OffsetStrength = offsetStrength,
                OffsetsX = inputs[0],
                OffsetsY = inputs[1]
            };
            return job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var job = new VoronoiBorderNoiseJob
            {
                ChunkCoords = cycleData.ChunkCoords,
                ChunkSize = cycleData.ChunkSize,
                VoronoiGridSize = gridSize,
                Seed = cycleData.Seed,
                TypeCount = typeCount,
                VoronoiBorderNoise = output,
                BorderZone = borderZone,
                OffsetStrength = offsetStrength,
                OffsetsX = inputs[0],
                OffsetsY = inputs[1]
            };
            job.Execute(index);
        }
    }
}