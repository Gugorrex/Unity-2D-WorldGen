﻿using System.Collections.Generic;
using _2D_WorldGen.Scripts.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

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
                VoronoiTypeMap[i] = Voronoi.GetType(worldCoords, VoronoiGridSize, Seed, TypeCount) / (float)TypeCount;
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