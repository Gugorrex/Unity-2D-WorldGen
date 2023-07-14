using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.TileOverrides
{
    [CreateAssetMenu(fileName = "TileOverridesData", menuName = "2D World Gen/TileOverridesData", order = 0)]
    public class TileOverridesData : ScriptableObject
    {
        [Serializable]
        public class ListWrapper<T>
        {
            public List<T> list = new();
        }
        
        [Serializable]
        public struct SerializableTile
        {
            public int3 coords;
            public int tileID;

            public bool Equals(SerializableTile other)
            {
                return coords.Equals(other.coords) && tileID == other.tileID;
            }

            public override bool Equals(object obj)
            {
                return obj is SerializableTile other && Equals(other);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(coords, tileID);
            }
        }

        [Serializable]
        public class SerializableTileOverrides : ListWrapper<SerializableTile> {}

        public Dictionary<int3, int> TileOverrides => new(_tileOverrides);

        [SerializeField] private SerializableTileOverrides serializedTileOverrides;
        [NonSerialized] private Dictionary<int3, int> _tileOverrides;

        public void Init()
        {
            serializedTileOverrides ??= new SerializableTileOverrides();
            _tileOverrides = new Dictionary<int3, int>();
            foreach (var tile in serializedTileOverrides.list)
            {
                _tileOverrides.TryAdd(tile.coords, tile.tileID);
            }
        }
        
        public void AddTileOverride(int2 worldCoords, int tilemapID, int tileID)
        {
            _tileOverrides.TryAdd(new int3(worldCoords.x, worldCoords.y, tilemapID), tileID);
            
            serializedTileOverrides.list.Add(new SerializableTile
            {
                coords = new int3(worldCoords.x, worldCoords.y, tilemapID),
                tileID = tileID
            });
        }

        public void RemoveTileOverride(int2 worldCoords, int tilemapID)
        {
            _tileOverrides.Remove(new int3(worldCoords.x, worldCoords.y, tilemapID), out var tileID);
            
            serializedTileOverrides.list.Remove(new SerializableTile
            {
                coords = new int3(worldCoords.x, worldCoords.y, tilemapID),
                tileID = tileID
            });
        }
    }
}