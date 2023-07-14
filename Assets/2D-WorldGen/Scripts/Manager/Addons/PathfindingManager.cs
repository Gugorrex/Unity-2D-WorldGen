using System;
using _2D_WorldGen.Scripts.Config;
using _2D_WorldGen.Scripts.Manager.Core;
using _2D_WorldGen.Scripts.Pathfinding;
using _2D_WorldGen.Scripts.Utils;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Manager.Addons
{
    [RequireComponent(typeof(WorldGenerator))]
    public class PathfindingManager : MonoBehaviour
    {
        public int obstacleLimit;
        public int tileCostLimit;

        private NativeHashSet<int2> _obstacles;
        private NativeHashMap<int2, float> _sparseTileCostsMap;
        private TilemapConfig _tilemapConfig;
        private int _chunkSize;

        private void Awake()
        {
            _obstacles = new NativeHashSet<int2>(obstacleLimit, Allocator.Persistent);
            _sparseTileCostsMap = new NativeHashMap<int2, float>(tileCostLimit, Allocator.Persistent);
            _tilemapConfig = GetComponent<TilemapManager>().TilemapConfig;
            _chunkSize = GetComponent<WorldGenerator>().ChunkSize;
        }

        public void AddChunk(NativeHashSet<int2> chunkObstacles, NativeHashMap<int2, float> chunkSparseTileCostsMap)
        {
            foreach (var coords in chunkObstacles)
            {
                _obstacles.Add(coords);
            }

            foreach (var entry in chunkSparseTileCostsMap)
            {
                _sparseTileCostsMap.TryAdd(entry.Key, entry.Value);
            }
        }

        public bool AddObstacle(int2 worldCoordsObstacle)
        {
            return _obstacles.Add(worldCoordsObstacle);
        }

        public bool TryAddTileCost(int2 worldCoords, float cost)
        {
            return _sparseTileCostsMap.TryAdd(worldCoords, cost);
        }

        public void RemoveChunk(int2 chunkCoords)
        {
            var worldCoordsRoot = GridCoordsConverter.SubToWorldGridCoords(_chunkSize, chunkCoords);
            
            for (var y = 0; y < _chunkSize; y++)
            {
                for (var x = 0; x < _chunkSize; x++)
                {
                    var coords = worldCoordsRoot + new int2(x, y);
                    RemoveCoords(coords);
                }
            }
        }

        private void AddCoord(int2 worldCoords, int tileID)
        {
            var tilemapID = _tilemapConfig.GetTileTilemapID(tileID);
            if (_tilemapConfig.IsObstacle(tilemapID) && tileID != 0)
            {
                AddObstacle(worldCoords);
            }
            else
            {
                var tileCost = _tilemapConfig.GetTileCost(tileID);
                if (Math.Abs(tileCost - 1) > 0.01f)
                {
                    _sparseTileCostsMap.TryAdd(worldCoords, tileCost);
                }
            }
        }
        
        public void RemoveCoords(int2 worldCoords)
        {
            _obstacles.Remove(worldCoords);
            _sparseTileCostsMap.Remove(worldCoords);
        }

        public void Dispose()
        {
            _obstacles.Dispose();
            _sparseTileCostsMap.Dispose();
        }

        public AStarJob CreateAStarJob(int2 startCoords, int2 targetCoords, int safeguard)
        {
            var job = new AStarJob
            {
                StartNode = new AStarJob.Node
                {
                    Coords = startCoords,
                    Parent = int2.zero,
                    GScore = int.MaxValue,
                    HScore = int.MaxValue
                },
                TargetNode = new AStarJob.Node
                {
                    Coords = targetCoords,
                    Parent = int2.zero,
                    GScore = int.MaxValue,
                    HScore = int.MaxValue
                },
                Safeguard = safeguard,
                Obstacles = _obstacles,
                SparseCostsMap = _sparseTileCostsMap
            };
            job.Init();
            return job;
        }

        private void OnApplicationQuit()
        {
            Dispose();
        }
    }
}