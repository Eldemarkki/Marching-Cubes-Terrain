using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public static class BoundsUtilities
    {
        /// <summary>
        /// Calculates the world space bounds of a chunk
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk</param>
        /// <param name="chunkSize">The size of the chunk</param>
        /// <returns>That chunk's world space bounds</returns>
        public static BoundsInt GetChunkBounds(int3 chunkCoordinate, int3 chunkSize)
        {
            int3 min = chunkCoordinate * chunkSize;
            int3 size = new int3(1, 1, 1) * (chunkSize + new int3(1, 1, 1));

            return new BoundsInt(min.ToVectorInt(), size.ToVectorInt());
        }
    }
}