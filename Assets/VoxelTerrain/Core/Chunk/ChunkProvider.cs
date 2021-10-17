using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    public abstract class ChunkProvider : MonoBehaviour
    {
        public VoxelWorld VoxelWorld { get; set; }

        /// <summary>
        /// Instantiates a chunk to <paramref name="chunkCoordinate"/> and initializes it, but does not generate its mesh yet.
        /// </summary>
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

        public abstract ChunkProperties EnsureChunkExistsAtCoordinate(int3 chunkCoordinate);
    }
}