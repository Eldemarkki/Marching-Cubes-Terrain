using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes
{
    [BurstCompile]
    public struct MarchingCubesJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float> densities;
        [ReadOnly] public float isolevel;
        [ReadOnly] public int chunkSize;
        [WriteOnly] public Counter counter;

        [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<Vector3> vertices;
        [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<int> triangles;

        public void Execute(int index)
        {
            VoxelCorners<float> densities = GetDensities(index);

            int cubeIndex = CalculateCubeIndex(densities, isolevel);
            if (cubeIndex == 0 || cubeIndex == 255)
            {
                return;
            }

            // Voxel's position inside the chunk. Goes from (0, 0, 0) to (chunkSize-1, chunkSize-1, chunkSize-1)
            int3 voxelLocalPosition = new int3(
                index / (chunkSize * chunkSize),
                index / chunkSize % chunkSize,
                index % chunkSize);

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VoxelCorners<float> GetDensities(int index)
        {
            int x = index / (chunkSize * chunkSize);
            int y = index / chunkSize % chunkSize;
            int z = index % chunkSize;

            VoxelCorners<float> densities = new VoxelCorners<float>();
            for (int i = 0; i < 8; i++)
            {
                int densityIndex = (x + LookupTables.CubeCornersX[i]) * (chunkSize + 1) * (chunkSize + 1) + (y + LookupTables.CubeCornersY[i]) * (chunkSize + 1) + z + LookupTables.CubeCornersZ[i];
                densities[i] = this.densities[densityIndex];
            }

            return densities;
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float3 VertexInterpolate(float3 p1, float3 p2, float v1, float v2, float isolevel)
        {
            return p1 + (isolevel - v1) * (p2 - p1) / (v2 - v1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private VertexList GenerateVertexList(VoxelCorners<float> densities, VoxelCorners<int3> corners,
            int edgeIndex, float isolevel)
        {
            var vertexList = new VertexList();

            for (int i = 0; i < 12; i++)
            {
                if ((edgeIndex & (1 << i)) == 0) { continue; }

                int edgeStartIndex = LookupTables.EdgeIndexTable[2*i+0];
                int edgeEndIndex = LookupTables.EdgeIndexTable[2*i+1];

                int3 corner1 = corners[edgeStartIndex];
                int3 corner2 = corners[edgeEndIndex];

                float density1 = densities[edgeStartIndex];
                float density2 = densities[edgeEndIndex];

                vertexList[i] = VertexInterpolate(corner1, corner2, density1, density2, isolevel);
            }

            return vertexList;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculateCubeIndex(VoxelCorners<float> densities, float isolevel)
        {
            int cubeIndex = 0;

            if (densities.Corner1 < isolevel) { cubeIndex |= 1; }
            if (densities.Corner2 < isolevel) { cubeIndex |= 2; }
            if (densities.Corner3 < isolevel) { cubeIndex |= 4; }
            if (densities.Corner4 < isolevel) { cubeIndex |= 8; }
            if (densities.Corner5 < isolevel) { cubeIndex |= 16; }
            if (densities.Corner6 < isolevel) { cubeIndex |= 32; }
            if (densities.Corner7 < isolevel) { cubeIndex |= 64; }
            if (densities.Corner8 < isolevel) { cubeIndex |= 128; }

            return cubeIndex;
        }
    }
}