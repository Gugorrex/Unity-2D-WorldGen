using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Utils
{
    public static class GridMouseTilePosition
    {
        public static int2 GetMouseOnTile(Grid grid)
        {
            if (Camera.main == null) return int2.zero;
            var worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            var tilePos = grid.WorldToCell(worldPos);
            return new int2(tilePos.x, tilePos.y);
        }
    }
}