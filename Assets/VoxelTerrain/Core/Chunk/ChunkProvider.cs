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

        /// <summary>
        /// Gets a collection of chunks that contain a world position. For a chunk to contain a position, the position has to be inside of the chunk or on the chunk's edge
        /// </summary>
        /// <param name="worldPosition">The world position to check</param>
        /// <param name="chunkSize">The size of the chunks</param>
        /// <returns>A collection of chunk coordinates that contain the world position</returns>
        public static IEnumerable<int3> GetChunkCoordinatesContainingPoint(int3 worldPosition, int chunkSize)
        {
            int3 localPosition = VectorUtilities.Mod(worldPosition, chunkSize);
          
            int chunkCheckCountX = localPosition.x == 0 ? 1 : 0;
            int chunkCheckCountY = localPosition.y == 0 ? 1 : 0;
            int chunkCheckCountZ = localPosition.z == 0 ? 1 : 0;

            int3 origin = VectorUtilities.WorldPositionToCoordinate(worldPosition, chunkSize);

            // The origin (worldPosition as a chunk coordinate) is always included
            yield return origin;

            // The first corner can be skipped, since it's (0, 0, 0) and would just return origin
            for (int i = 1; i < 8; i++)
            {
                var cornerOffset = LookupTables.CubeCorners[i];
                if(cornerOffset.x <= chunkCheckCountX && cornerOffset.y <= chunkCheckCountY && cornerOffset.z <= chunkCheckCountZ)
                {
                    yield return origin - cornerOffset;
                }
            }
        }
    }
}