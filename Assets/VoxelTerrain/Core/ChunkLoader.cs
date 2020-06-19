using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    public class ChunkLoader : MonoBehaviour
    {
        public VoxelWorld VoxelWorld { get; set; }

        public Chunk LoadChunkToCoordinate(int3 chunkCoordinate)
        {
            int3 worldPosition = chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize;
            Chunk chunk = Instantiate(VoxelWorld.WorldSettings.ChunkPrefab, worldPosition.ToVectorInt(), Quaternion.identity);

            Bounds chunkBounds = BoundsUtilities.GetChunkBounds(chunkCoordinate, VoxelWorld.WorldSettings.ChunkSize);
            DensityVolume chunkDensities = VoxelWorld.VoxelDataGenerator.GenerateVoxelData(chunkBounds, Allocator.Persistent);

            VoxelWorld.VoxelDataStore.SetDensityChunk(chunkDensities, chunkCoordinate);

            chunk.Initialize(chunkCoordinate, VoxelWorld);

            VoxelWorld.ChunkStore.AddChunk(chunkCoordinate, chunk);

            return chunk;
        }
    }
}