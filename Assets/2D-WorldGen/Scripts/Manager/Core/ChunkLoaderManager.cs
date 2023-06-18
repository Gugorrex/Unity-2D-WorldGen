using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace _2D_WorldGen.Scripts.Manager.Core
{
    public class ChunkLoaderManager : MonoBehaviour
    {
        public List<ChunkLoader> chunkLoaders;

        public bool NextChunkToLoad(List<int2> loadedChunks, out int2 chunkToLoad, out List<int2> chunksToRemove)
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
            chunksToRemove = loadedChunks.ToList().Except(loadingRange.ToList()).ToList(); // out-parameter
            
            // determine chunks to load by removing loaded chunks
            var diffLoadingRange = loadingRange.ToList().Except(loadedChunks.ToList()).ToList();
            
            // -> chunks which are in loading range and are already loaded are untouched!

            if (diffLoadingRange.Count > 0)
            {
                chunkToLoad = diffLoadingRange[0];
                return true;
            }
            chunkToLoad = int2.zero;
            return false;
        }
        
    }
}