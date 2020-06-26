using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A class for loading and initializing chunks
    /// </summary>
    public class ChunkLoader : MonoBehaviour
    {
        /// <summary>
        /// The world for which to generate the chunk for
        /// </summary>
        public VoxelWorld VoxelWorld { get; set; }

        /// <summary>
        /// Loads a chunk to a specific coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk to load</param>
        /// <returns>The newly loaded chunk</returns>
        public Chunk LoadChunkToCoordinate(int3 chunkCoordinate)
        {
            int3 worldPosition = chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize;
            Chunk chunk = Instantiate(VoxelWorld.WorldSettings.ChunkPrefab, worldPosition.ToVectorInt(), Quaternion.identity);

            Bounds chunkBounds = BoundsUtilities.GetChunkBounds(chunkCoordinate, VoxelWorld.WorldSettings.ChunkSize);
            var jobHandleWithData = VoxelWorld.VoxelDataGenerator.GenerateVoxelData(chunkBounds, Allocator.Persistent);
            VoxelWorld.VoxelDataStore.SetDensityChunkJobHandle(jobHandleWithData, chunkCoordinate);

            chunk.Initialize(chunkCoordinate, VoxelWorld);

            VoxelWorld.ChunkStore.AddChunk(chunk);

            return chunk;
        }
    }
}