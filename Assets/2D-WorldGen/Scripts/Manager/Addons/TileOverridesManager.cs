﻿using _2D_WorldGen.Scripts.Manager.Core;
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
            var prevTileID = _tilemapManager.GetTileID(worldCoords, tilemapID);
            
            // Apply Change directly
            _tilemapManager.RenderTile(worldCoords, tilemapID, tileID);
            _tilemapManager.RefreshTile(worldCoords, tilemapID);
            
            // TODO Apply Change to Pathfinding if Addon exists
            
            // Save Change
            tileOverridesData.AddTileOverride(worldCoords, tilemapID, tileID, prevTileID);
        }

        public void RemoveTileOverride(int2 worldCoords, int tilemapID)
        {
            var tileOverrideKey = new int3(worldCoords.x, worldCoords.y, tilemapID);
            if (!tileOverridesData.TileOverrides.ContainsKey(tileOverrideKey))
            {
                return;
            }
            
            // Apply Change directly
            _tilemapManager.RenderTile(worldCoords, tilemapID, tileOverridesData.TileOverrides[tileOverrideKey].y);
            _tilemapManager.RefreshTile(worldCoords, tilemapID);
            
            // TODO Apply Change to Pathfinding if Addon exists
            
            // Save Change
            tileOverridesData.RemoveTileOverride(worldCoords, tilemapID);
        }
    }
}