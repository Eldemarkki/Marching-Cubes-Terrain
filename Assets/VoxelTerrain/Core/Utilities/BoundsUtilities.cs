using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public static class BoundsUtilities
    {
        public static Bounds GetChunkBounds(int3 chunkCoordinate, int chunkSize)
        {
            Bounds bounds = new Bounds();

            var min = chunkCoordinate * chunkSize;
            var max = chunkCoordinate.ToVectorInt() * chunkSize + Vector3.one * (chunkSize + 1);
            bounds.SetMinMax(min.ToVectorInt(), max);

            return bounds;
        }
    }
}