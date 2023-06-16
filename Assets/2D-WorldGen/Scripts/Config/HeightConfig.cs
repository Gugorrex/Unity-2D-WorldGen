using System;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Config
{
    [CreateAssetMenu(fileName = "HeightConfig", menuName = "2D World Gen/Config/HeightConfig", order = 0)]
    public class HeightConfig : ScriptableObject
    {
        public TileHeight[] tileHeights;
        
        [Serializable]
        public struct TileHeight
        {
            public string tileStringID;
            [Range(0f, 1f)] public float height;
        }
        
    }
}