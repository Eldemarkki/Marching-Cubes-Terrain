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
        public static Bounds GetChunkBounds(int3 chunkCoordinate, int3 chunkSize)
        {
            Bounds bounds = new Bounds();

            int3 min = chunkCoordinate * chunkSize;
            int3 max = chunkCoordinate * chunkSize + new int3(1, 1, 1) * (chunkSize + new int3(1, 1, 1));
            bounds.SetMinMax(min.ToVectorInt(), max.ToVectorInt());

            return bounds;
        }
    }
}