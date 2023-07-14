using System;
using System.Collections.Generic;
using System.Linq;
using _2D_WorldGen.Scripts.Config;
using _2D_WorldGen.Scripts.Manager.Addons;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _2D_WorldGen.Scripts.Manager.Core
{
    [RequireComponent(typeof(WorldGenerator))]
    public class TilemapManager : MonoBehaviour
    {
        [SerializeField] private TilemapConfig tilemapConfig;
        [SerializeField] private TaggedTilemap[] taggedTilemaps;

        private WorldGenerator _worldGenerator;

        [Serializable]
        public struct TaggedTilemap
        {
            public string stringID;
            public Tilemap tilemap;
        }

        private Dictionary<int, Tilemap> Tilemaps => taggedTilemaps.ToDictionary(
            tilemap => tilemapConfig.GetTilemapID(tilemap.stringID), 
            tilemap => tilemap.tilemap);

        public TilemapConfig TilemapConfig => tilemapConfig;
        
        private TileOverridesManager _tileOverridesManager;

        private void Awake()
        {
            _worldGenerator = GetComponent<WorldGenerator>();
            tilemapConfig.CreateDictionaries();
            
            if (TryGetComponent<TileOverridesManager>(out var tileOverridesManager))
            {
                _tileOverridesManager = tileOverridesManager;
            }
        }

        public void RenderChunk(int2 chunkCoords, NativeArray<int> chunk)
        {
            var chunkSize = _worldGenerator.ChunkSize;
            var positionsArray = new Vector3Int[chunkSize * chunkSize];
            var tilesArray = new TileBase[chunkSize * chunkSize];

            for (var x = 0; x < chunkSize; x++)
            {
                for (var y = 0; y < chunkSize; y++)
                {
                    positionsArray[y * chunkSize + x] = new Vector3Int((chunkCoords.x * chunkSize) + x, 
                        (chunkCoords.y * chunkSize) + y, 0);
                }
            }

            var zOffset = chunkSize * chunkSize;
            foreach (var tilemap in Tilemaps)
            {
                for (var x = 0; x < chunkSize; x++)
                {
                    for (var y = 0; y < chunkSize; y++)
                    {
                        var tileID = chunk[tilemap.Key * zOffset + y * chunkSize + x];
                        var prevTile = tilemap.Value.GetTile(new Vector3Int(chunkCoords.x * chunkSize + x, 
                            chunkCoords.y * chunkSize + y));
                        var prevID = prevTile != null ? tilemapConfig.GetTileID(prevTile) : 0;

                        var tile = tilemapConfig.GetTile(tileID);
                        
                        // Addons
                        if (_tileOverridesManager != null)
                        {
                            var overrideKey = new int3(chunkCoords.x * chunkSize + x, 
                                chunkCoords.y * chunkSize + y, tilemap.Key);
                            tile = _tileOverridesManager.tileOverridesData.TileOverrides.ContainsKey(overrideKey)
                                ? tilemapConfig.GetTile(_tileOverridesManager.tileOverridesData.TileOverrides[overrideKey]) 
                                : tile;
                        }
                        
                            
                        tilesArray[y * chunkSize + x] = prevID == 0 ? tile : prevTile; // prevent overrides from new generation cycles
                    }
                }
            
                tilemap.Value.SetTiles(positionsArray, tilesArray);
            }
        }

        public void RefreshChunk(int2 chunkCoords)
        {
            var chunkSize = _worldGenerator.ChunkSize;
            for (var x = 0; x < chunkSize; x++)
            {
                for (var y = 0; y < chunkSize; y++)
                {
                    var position = new Vector3Int((chunkCoords.x * chunkSize) + x,
                        (chunkCoords.y * chunkSize) + y, 0);
                    foreach (var tilemap in Tilemaps.Values)
                    {
                        tilemap.RefreshTile(position);
                    }
                }
            }
        }

        public void RemoveChunk(int2 chunkCoords)
        {
            var chunkSize = _worldGenerator.ChunkSize;
            var positionsArray = new Vector3Int[chunkSize * chunkSize];
            var tilesArray = new TileBase[chunkSize * chunkSize];
            
            for (var x = 0; x < chunkSize; x++)
            {
                for (var y = 0; y < chunkSize; y++)
                {
                    positionsArray[y * chunkSize + x] = new Vector3Int((chunkCoords.x * chunkSize) + x, 
                        (chunkCoords.y * chunkSize) + y, 0);
                }
            }

            foreach (var tilemap in Tilemaps.Values)
            {
                tilemap.SetTiles(positionsArray, tilesArray);
            }
        }

        public void RenderTile(int2 worldCoords, int tilemapID, int tileID)
        {
            var tilemap = Tilemaps[tilemapID];
            tilemap.SetTile(new Vector3Int(worldCoords.x, worldCoords.y, 0), tilemapConfig.GetTile(tileID));
        }

        public void RefreshTile(int2 worldCoords, int tilemapID)
        {
            var tilemap = Tilemaps[tilemapID];
            tilemap.RefreshTile(new Vector3Int(worldCoords.x, worldCoords.y, 0));
        }

        public void RemoveTile(int2 worldCoords, int tilemapID)
        {
            var tilemap = Tilemaps[tilemapID];
            tilemap.SetTile(new Vector3Int(worldCoords.x, worldCoords.y, 0), null);
        }
    }
}