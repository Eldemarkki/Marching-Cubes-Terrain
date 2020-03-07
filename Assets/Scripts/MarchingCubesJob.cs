using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes
{
    /// <summary>
    /// A marching cubes mesh generation job
    /// </summary>
    [BurstCompile]
    public struct MarchingCubesJob : IJobParallelFor
    {
        /// <summary>
        /// The densities to generate the mesh off of
        /// </summary>
        [ReadOnly] public NativeArray<float> densities;
        
        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        [ReadOnly] public float isolevel;
        
        /// <summary>
        /// The chunk's size. This represents the width, height and depth in Unity units.
        /// </summary>
        [ReadOnly] public int chunkSize;
        
        /// <summary>
        /// The counter to keep track of the triangle index
        /// </summary>
        [WriteOnly] public Counter counter;

        /// <summary>
        /// The generated vertices
        /// </summary>
        [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<Vector3> vertices;
        
        /// <summary>
        /// The generated triangles
        /// </summary>
        [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<int> triangles;

        /// <summary>
        /// The execute method required by the Unity Job System's IJobParallelFor
        /// </summary>
        /// <param name="index">The iteration index</param>
        public void Execute(int index)
        {
            // Voxel's position inside the chunk. Goes from (0, 0, 0) to (chunkSize-1, chunkSize-1, chunkSize-1)
            int3 voxelLocalPosition = new int3(
                index / (chunkSize * chunkSize),
                index / chunkSize % chunkSize,
                index % chunkSize);
            
            VoxelCorners<float> densities = GetDensities(voxelLocalPosition);

            int cubeIndex = CalculateCubeIndex(densities, isolevel);
            if (cubeIndex == 0 || cubeIndex == 255)
            {
                return;
            }

            VoxelCorners<int3> corners = GetCorners(voxelLocalPosition);

            int edgeIndex = LookupTables.EdgeTable[cubeIndex];

            VertexList vertexList = GenerateVertexList(densities, corners, edgeIndex, isolevel);

            // Index at the beginning of the row
            int rowIndex = 15 * cubeIndex;

            for (int i = 0; LookupTables.TriangleTable[rowIndex+i] != -1 && i < 15; i += 3)
            {
                int triangleIndex = counter.Increment() * 3;

                vertices[triangleIndex + 0] = vertexList[LookupTables.TriangleTable[rowIndex + i + 0]];
                triangles[triangleIndex + 0] = triangleIndex + 0;

                vertices[triangleIndex + 1] = vertexList[LookupTables.TriangleTable[rowIndex + i + 1]];
                triangles[triangleIndex + 1] = triangleIndex + 1;

                vertices[triangleIndex + 2] = vertexList[LookupTables.TriangleTable[rowIndex + i + 2]];
                triangles[triangleIndex + 2] = triangleIndex + 2;
            }
        }

        /// <summary>
        /// Gets the densities for the voxel at a position
        /// </summary>
        /// <param name="localPosition">Voxel's local position</param>
        /// <returns>The densities of that voxel</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VoxelCorners<float> GetDensities(int3 localPosition)
        {
            VoxelCorners<float> densities = new VoxelCorners<float>();
            for (int i = 0; i < 8; i++)
            {
                int3 voxelCorner = localPosition + LookupTables.CubeCorners[i];
                int densityIndex = voxelCorner.x * (chunkSize + 1) * (chunkSize + 1) + voxelCorner.y * (chunkSize + 1) + voxelCorner.z;
                densities[i] = this.densities[densityIndex];
            }

            return densities;
        }

        /// <summary>
        /// Gets the corners for the voxel at a position
        /// </summary>
        /// <param name="position">The voxel's position</param>
        /// <returns>The voxel's corners</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VoxelCorners<int3> GetCorners(int3 position)
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
        private float3 VertexInterpolate(float3 p1, float3 p2, float v1, float v2, float isolevel)
        {
            return p1 + (isolevel - v1) * (p2 - p1) / (v2 - v1);
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
        private VertexList GenerateVertexList(VoxelCorners<float> voxelDensities, VoxelCorners<int3> voxelCorners,
            int edgeIndex, float isolevel)
        {
            var vertexList = new VertexList();

            for (int i = 0; i < 12; i++)
            {
                if ((edgeIndex & (1 << i)) == 0) { continue; }

                int edgeStartIndex = LookupTables.EdgeIndexTable[2*i+0];
                int edgeEndIndex = LookupTables.EdgeIndexTable[2*i+1];

                int3 corner1 = voxelCorners[edgeStartIndex];
                int3 corner2 = voxelCorners[edgeEndIndex];

                float density1 = voxelDensities[edgeStartIndex];
                float density2 = voxelDensities[edgeEndIndex];

                vertexList[i] = VertexInterpolate(corner1, corner2, density1, density2, isolevel);
            }

            return vertexList;
        }

        /// <summary>
        /// Calculates the cube index of a single voxel
        /// </summary>
        /// <param name="voxelDensities">The voxel's densities</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)</param>
        /// <returns>The calculated cube index</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculateCubeIndex(VoxelCorners<float> voxelDensities, float isolevel)
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
    }
}