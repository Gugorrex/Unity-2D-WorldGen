using System;
using Unity.Mathematics;

namespace _2D_WorldGen.Scripts.GenerationTree
{
    public readonly struct GenerationCycleData
    {
        public int Seed { get; }
        public int2 ChunkCoords { get; }
        public int ChunkSize { get; }
        public int TilemapCount { get; }

        public int ArrayLength => ChunkSize * ChunkSize;

        public GenerationCycleData(int seed, int2 chunkCoords, int chunkSize, int tilemapCount)
        {
            Seed = seed;
            ChunkCoords = chunkCoords;
            ChunkSize = chunkSize;
            TilemapCount = tilemapCount;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Seed, ChunkCoords, ChunkSize, TilemapCount);
        }
    }
}