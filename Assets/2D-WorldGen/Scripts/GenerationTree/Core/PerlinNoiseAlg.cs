using System;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    [CreateAssetMenu(fileName = "PerlinNoiseAlg", menuName = "GenerationTree/Core/PerlinNoise", order = 0)]
    public class PerlinNoiseAlg : GenerationAlgorithm
    {
        [Header("Perlin Noise Settings")] 
        public OctaveSettings[] octaveSettingsArray;
        public float noiseScale;
        public Vector2Int additionalOffset;

        [Serializable]
        public struct OctaveSettings
        {
            public float amplitude;
            public float frequency;
        }
        
        private struct OctaveConfig
        {
            public float Amplitude;
            public float Frequency;
            public int CellOffsetY; // cell offset (e.g. chunk offset); all scales & factors will be applied to them
            public int CellOffsetX; // since they are projected coordinates INSIDE the perlin noise map
            public int PerlinOffsetX; // offset of the perlin noise itself, usually random
            public int PerlinOffsetY; // used to prevent origin symmetry & to get completely different noise maps for octaves to prevent similarity
        }
        
        [BurstCompile(CompileSynchronously = true)]
        private struct PerlinNoiseJob : IJobFor
        {
            [ReadOnly] public int ChunkSize;
            [ReadOnly] public NativeArray<OctaveConfig> OctaveConfigs;
            [ReadOnly] public float Scale;
            public NativeArray<float> Output;

            public void Execute(int index)
            {
                var x = index % ChunkSize;
                var y = index / ChunkSize;
                
                // 2. Actual noise calculation
            
                float noise = 0;

                // Calculate noise for each octave
                for (var i = 0; i < OctaveConfigs.Length; i++)
                {
                    var cellX = x + OctaveConfigs[i].CellOffsetX;
                    var cellY = y + OctaveConfigs[i].CellOffsetY;
                    var perlinX = ((cellX / Scale) * OctaveConfigs[i].Frequency) + OctaveConfigs[i].PerlinOffsetX;
                    var perlinY = ((cellY / Scale) * OctaveConfigs[i].Frequency) + OctaveConfigs[i].PerlinOffsetY;

                    // add octave perlin noise to final noise
                    noise += Mathf.PerlinNoise(perlinX, perlinY) * OctaveConfigs[i].Amplitude;
                }

                Output[y * ChunkSize + x] = Mathf.Clamp01(noise);
            }
        }
        
        private PerlinNoiseJob CreateJob(int seed, int2 chunkCoords, int chunkSize, NativeArray<float> output,
            OctaveSettings[] octaveSettings, out NativeArray<OctaveConfig> octaveConfigs)
        {
            // 1. Variables Setup

            if (octaveSettings.Length < 1) Debug.LogWarning("Noise Calculation: No octave offsets! Need at " +
                                                            "least one! Maybe not set in ProceduralTilemap (Inspector)?");
            var scale = noiseScale;
            if (scale <= 0f) scale = 0.0001f;
            
            // pre-calculate offset (random offset must be the same for each octave!)
            octaveConfigs = new NativeArray<OctaveConfig>(octaveSettings.Length, Allocator.TempJob);
            var random = new System.Random(seed);
            for (var i = 0; i < octaveSettings.Length; i++)
            {
                octaveConfigs[i] = new OctaveConfig
                {
                    Amplitude = octaveSettings[i].amplitude,
                    Frequency = octaveSettings[i].frequency,
                    CellOffsetX = chunkCoords.x * chunkSize + additionalOffset.x,
                    CellOffsetY = chunkCoords.y * chunkSize + additionalOffset.y,
                    PerlinOffsetX = random.Next(-100000, 100000),
                    PerlinOffsetY = random.Next(-100000, 100000)
                };
            }
            
            // Set job parameters for further steps
            var job = new PerlinNoiseJob
            {
                ChunkSize = chunkSize,
                OctaveConfigs = octaveConfigs,
                Scale = scale,
                Output = output
            };

            return job;
        }
        
        protected override JobHandle Schedule(GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output, JobHandle dependsOn = default)
        {
            var job = CreateJob(cycleData.Seed, cycleData.ChunkCoords, cycleData.ChunkSize,
                output, octaveSettingsArray, out var octaveConfigs);
            var handle = job.ScheduleParallel(cycleData.ArrayLength, batchSize, dependsOn);

            octaveConfigs.Dispose(handle);
            return handle;
        }

        protected override void Execute(int index, GenerationCycleData cycleData, List<NativeArray<float>> inputs, NativeArray<float> output)
        {
            var job = CreateJob(cycleData.Seed, cycleData.ChunkCoords, cycleData.ChunkSize,
                output, octaveSettingsArray, out var octaveConfigs);
            job.Execute(index);
            octaveConfigs.Dispose();
        }
    }
}