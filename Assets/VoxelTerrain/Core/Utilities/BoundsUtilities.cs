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
        public static Bounds GetChunkBounds(int3 chunkCoordinate, int chunkSize)
        {
            Bounds bounds = new Bounds();

            int3 min = chunkCoordinate * chunkSize;
            Vector3 max = chunkCoordinate.ToVectorInt() * chunkSize + Vector3.one * (chunkSize + 1);
            bounds.SetMinMax(min.ToVectorInt(), max);

            return bounds;
        }
    }
}