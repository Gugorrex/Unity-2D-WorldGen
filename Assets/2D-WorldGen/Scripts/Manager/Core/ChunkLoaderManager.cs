using System.Collections.Generic;
using System.Linq;
using _2D_WorldGen.Scripts.Manager.Addons;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Manager.Core
{
    [RequireComponent(typeof(TilemapManager))]
    public class ChunkLoaderManager : MonoBehaviour
    {
        public List<ChunkLoader> chunkLoaders;
        private readonly List<int2> _loadedChunks = new();
        private TilemapManager _tilemapManager;
        
        // Pathfinding
        private PathfindingManager _pathfindingManager;

        private void Awake()
        {
            _tilemapManager = GetComponent<TilemapManager>();

            if (TryGetComponent<PathfindingManager>(out var pathfindingManager))
            {
                _pathfindingManager = pathfindingManager;
            }
        }

        /// <summary>
        /// updates loading range of chunks. The loading range can lose as many chunks as needed.
        /// However, only one chunk can be added per call, to allow the world generator to generate one chunk per frame.
        /// </summary>
        /// <param name="chunkToGenerate"></param>
        /// <returns>true if a chunk needs to be generated</returns>
        public bool UpdateLoadingRange(out int2 chunkToGenerate)
        {
            // Append all chunk loaders normalized chunk coordinates to loading range
            var accList = new List<ChunkLoader.PriorityCoords>();
            foreach (var chunkLoader in chunkLoaders)
            {
                accList.AddRange(chunkLoader.GetPriorityAssignedChunkCoords());
            }
            accList.Sort((pc1, pc2) => pc1.Priority - pc2.Priority);
            var loadingRange = accList.Select(pc => pc.Coords).Distinct().ToList();
            
            // remove already loaded chunks which are no longer in the loading range
            var chunksToRemove = _loadedChunks.ToList().Except(loadingRange.ToList()).ToList();
            foreach (var chunk in chunksToRemove)
            {
                _tilemapManager.RemoveChunk(chunk);
                _tilemapManager.RefreshChunk(chunk);
                _loadedChunks.Remove(chunk);
                
                // Pathfinding
                if (_pathfindingManager != null)
                {
                    _pathfindingManager.RemoveChunk(chunk);
                }
            }

            // determine chunks to load by removing loaded chunks
            var diffLoadingRange = loadingRange.ToList().Except(_loadedChunks.ToList()).ToList();
            
            // -> chunks which are in loading range and are already loaded are untouched!

            if (diffLoadingRange.Count > 0)
            {
                chunkToGenerate = diffLoadingRange[0];
                _loadedChunks.Add(chunkToGenerate);
                return true;
            }
            chunkToGenerate = int2.zero;
            return false;
        }
        
    }
}