using System;
using System.Collections.Generic;
using System.Linq;
using _2D_WorldGen.Scripts.Config;
using _2D_WorldGen.Scripts.GenerationTree;
using _2D_WorldGen.Scripts.Manager.Addons;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Manager.Core
{
    [RequireComponent(typeof(TilemapManager))]
    [RequireComponent(typeof(ChunkLoaderManager))]
    public class WorldGenerator : MonoBehaviour
    {
        public bool debugMode = false;
        [SerializeField] private int seed;
        [SerializeField] private int chunkSize;
        [SerializeField] private int batchSize = 32;
        [SerializeField] private NodeConfigMatch[] generationSchedule;

        private TilemapManager _tilemapManager;
        private ChunkLoaderManager _chunkLoaderManager;

        public int ChunkSize => chunkSize;
        
        // Addons
        private PathfindingManager _pathfindingManager;

        [Serializable]
        public struct NodeConfigMatch
        {
            public bool active;
            public GenerationAlgorithm generationAlgorithm;
            public GeneratorConfig generatorConfig;
        }

        private void Awake()
        {
            _tilemapManager = GetComponent<TilemapManager>();
            _chunkLoaderManager = GetComponent<ChunkLoaderManager>();
            
            // Addons
            if (TryGetComponent<PathfindingManager>(out var pathfindingManager))
            {
                _pathfindingManager = pathfindingManager;
            }
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
            var biomes = new NativeArray<int>(cycleData.ArrayLength, Allocator.Persistent);

            foreach (var config in generationSchedule)
            {
                if (!config.active) continue;

                if (debugMode)
                {
                    for (var i = 0; i < cycleData.ChunkSize * cycleData.ChunkSize; i++)
                    {
                        config.generationAlgorithm.ExecuteOne(i, cycleData);
                    }
                }
                else
                {
                    var handle = config.generationAlgorithm.ScheduleAll(cycleData);
                    handle.Complete();
                }
                

                switch (config.generatorConfig.action)
                {
                    case GeneratorConfig.GeneratorAction.ApplyChunk:
                        ApplyChunk(cycleData, config, biomes);
                        break;
                    
                    case GeneratorConfig.GeneratorAction.ApplyBiomes:
                        ApplyBiomes(biomes, config);
                        break;
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            biomes.Dispose();
        }

        private void ApplyChunk(GenerationCycleData cycleData, NodeConfigMatch config, NativeArray<int> biomes)
        {
            var obstacles = new NativeHashSet<int2>(chunkSize * chunkSize, Allocator.TempJob);
            var sparseTileCostsMap = new NativeHashMap<int2, float>(chunkSize * chunkSize, Allocator.TempJob);
            var job = new NoiseToTilesJob
            {
                Noise = config.generationAlgorithm.GetResults(),
                Chunk = new NativeArray<int>(cycleData.ArrayLength * cycleData.TilemapCount,
                    Allocator.TempJob),
                TileSettingsArray = GetTileSettings(config.generatorConfig.HeightConfigs,
                    out var indices),
                Indices = indices,
                Biomes = biomes, // default = 0 // TODO implement biomes or general ID based heightConfig selection
                ChunkSize = chunkSize,
                
                // Pathfinding
                TileObstacleMap = _tilemapManager.TilemapConfig.ObstacleMap,
                TileTileCostsMap = _tilemapManager.TilemapConfig.TileCostsMap,
                Obstacles = obstacles.AsParallelWriter(),
                SparseTileCostsMap = sparseTileCostsMap.AsParallelWriter(),
                ChunkCoords = cycleData.ChunkCoords
            };
            
            var jobHandle = job.ScheduleParallel(cycleData.ArrayLength, batchSize, default);
            job.TileSettingsArray.Dispose(jobHandle);
            job.Indices.Dispose(jobHandle);
            jobHandle.Complete();
            _tilemapManager.RenderChunk(cycleData.ChunkCoords, job.Chunk);
            _tilemapManager.RefreshChunk(cycleData.ChunkCoords);
            job.Chunk.Dispose();

            if (_pathfindingManager != null)
            {
                _pathfindingManager.AddChunk(obstacles, sparseTileCostsMap);
                obstacles.Dispose();
                sparseTileCostsMap.Dispose();
            }
        }

        private void ApplyBiomes(NativeArray<int> biomes, NodeConfigMatch config)
        {
            var rawBiomes = config.generationAlgorithm.GetResults();
            var biomesCount = config.generatorConfig.biomesCount;
            
            for (var i = 0; i < rawBiomes.Length; i++)
            {
                biomes[i] = (int)(rawBiomes[i] * biomesCount);
            }
        }

        private NativeArray<TileSettings> GetTileSettings(Dictionary<int,HeightConfig> heightConfigMap, out NativeHashMap<int,int2> indices)
        {
            var tilemapConfig = _tilemapManager.TilemapConfig;
            var length = heightConfigMap.Values.Sum(heightConfig => heightConfig.tileHeights.Length);
            var tileSettings = new NativeArray<TileSettings>(length, Allocator.TempJob);
            indices = new NativeHashMap<int,int2>(heightConfigMap.Count, Allocator.TempJob);

            var i = 0;
            foreach (var (key, heightConfig) in heightConfigMap)
            {
                indices.Add(key, new int2(i, i + heightConfig.tileHeights.Length - 1));
                for (var j = 0; j < heightConfig.tileHeights.Length; j++)
                {
                    var settings = new TileSettings
                    {
                        ID = tilemapConfig.GetTileID(heightConfig.tileHeights[j].tileStringID),
                        Height = heightConfig.tileHeights[j].height,
                    };
                    settings.TilemapID = tilemapConfig.GetTileTilemapID(settings.ID);
                    tileSettings[i++] = settings;
                }
            }

            return tileSettings;
        }

        private struct TileSettings
        {
            public int ID;
            public float Height;
            public int TilemapID;
        }

        [BurstCompile(CompileSynchronously = true)]
        private struct NoiseToTilesJob : IJobFor
        {
            [ReadOnly] public NativeArray<float> Noise;
            [ReadOnly] public NativeArray<TileSettings> TileSettingsArray;
            [ReadOnly] public NativeHashMap<int, int2> Indices;
            [ReadOnly] public NativeArray<int> Biomes;
            [ReadOnly] public int ChunkSize;
            
            // 3d Chunk (2x ChunkSize, 1x TilemapID) leads to ParallelFor IndexOutOfRange Error due to Safety System
            // with correct tilemapID and ChunkSize this should not lead to a race condition
            // -> NativeDisableParallelForRestriction attribute for Chunk array
            [NativeDisableParallelForRestriction] public NativeArray<int> Chunk;
            
            // Pathfinding
            [ReadOnly] public int2 ChunkCoords;
            [ReadOnly] public NativeHashSet<int> TileObstacleMap;
            [ReadOnly] public NativeHashMap<int, float> TileTileCostsMap;
            public NativeHashSet<int2>.ParallelWriter Obstacles;
            public NativeHashMap<int2, float>.ParallelWriter SparseTileCostsMap;

            public void Execute(int i)
            {
                var x = i % ChunkSize;
                var y = i / ChunkSize;
                
                var index = Indices[Biomes[i]];
                
                // Loop over configured tile settings
                for (var j = index.x; j <= index.y; j++)
                {
                    // If the height is smaller or equal then use this tileSetting (j)
                    if (!(Noise[i] <= TileSettingsArray[j].Height)) continue;

                    if (TileSettingsArray[j].ID != 0) // ignore intended null / empty tiles (empty stringID / 0)
                    {
                        Chunk[TileSettingsArray[j].TilemapID * ChunkSize * ChunkSize + i] = TileSettingsArray[j].ID;
                        
                        // Pathfinding
                        if (TileObstacleMap.Contains(TileSettingsArray[j].ID))
                        {
                            Obstacles.Add(new int2(ChunkCoords.x * ChunkSize + x, ChunkCoords.y * ChunkSize + y));
                        } 
                        else if (TileTileCostsMap.ContainsKey(TileSettingsArray[j].ID))
                        {
                            SparseTileCostsMap.TryAdd(
                                new int2(ChunkCoords.x * ChunkSize + x, ChunkCoords.y * ChunkSize + y),
                                TileTileCostsMap[TileSettingsArray[j].ID]);
                        }
                    }

                    break;
                }
            }
        }
    }
}