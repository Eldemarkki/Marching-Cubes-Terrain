using Eldemarkki.VoxelTerrain.Utilities;
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
        /// Instantiates a chunk to <paramref name="chunkCoordinate"/> and initializes it, but does not generate its mesh
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <returns>The new chunk</returns>
        protected ChunkProperties CreateUnloadedChunkToCoordinate(int3 chunkCoordinate)
        {
            int3 worldPosition = chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize;
            GameObject chunkGameObject = Instantiate(VoxelWorld.WorldSettings.ChunkPrefab, worldPosition.ToVectorInt(), Quaternion.identity);

            ChunkProperties chunkProperties = new ChunkProperties(VoxelWorld.WorldSettings.ChunkSize)
            {
                ChunkGameObject = chunkGameObject
            };

            chunkProperties.Move(chunkCoordinate);

            VoxelWorld.ChunkStore.AddChunk(chunkCoordinate, chunkProperties);

            return chunkProperties;
        }

        /// <summary>
        /// Instantiates a chunk to <paramref name="chunkCoordinate"/>, initializes it and generates its mesh
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk to create</param>
        /// <returns>The new chunk</returns>
        public ChunkProperties CreateLoadedChunkToCoordinateImmediate(int3 chunkCoordinate)
        {
            ChunkProperties chunkProperties = CreateUnloadedChunkToCoordinate(chunkCoordinate);
            VoxelWorld.ChunkUpdater.GenerateChunkImmediate(chunkProperties);
            return chunkProperties;
        }
    }
}