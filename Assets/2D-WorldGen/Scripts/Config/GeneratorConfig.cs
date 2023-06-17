using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Config
{
    [CreateAssetMenu(fileName = "GeneratorConfig", menuName = "2D World Gen/Config/GeneratorConfig", order = 0)]
    public class GeneratorConfig : ScriptableObject
    {
        public GeneratorAction action;
        
        [SerializeField] private HeightConfigsMapEntry[] heightConfigsMap;

        public Dictionary<int, HeightConfig> HeightConfigs => heightConfigsMap.ToDictionary(entry => entry.id, entry => entry.heightConfig);

        [Serializable]
        public struct HeightConfigsMapEntry
        {
            public int id;
            public HeightConfig heightConfig;
        }
        
        [Serializable]
        public enum GeneratorAction
        {
            ApplyChunk
        }
        
    }
}