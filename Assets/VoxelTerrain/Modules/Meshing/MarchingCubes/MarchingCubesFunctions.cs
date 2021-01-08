using Eldemarkki.VoxelTerrain.Utilities;
using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Meshing.MarchingCubes
{
    /// <summary>
    /// A collection of Marching Cubes -related functions
    /// </summary>
    public static class MarchingCubesFunctions
    {
        /// <summary>
        /// Multiply a byte by this constant, to convert it to a float in the range [0, 1]
        /// </summary>
        public const float ByteToFloat = 1f / 255f;

        /// <summary>
        /// Interpolates the vertex's position 
        /// </summary>
        /// <param name="p1">The first corner's position</param>
        /// <param name="p2">The second corner's position</param>
        /// <param name="v1">The first corner's density</param>
        /// <param name="v2">The second corner's density</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The interpolated vertex's position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 VertexInterpolate(float3 p1, float3 p2, float v1, float v2, float isolevel)
        {
            return p1 + (isolevel - v1) * (p2 - p1) / (v2 - v1);
        }

        /// <summary>
        /// Calculates the cube index of a single voxel
        /// </summary>
        /// <param name="voxelDensities">The densities of the voxel</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The calculated cube index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte CalculateCubeIndex(float* voxelDensities, float isolevel)
        {
            byte cubeIndex = (byte)math.select(0, 1, voxelDensities[0] < isolevel);
            cubeIndex |= (byte)math.select(0, 2, voxelDensities[1] < isolevel);
            cubeIndex |= (byte)math.select(0, 4, voxelDensities[2] < isolevel);
            cubeIndex |= (byte)math.select(0, 8, voxelDensities[3] < isolevel);
            cubeIndex |= (byte)math.select(0, 16, voxelDensities[4] < isolevel);
            cubeIndex |= (byte)math.select(0, 32, voxelDensities[5] < isolevel);
            cubeIndex |= (byte)math.select(0, 64, voxelDensities[6] < isolevel);
            cubeIndex |= (byte)math.select(0, 128, voxelDensities[7] < isolevel);

            return cubeIndex;
        }

        /// <summary>
        /// Gets a vertex that is located on edge <paramref name="indexInTriangleTable"/>. If none exists, returns float3(0,0,0)
        /// </summary>
        /// <param name="indexInTriangleTable">The index of the edge in <see cref="MarchingCubesLookupTables.TriangleTableWithLengths"></see></param>
        /// <param name="voxelLocalPosition">The local voxel position of the voxel whose vertex should be genererated</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <param name="voxelDensities">The densities of the voxel</param>
        /// <returns>The position of the vertex on the edge</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float3 GetVertex(int indexInTriangleTable, int3 voxelLocalPosition, float isolevel, float* voxelDensities)
        {
            int edgeIntersectionIndex = MarchingCubesLookupTables.TriangleTableWithLengths[indexInTriangleTable];

            int edgeStartIndex = MarchingCubesLookupTables.EdgeIndexTable[2 * edgeIntersectionIndex + 0];
            int edgeEndIndex = MarchingCubesLookupTables.EdgeIndexTable[2 * edgeIntersectionIndex + 1];

            int3 corner1 = voxelLocalPosition + LookupTables.CubeCorners[edgeStartIndex];
            int3 corner2 = voxelLocalPosition + LookupTables.CubeCorners[edgeEndIndex];

            float density1 = voxelDensities[edgeStartIndex];
            float density2 = voxelDensities[edgeEndIndex];

            return VertexInterpolate(corner1, corner2, density1, density2, isolevel);
        }
    }
}