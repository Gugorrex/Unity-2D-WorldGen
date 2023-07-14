using _2D_WorldGen.Scripts.Manager.Addons;
using _2D_WorldGen.Scripts.Manager.Core;
using _2D_WorldGen.Scripts.Utils;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Testing
{
    public class TileOverridesTesting : MonoBehaviour
    {
        public int tileID = 1;
        public KeyCode setOverride = KeyCode.None;
        public KeyCode removeOverride = KeyCode.None;
        public TileOverridesManager tileOverridesManager;
        public Grid grid;
        public TilemapManager tilemapManager;
        
        private void Update()
        {
            if (Input.GetKeyDown(setOverride))
            {
                Debug.Log("Place");
                var tilePos = GridMouseTilePosition.GetMouseOnTile(grid);
                var tilemapID = tilemapManager.TilemapConfig.GetTileTilemapID(tileID);
                tileOverridesManager.AddTileOverride(tilePos, tilemapID, tileID);
            }

            if (Input.GetKeyDown(removeOverride))
            {
                Debug.Log("Remove");
                var tilePos = GridMouseTilePosition.GetMouseOnTile(grid);
                var tilemapID = tilemapManager.TilemapConfig.GetTileTilemapID(tileID);
                tileOverridesManager.RemoveTileOverride(tilePos, tilemapID);
            }
        }
    }
}