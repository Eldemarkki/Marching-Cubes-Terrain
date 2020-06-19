using System.Collections.Generic;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A base class for providing chunks to the world
    /// </summary>
    public abstract class ChunkProvider : MonoBehaviour
    {
        public VoxelWorld VoxelWorld { get; set; }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a new chunk is created in the next frame
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public virtual void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (!VoxelWorld.ChunkStore.DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                VoxelWorld.ChunkLoader.LoadChunkToCoordinate(chunkCoordinate);
            }
        }

        /// <summary>
        /// Gets a list of chunks that contain a world position. For a chunk to contain a position, the position has to be inside of the chunk or on the chunk's edge
        /// </summary>
        /// <param name="worldPosition">The world position to check for</param>
        /// <returns>A list of chunks that contain the world position</returns>
        public static List<int3> GetChunkCoordinatesContainingPoint(int3 worldPosition, int chunkSize)
        {
            List<int3> chunkCoordinates = new List<int3>();

            for (int i = 0; i < 8; i++)
            {
                int3 chunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldPosition - LookupTables.CubeCorners[i], chunkSize);
                if (chunkCoordinates.Contains(chunkCoordinate))
                {
                    continue;
                }

                chunkCoordinates.Add(chunkCoordinate);
            }

            return chunkCoordinates;
        }
    }
}