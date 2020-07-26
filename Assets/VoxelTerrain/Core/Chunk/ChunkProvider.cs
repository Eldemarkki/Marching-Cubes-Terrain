using System.Collections.Generic;
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
        /// Ensures that a chunk exists at a coordinate, if there is not, a new chunk is created
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public virtual void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (!VoxelWorld.ChunkStore.DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                VoxelWorld.ChunkLoader.LoadChunkToCoordinate(chunkCoordinate);
            }
        }
    }
}