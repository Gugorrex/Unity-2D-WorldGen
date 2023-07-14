using _2D_WorldGen.Scripts.Manager.Core;
using _2D_WorldGen.Scripts.TileOverrides;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Manager.Addons
{
    [RequireComponent(typeof(WorldGenerator))]
    public class TileOverridesManager : MonoBehaviour
    {
        public TileOverridesData tileOverridesData;

        private TilemapManager _tilemapManager;

        private void Awake()
        {
            tileOverridesData.Init();
            _tilemapManager = GetComponent<TilemapManager>();
        }
        
        public void AddTileOverride(int2 worldCoords, int tilemapID, int tileID)
        {
            // Apply Change directly
            _tilemapManager.RenderTile(worldCoords, tilemapID, tileID);
            _tilemapManager.RefreshTile(worldCoords, tilemapID);
            
            // TODO Apply Change to Pathfinding if Addon exists
            
            // Save Change
            tileOverridesData.AddTileOverride(worldCoords, tilemapID, tileID);
        }

        public void RemoveTileOverride(int2 worldCoords, int tilemapID)
        {
            // Apply Change directly
            _tilemapManager.RemoveTile(worldCoords, tilemapID);
            // TODO generate single tile with world generator and call RenderTile afterwards
            _tilemapManager.RefreshTile(worldCoords, tilemapID);
            
            // TODO Apply Change to Pathfinding if Addon exists
            
            // Save Change
            tileOverridesData.RemoveTileOverride(worldCoords, tilemapID);
        }
    }
}