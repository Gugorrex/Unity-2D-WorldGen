using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace _2D_WorldGen.Scripts.Config
{
    [CreateAssetMenu(fileName = "TilemapConfig", menuName = "2D World Gen/Config/TilemapConfig")]
    public class TilemapConfig : ScriptableObject
    {

        [Serializable]
        public struct TilemapConfiguration
        {
            public string stringID;
            public bool isObstacle;
            public TileConfiguration[] tileConfigurations;
        }
        
        [Serializable]
        public record TileConfiguration
        {
            public string stringID;
            public TileBase tile;
            [Range(0, float.MaxValue)] public float tileCost = 1;
            [Range(0, 1)] public float height;
        }

        private record TileSettings
        {
            public TileBase Tile;
            public int TilemapID;
            public float TileCost = 1;
            public float Height;
        }

        [SerializeField] private int tileSize;
        [SerializeField] private TilemapConfiguration[] tilemapConfigurations;

        private Dictionary<int, TileSettings> _tileDictionary;
        private Dictionary<string, int> _tileIdMapping;
        private Dictionary<TileBase, int> _tileBaseIdMapping;
        private Dictionary<string, int> _tilemapIdMapping;
        private HashSet<int> _obstacleTilemapIDs;

        public int TileSize => tileSize;

        public int GetTileID(string stringID, out int tilemapID)
        {
            var tileID = _tileIdMapping[stringID];
            tilemapID = GetTileTilemapID(tileID);
            return tileID;
        }

        public int GetTileID(string stringID)
        {
            return GetTileID(stringID, out _);
        }

        public int GetTileID(TileBase tileBase)
        {
            return _tileBaseIdMapping[tileBase];
        }

        public int GetTileTilemapID(int tileID)
        {
            return _tileDictionary[tileID].TilemapID;
        }

        public TileBase GetTile(int tileID)
        {
            return _tileDictionary[tileID].Tile;
        }

        public float GetTileCost(int tileID)
        {
            return _tileDictionary[tileID].TileCost;
        }

        public int GetTilemapID(string stringID)
        {
            return _tilemapIdMapping[stringID];
        }

        public bool IsObstacle(int tilemapID)
        {
            return _obstacleTilemapIDs.Contains(tilemapID);
        }

        public float GetHeight(int tileID)
        {
            return _tileDictionary[tileID].Height;
        }

        public Dictionary<string, int> ReadonlyTileIdMapping => new(_tileIdMapping);
        public Dictionary<string, int> ReadonlyTilemapIdMapping => new(_tilemapIdMapping);

        public void CreateDictionaries()
        {
            _tileIdMapping = new Dictionary<string, int> { { "", 0 } };
            _tileBaseIdMapping = new Dictionary<TileBase, int>();
            _tilemapIdMapping = new Dictionary<string, int>();
            _tileDictionary = new Dictionary<int, TileSettings>
            { { 0, new TileSettings
            {
                Tile = null,
                TilemapID = -1, // valid for all tilemaps
                TileCost = 1,
                Height = -1 // valid for all heights
            } } };
            _obstacleTilemapIDs = new HashSet<int>();

            var i = 0; // tilemapID
            var j = 1; // tileID

            foreach (var configuration in tilemapConfigurations)
            {
                // tilemap
                if (_tilemapIdMapping.ContainsKey(configuration.stringID))
                {
                    Debug.LogWarning("stringID " + configuration.stringID + " is already used! (skipped)");
                    continue;
                }
                _tilemapIdMapping.Add(configuration.stringID, i);
                
                if (configuration.isObstacle)
                {
                    _obstacleTilemapIDs.Add(i);
                }

                // tiles
                foreach (var tile in configuration.tileConfigurations)
                {
                    if (tile.stringID.Length == 0)
                    {
                        Debug.LogWarning("TileSet-Warning: Empty stringID is reserved and can not be used!");
                        continue;
                    }
                    if (_tileIdMapping.ContainsKey(tile.stringID))
                    {
                        Debug.LogWarning("stringID " + tile.stringID + " is already used! (skipped)");
                        continue;
                    }
                
                    _tileIdMapping.Add(tile.stringID, j);
                    _tileBaseIdMapping.Add(tile.tile, j);
                    _tileDictionary.Add(j++, new TileSettings
                    {
                        Tile = tile.tile,
                        TilemapID = i,
                        TileCost = tile.tileCost,
                        Height = tile.height
                    });
                }

                i++;
            }
        }
    }
}