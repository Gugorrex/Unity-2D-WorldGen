using System;
using Unity.Mathematics;

namespace _2D_WorldGen.Scripts.GenerationTree.Core
{
    public struct GenerationCycleData
    {
        public int Seed { get; }
        public int2 ChunkCoords { get; }
        public int ChunkSize { get; } // TODO move to chunk manager

        public int ArrayLength => ChunkSize * ChunkSize;

        public GenerationCycleData(int seed, int2 chunkCoords, int chunkSize)
        {
            Seed = seed;
            ChunkCoords = chunkCoords;
            ChunkSize = chunkSize;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Seed, ChunkCoords, ChunkSize);
        }
    }
}