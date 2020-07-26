using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.VoxelData;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    /// <summary>
    /// A class for providing chunks to the world
    /// </summary>
    public class ChunkProvider : MonoBehaviour
    {
        /// <summary>
        /// The world for which to provide chunks for
        /// </summary>
        public VoxelWorld VoxelWorld { get; set; }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a new chunk is created
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public virtual void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (!VoxelWorld.ChunkStore.DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                LoadChunkToCoordinate(chunkCoordinate);
            }
        }

        /// <summary>
        /// Loads a chunk to a specific coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk to load</param>
        /// <returns>The newly loaded chunk</returns>
        protected Chunk LoadChunkToCoordinate(int3 chunkCoordinate)
        {
            int3 worldPosition = chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize;
            Chunk chunk = Instantiate(VoxelWorld.WorldSettings.ChunkPrefab, worldPosition.ToVectorInt(), Quaternion.identity);

            Bounds chunkBounds = BoundsUtilities.GetChunkBounds(chunkCoordinate, VoxelWorld.WorldSettings.ChunkSize);
            JobHandleWithData<IVoxelDataGenerationJob> jobHandleWithData = VoxelWorld.VoxelDataGenerator.GenerateVoxelData(chunkBounds);
            VoxelWorld.VoxelDataStore.SetVoxelDataJobHandle(jobHandleWithData, chunkCoordinate);

            chunk.Initialize(chunkCoordinate, VoxelWorld);

            VoxelWorld.ChunkStore.AddChunk(chunk);

            return chunk;
        }
    }
}