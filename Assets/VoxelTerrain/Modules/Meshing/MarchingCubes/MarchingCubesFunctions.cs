using Eldemarkki.VoxelTerrain.Utilities;
using System.Runtime.CompilerServices;
using Eldemarkki.VoxelTerrain.Meshing.Data;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Meshing.MarchingCubes
{
    /// <summary>
    /// A collection of Marching Cubes -related functions
    /// </summary>
    public static class MarchingCubesFunctions
    {
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
        public static byte CalculateCubeIndex(VoxelCorners<float> voxelDensities, float isolevel)
        {
            byte cubeIndex = (byte)math.select(0, 1, voxelDensities.Corner1 < isolevel);
            cubeIndex |= (byte)math.select(0, 2, voxelDensities.Corner2 < isolevel);
            cubeIndex |= (byte)math.select(0, 4, voxelDensities.Corner3 < isolevel);
            cubeIndex |= (byte)math.select(0, 8, voxelDensities.Corner4 < isolevel);
            cubeIndex |= (byte)math.select(0, 16, voxelDensities.Corner5 < isolevel);
            cubeIndex |= (byte)math.select(0, 32, voxelDensities.Corner6 < isolevel);
            cubeIndex |= (byte)math.select(0, 64, voxelDensities.Corner7 < isolevel);
            cubeIndex |= (byte)math.select(0, 128, voxelDensities.Corner8 < isolevel);

            return cubeIndex;
        }

        /// <summary>
        /// Generates the vertex list for a single voxel
        /// </summary>
        /// <param name="voxelDensities">The densities of the voxel</param>
        /// <param name="voxelLocalPosition">The local voxel position of the voxel whose vertex list should be generated</param>
        /// <param name="edgeIndex">The edge index</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The generated vertex list for the voxel</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VertexList GenerateVertexList(VoxelCorners<float> voxelDensities, int3 voxelLocalPosition,
            int edgeIndex, float isolevel)
        {
            VertexList vertexList = new VertexList();

            for (int i = 0; i < 12; i++)
            {
                if ((edgeIndex & (1 << i)) == 0) { continue; }

                int edgeStartIndex = MarchingCubesLookupTables.EdgeIndexTable[2 * i + 0];
                int edgeEndIndex = MarchingCubesLookupTables.EdgeIndexTable[2 * i + 1];

                int3 corner1 = voxelLocalPosition + LookupTables.CubeCorners[edgeStartIndex];
                int3 corner2 = voxelLocalPosition + LookupTables.CubeCorners[edgeEndIndex];

                float density1 = voxelDensities[edgeStartIndex];
                float density2 = voxelDensities[edgeEndIndex];

                vertexList[i] = VertexInterpolate(corner1, corner2, density1, density2, isolevel);
            }

            return vertexList;
        }
    }
}