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
        /// Calculates the cube index of a single voxel
        /// </summary>
        /// <param name="voxelDensities">The densities of the voxel</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The calculated cube index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe byte CalculateCubeIndex(float* voxelDensities, float isolevel)
        {
            float4 voxelDensitiesPart1 = new float4(voxelDensities[0], voxelDensities[1], voxelDensities[2], voxelDensities[3]);
            float4 voxelDensitiesPart2 = new float4(voxelDensities[4], voxelDensities[5], voxelDensities[6], voxelDensities[7]);

            int4 p1 = math.select(0, new int4(1, 2, 4, 8), voxelDensitiesPart1 < isolevel);
            int4 p2 = math.select(0, new int4(16, 32, 64, 128), voxelDensitiesPart2 < isolevel);

            return (byte)(p1.x | p1.y | p1.z | p1.w | p2.x | p2.y | p2.z | p2.w);
        }

        /// <summary>
        /// Gets a single interpolated triangle in a voxel
        /// </summary>
        /// <param name="indexInTriangleTable">The index of the triangle in <see cref="MarchingCubesLookupTables.TriangleTableWithLengths"/></param>
        /// <param name="voxelLocalPosition">The position of the voxel</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid), and densities above this will be outside the surface (air)</param>
        /// <param name="densities">The densities of the voxel</param>
        /// <returns>The points of the triangle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe float3x3 GetTriangle(int indexInTriangleTable, int3 voxelLocalPosition, float isolevel, float* densities)
        {
            int3 edgeIntersectionIndices = new int3(
                MarchingCubesLookupTables.TriangleTableWithLengths[indexInTriangleTable + 0],
                MarchingCubesLookupTables.TriangleTableWithLengths[indexInTriangleTable + 1],
                MarchingCubesLookupTables.TriangleTableWithLengths[indexInTriangleTable + 2]
            );

            int3 edgeStartIndices = new int3(
                MarchingCubesLookupTables.EdgeIndexTable[2 * edgeIntersectionIndices.x],
                MarchingCubesLookupTables.EdgeIndexTable[2 * edgeIntersectionIndices.y],
                MarchingCubesLookupTables.EdgeIndexTable[2 * edgeIntersectionIndices.z]
            );

            int3 edgeEndIndices = new int3(
                MarchingCubesLookupTables.EdgeIndexTable[2 * edgeIntersectionIndices.x + 1],
                MarchingCubesLookupTables.EdgeIndexTable[2 * edgeIntersectionIndices.y + 1],
                MarchingCubesLookupTables.EdgeIndexTable[2 * edgeIntersectionIndices.z + 1]
            );

            int3x3 startCorners = new int3x3(
                voxelLocalPosition + LookupTables.CubeCorners[edgeStartIndices.x],
                voxelLocalPosition + LookupTables.CubeCorners[edgeStartIndices.y],
                voxelLocalPosition + LookupTables.CubeCorners[edgeStartIndices.z]
            );

            int3x3 endCorners = new int3x3(
                voxelLocalPosition + LookupTables.CubeCorners[edgeEndIndices.x],
                voxelLocalPosition + LookupTables.CubeCorners[edgeEndIndices.y],
                voxelLocalPosition + LookupTables.CubeCorners[edgeEndIndices.z]
            );

            float3 startDensities = new float3(
                densities[edgeStartIndices.x],
                densities[edgeStartIndices.y],
                densities[edgeStartIndices.z]
            );

            float3 endDensities = new float3(
                densities[edgeEndIndices.x],
                densities[edgeEndIndices.y],
                densities[edgeEndIndices.z]
            );

            return VertexInterpolateTriangle(startCorners, endCorners, startDensities, endDensities, isolevel);
        }

        /// <summary>
        /// Interpolates the vertex's position on 3 different edges to form a triangle
        /// </summary>
        /// <param name="p1">The start corners of the 3 edges</param>
        /// <param name="p2">The end corners of the 3 edges</param>
        /// <param name="v1">The densities on the start corners</param>
        /// <param name="v2">The densities on the end corners</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The interpolated points of the new triangle</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3x3 VertexInterpolateTriangle(int3x3 p1, int3x3 p2, float3 v1, float3 v2, float isolevel)
        {
            float3 k = (isolevel - v1) / (v2 - v1);
            return p1 + (p2 - p1) * new float3x3(k.x, k.y, k.z);
        }
    }
}