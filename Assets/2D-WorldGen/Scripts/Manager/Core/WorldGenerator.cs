using System;
using _2D_WorldGen.Scripts.Config;
using _2D_WorldGen.Scripts.GenerationTree;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Manager.Core
{
    [RequireComponent(typeof(TilemapManager))]
    [RequireComponent(typeof(ChunkLoaderManager))]
    public class WorldGenerator : MonoBehaviour
    {
        [SerializeField] private int chunkSize;
        [SerializeField] private NodeConfigMatch[] generationSchedule;

        public int ChunkSize => chunkSize;

        [Serializable]
        public struct NodeConfigMatch
        {
            public GenerationAlgorithm generationAlgorithm;
            public GeneratorConfig generatorConfig;
        }
    }
}