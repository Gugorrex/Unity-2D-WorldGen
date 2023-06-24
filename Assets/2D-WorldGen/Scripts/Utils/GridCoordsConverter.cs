using Unity.Mathematics;

namespace _2D_WorldGen.Scripts.Utils
{
    public struct GridCoordsConverter
    {
        public static int2 WorldToSubGridCoords(int subGridSize, int2 worldCoords, out int2 localSubCoords)
        {
            var subCoords = int2.zero;
            localSubCoords = int2.zero;

            subCoords.x = worldCoords.x >= 0 ? worldCoords.x / subGridSize : ((worldCoords.x + 1) / subGridSize) - 1;
            subCoords.y = worldCoords.y >= 0 ? worldCoords.y / subGridSize : ((worldCoords.y + 1) / subGridSize) - 1;

            localSubCoords.x = worldCoords.x >= 0
                ? worldCoords.x % subGridSize
                : worldCoords.x + (-subCoords.x * subGridSize);
            localSubCoords.y = worldCoords.y >= 0
                ? worldCoords.y % subGridSize
                : worldCoords.y + (-subCoords.y * subGridSize);
            
            return subCoords;
        }

        public static int2 SubToWorldGridCoords(int subGridSize, int2 subCoords)
        {
            var localSubCoords = int2.zero;
            return SubToWorldGridCoords(subGridSize, subCoords, localSubCoords);
        }
        
        public static int2 SubToWorldGridCoords(int subGridSize, int2 subCoords, int2 localSubCoords)
        {
            var worldCoords = int2.zero;

            worldCoords.x = subCoords.x * subGridSize + localSubCoords.x;
            worldCoords.y = subCoords.y * subGridSize + localSubCoords.y;

            return worldCoords;
        }
    }
}