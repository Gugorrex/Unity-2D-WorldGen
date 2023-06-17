using System;
using System.Collections.Generic;
using System.Linq;
using _2D_WorldGen.Scripts.Config;
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

        [Serializable]
        public struct TaggedTilemap
        {
            public string stringID;
            public Tilemap tilemap;
        }

        public Dictionary<int, Tilemap> Tilemaps => taggedTilemaps.ToDictionary(
            tilemap => tilemapConfig.GetTilemapID(tilemap.stringID), 
            tilemap => tilemap.tilemap);

        public void RenderChunk(int2 chunkCoords, int chunkSize, NativeArray<int> chunk)
        {
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
                        tilesArray[y * chunkSize + x] = tilemapConfig.GetTile(tileID);
                    }
                }
            
                tilemap.Value.SetTiles(positionsArray, tilesArray);
            }
        }

        public void RefreshChunk(int2 chunkCoords, int chunkSize)
        {
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

        public void RemoveChunk(int2 chunkCoords, int chunkSize)
        {
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

        /* TODO find out if we want to use chunk as input or directly use worldCoords -> where do we need these methods?
        public void RenderTile(int2 worldCoords, int chunkSize)
        {
            var normalChunkCoords = GridCoordsConverter.WorldToSubGridCoords(chunkSize, worldCoords, out var localCoords);
            var chunk = _chunkManager.GetChunk(normalChunkCoords);
            var yOffset = chunkSize;
            var zOffset = yOffset * yOffset;
            foreach (var tilemap in _tilemaps)
            {
                var tileID = chunk[tilemap.Key * zOffset + localCoords.y * yOffset + localCoords.x];
                var tile = _tilemapConfig.GetTile(tileID);
                tilemap.Value.SetTile(new Vector3Int(worldCoords.x, worldCoords.y, 0), tile);
            }
        }

        public void RefreshTile(int2 worldCoords)
        {
            foreach (var tilemap in _tilemaps.Values)
            {
                tilemap.RefreshTile(new Vector3Int(worldCoords.x, worldCoords.y, 0));
            }
        }

        public void RemoveTile(int2 worldCoords)
        {
            foreach (var tilemap in _tilemaps.Values)
            {
                tilemap.SetTile(new Vector3Int(worldCoords.x, worldCoords.y, 0), null);
            }
        }
        */
    }
}