using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public static class BoundsUtilities
    {
        /// <summary>
        /// Calculates the world space bounds of a chunk
        /// </summary>
        public static BoundsInt CalculateChunkBounds(int3 chunkCoordinate, int3 chunkSize) => new BoundsInt((chunkCoordinate * chunkSize).ToVectorInt(), (chunkSize + 1).ToVectorInt());

        /// <summary>
        /// Calculates the volume of <paramref name="bounds"/>; how many <see cref="int3"/> points are inside of <paramref name="bounds"/>
        /// </summary>
        /// <param name="bounds">The bounds whose volume should be calculated</param>
        /// <returns>The amount of points inside of <paramref name="bounds"/></returns>
        public static int CalculateBoundsVolume(this BoundsInt bounds) => bounds.size.x * bounds.size.y * bounds.size.z;
    }
}