using System;
using _2D_WorldGen.Scripts.Config;
using _2D_WorldGen.Scripts.GenerationTree;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Manager.Core
{
    [RequireComponent(typeof(TilemapManager))]
    [RequireComponent(typeof(ChunkLoaderManager))]
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private int seed;
        [SerializeField] private int chunkSize;
        [SerializeField] private int batchSize = 32;
        [SerializeField] private NodeConfigMatch[] generationSchedule;

        private TilemapManager _tilemapManager;
        private ChunkLoaderManager _chunkLoaderManager;

        public int ChunkSize => chunkSize;

        [Serializable]
        public struct NodeConfigMatch
        {
            public GenerationAlgorithm generationAlgorithm;
            public GeneratorConfig generatorConfig;
        }

        private void Awake()
        {
            _tilemapManager = GetComponent<TilemapManager>();
            _chunkLoaderManager = GetComponent<ChunkLoaderManager>();
        }
        
        private void Update()
        {
            if (_chunkLoaderManager.UpdateLoadingRange(out var chunkToGenerate))
            {
                GenerateChunk(chunkToGenerate);
            }
        }
        
        private void GenerateChunk(int2 chunkCoords)
        {
            var cycleData = new GenerationCycleData(seed, chunkCoords, chunkSize, 
                _tilemapManager.TilemapConfig.ReadonlyTilemapIdMapping.Count);
            
            foreach (var config in generationSchedule)
            {
                var handle = config.generationAlgorithm.ScheduleAll(cycleData);
                handle.Complete();

                switch (config.generatorConfig.action)
                {
                    case GeneratorConfig.GeneratorAction.ApplyChunk:
                        ApplyChunk(cycleData, config);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void ApplyChunk(GenerationCycleData cycleData, NodeConfigMatch config)
        {
            var job = new NoiseToTilesJob()
            {
                Noise = config.generationAlgorithm.GetResults(),
                Chunk = new NativeArray<int>(cycleData.ArrayLength * cycleData.TilemapCount, Allocator.TempJob),
                TileSettingsArray = GetTileSettings(config.generatorConfig.HeightConfigs[0]), // TODO implement biomes or general ID based heightConfig selection
                ChunkSize = chunkSize
            };
            var jobHandle = job.ScheduleParallel(cycleData.ArrayLength, batchSize, default);
            job.TileSettingsArray.Dispose(jobHandle);
            jobHandle.Complete();
            _tilemapManager.RenderChunk(cycleData.ChunkCoords, job.Chunk);
            _tilemapManager.RefreshChunk(cycleData.ChunkCoords);
            job.Chunk.Dispose();
        }

        private NativeArray<TileSettings> GetTileSettings(HeightConfig heightConfig)
        {
            var tilemapConfig = _tilemapManager.TilemapConfig;
            var tileSettings = new NativeArray<TileSettings>(heightConfig.tileHeights.Length, Allocator.TempJob);

            for (var i = 0; i < heightConfig.tileHeights.Length; i++)
            {
                var settings = new TileSettings
                {
                    ID = tilemapConfig.GetTileID(heightConfig.tileHeights[i].tileStringID),
                    Height = heightConfig.tileHeights[i].height
                };
                settings.TilemapID = tilemapConfig.GetTileTilemapID(settings.ID);
                tileSettings[i] = settings;
            }

            return tileSettings;
        }

        private struct TileSettings
        {
            public int ID;
            public float Height;
            public int TilemapID;
        }

        private struct NoiseToTilesJob : IJobFor
        {
            [ReadOnly] public NativeArray<float> Noise;
            [ReadOnly] public NativeArray<TileSettings> TileSettingsArray;
            [ReadOnly] public int ChunkSize;
            
            // 3d Chunk (2x ChunkSize, 1x TilemapID) leads to ParallelFor IndexOutOfRange Error due to Safety System
            // with correct tilemapID and ChunkSize this should not lead to a race condition
            // -> NativeDisableParallelForRestriction attribute for Chunk array
            [NativeDisableParallelForRestriction] public NativeArray<int> Chunk;

            public void Execute(int i)
            {
                // Loop over configured tile settings
                for (var j = 0; j < TileSettingsArray.Length; j++)
                {
                    // If the height is smaller or equal then use this tileSetting (j)
                    if (!(Noise[i] <= TileSettingsArray[j].Height)) continue;

                    if (TileSettingsArray[j].ID != 0) // ignore intended null / empty tiles (empty stringID / 0)
                    {
                        Chunk[TileSettingsArray[j].TilemapID * ChunkSize * ChunkSize + i] = TileSettingsArray[j].ID;
                    }

                    break;
                }
            }
        }
    }
}