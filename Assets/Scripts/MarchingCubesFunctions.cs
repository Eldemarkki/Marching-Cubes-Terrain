using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace MarchingCubes
{
    /// <summary>
    /// A collection of Marching Cubes -related functions
    /// </summary>
    public static class MarchingCubesFunctions
    {
        /// <summary>
        /// Gets the corners for the voxel at a position
        /// </summary>
        /// <param name="position">The voxel's position</param>
        /// <returns>The voxel's corners</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VoxelCorners<int3> GetCorners(int3 position)
        {
            VoxelCorners<int3> corners = new VoxelCorners<int3>();
            for (int i = 0; i < 8; i++)
            {
                corners[i] = position + LookupTables.CubeCorners[i];
            }

            return corners;
        }

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
        /// <param name="voxelDensities">The voxel's densities</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The calculated cube index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int CalculateCubeIndex(VoxelCorners<float> voxelDensities, float isolevel)
        {
            int cubeIndex = 0;

            if (voxelDensities.Corner1 < isolevel) { cubeIndex |= 1; }
            if (voxelDensities.Corner2 < isolevel) { cubeIndex |= 2; }
            if (voxelDensities.Corner3 < isolevel) { cubeIndex |= 4; }
            if (voxelDensities.Corner4 < isolevel) { cubeIndex |= 8; }
            if (voxelDensities.Corner5 < isolevel) { cubeIndex |= 16; }
            if (voxelDensities.Corner6 < isolevel) { cubeIndex |= 32; }
            if (voxelDensities.Corner7 < isolevel) { cubeIndex |= 64; }
            if (voxelDensities.Corner8 < isolevel) { cubeIndex |= 128; }

            return cubeIndex;
        }

        /// <summary>
        /// Generates the vertex list for a single voxel
        /// </summary>
        /// <param name="voxelDensities">The voxel's densities</param>
        /// <param name="voxelCorners">The voxel's corners</param>
        /// <param name="edgeIndex">The edge index</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The generated vertex list for the voxel</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VertexList GenerateVertexList(VoxelCorners<float> voxelDensities, VoxelCorners<int3> voxelCorners,
            int edgeIndex, float isolevel)
        {
            var vertexList = new VertexList();

            for (int i = 0; i < 12; i++)
            {
                if ((edgeIndex & (1 << i)) == 0) { continue; }

                int edgeStartIndex = LookupTables.EdgeIndexTable[2 * i + 0];
                int edgeEndIndex = LookupTables.EdgeIndexTable[2 * i + 1];

                int3 corner1 = voxelCorners[edgeStartIndex];
                int3 corner2 = voxelCorners[edgeEndIndex];

                float density1 = voxelDensities[edgeStartIndex];
                float density2 = voxelDensities[edgeEndIndex];

                vertexList[i] = VertexInterpolate(corner1, corner2, density1, density2, isolevel);
            }

            return vertexList;
        }
    }
}