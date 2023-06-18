using System.Collections.Generic;
using _2D_WorldGen.Scripts.Utils;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Manager.Core
{
    [RequireComponent(typeof(WorldGenerator))]
    public class ChunkLoader : MonoBehaviour
    {
        public int radiusInChunks;
        private int2 LoaderChunkCoords { get; set; }
        private WorldGenerator _worldGenerator;
        
        private void Awake()
        {
            _worldGenerator = GetComponent<WorldGenerator>();
        }

        private void Start()
        {
            var pos = transform.position;
            LoaderChunkCoords =
                GridCoordsConverter.WorldToSubGridCoords(_worldGenerator.ChunkSize, new int2((int)pos.x, (int)pos.y),
                    out _);
        }

        private void Update()
        {
            var pos = transform.position;
            LoaderChunkCoords =
                GridCoordsConverter.WorldToSubGridCoords(_worldGenerator.ChunkSize, new int2((int)pos.x, (int)pos.y),
                    out _);
        }

        public struct PriorityCoords
        {
            public int2 Coords;
            public int Priority;
        }
        
        public List<PriorityCoords> GetPriorityAssignedChunkCoords()
        {
            var chunkCoords = new List<PriorityCoords>();
            
            var loaderPos = LoaderChunkCoords;
            
            for (var x = loaderPos.x - radiusInChunks; x <= loaderPos.x + radiusInChunks; x++)
            {
                for (var y = loaderPos.y - radiusInChunks; y <= loaderPos.y + radiusInChunks; y++)
                {
                    var deltaX = Mathf.Abs(x - loaderPos.x);
                    var deltaY = Mathf.Abs(y - loaderPos.y);
                    
                    chunkCoords.Add(new PriorityCoords()
                    {
                        Coords = new int2(x,y), 
                        
                        // squared distance as priority metric (higher performance than real distance (sqrt))
                        Priority = (deltaX * deltaX) + (deltaY * deltaY)
                    });
                }
            }

            return chunkCoords;
        }
    }
}