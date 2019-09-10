using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MarchingCubes
{
    public static class MarchingCubes
    {
        private static Vector3 VertexInterpolate(Vector3 p1, Vector3 p2, float v1, float v2, float isolevel)
        {
            return p1 + (isolevel - v1) * (p2 - p1) / (v2 - v1);
        }

        private static VertexList GenerateVertexList(VoxelCorners<float> densities, VoxelCorners<Vector3Int> corners,
            int edgeIndex, float isolevel)
        {
            var vertexList = new VertexList();

            for (int i = 0; i < 12; i++)
            {
                if ((edgeIndex & (1 << i)) == 0) { continue; }
                
                int[] edgePair = LookupTables.EdgeIndexTable[i];
                int edge1 = edgePair[0];
                int edge2 = edgePair[1];

                Vector3Int corner1 = corners[edge1];
                Vector3Int corner2 = corners[edge2];

                float density1 = densities[edge1];
                float density2 = densities[edge2];

                vertexList[i] = VertexInterpolate(corner1, corner2, density1, density2, isolevel);
            }

            return vertexList;
        }

        private static int CalculateCubeIndex(VoxelCorners<float> densities, float isolevel)
        {
            int cubeIndex = 0;

            if (densities.c1 < isolevel) { cubeIndex |= 1; }
            if (densities.c2 < isolevel) { cubeIndex |= 2; }
            if (densities.c3 < isolevel) { cubeIndex |= 4; }
            if (densities.c4 < isolevel) { cubeIndex |= 8; }
            if (densities.c5 < isolevel) { cubeIndex |= 16; }
            if (densities.c6 < isolevel) { cubeIndex |= 32; }
            if (densities.c7 < isolevel) { cubeIndex |= 64; }
            if (densities.c8 < isolevel) { cubeIndex |= 128; }

            return cubeIndex;
        }

        public static MeshData CreateMeshData(ValueGrid<float> densityField, float isolevel)
        {
            var vertices = new List<Vector3>();

            int xAxis = densityField.Width * densityField.Height;
            int yAxis = densityField.Width;

            int originIndex = 0;
            for (int x = 0; x < densityField.Width-1; x++)
            {
                for (int y = 0; y < densityField.Height-1; y++)
                {
                    for (int z = 0; z < densityField.Depth-1; z++)
                    {
                        VoxelCorners<float> densities = GetDensities(originIndex, xAxis, yAxis, 1, densityField);
                        originIndex++;

                        int cubeIndex = CalculateCubeIndex(densities, isolevel);
                        if (cubeIndex == 0 || cubeIndex == 255) 
                        {
                            continue;
                        }

                        VoxelCorners<Vector3Int> corners = GetCorners(x,y,z);
                        int edgeIndex = LookupTables.EdgeTable[cubeIndex];

                        VertexList vertexList = GenerateVertexList(densities, corners, edgeIndex, isolevel);

                        int[] row = LookupTables.TriangleTable[cubeIndex];
                        for (int i = 0; i < row.Length; i++)
                        {
                            vertices.Add(vertexList[row[i]]);
                        }
                    }
                    originIndex++;
                }
                originIndex += densityField.Height;
            }

            // The Marching Cubes algorithm produces vertices in an order that you can 
            // connect them in the order that they were created.
            int[] triangles = Enumerable.Range(0, vertices.Count).ToArray();
            
            return new MeshData(vertices, triangles);
        }

        private static VoxelCorners<Vector3Int> GetCorners(int x, int y, int z)
        {
            var corners = new VoxelCorners<Vector3Int>();
            var origin = new Vector3Int(x, y, z);

            corners.c1 = origin + LookupTables.CubeCorners[0];
            corners.c2 = origin + LookupTables.CubeCorners[1];
            corners.c3 = origin + LookupTables.CubeCorners[2];
            corners.c4 = origin + LookupTables.CubeCorners[3];
            corners.c5 = origin + LookupTables.CubeCorners[4];
            corners.c6 = origin + LookupTables.CubeCorners[5];
            corners.c7 = origin + LookupTables.CubeCorners[6];
            corners.c8 = origin + LookupTables.CubeCorners[7];

            return corners;
        }

        private static VoxelCorners<float> GetDensities(int originIndex, int xAxis, int yAxis, int zAxis, ValueGrid<float> densityField)
        {
            return new VoxelCorners<float>
            (
                densityField.data[originIndex + LookupTables.CubeCornersX[0] * xAxis + LookupTables.CubeCornersY[0] * yAxis + LookupTables.CubeCornersZ[0] * zAxis],
                densityField.data[originIndex + LookupTables.CubeCornersX[1] * xAxis + LookupTables.CubeCornersY[1] * yAxis + LookupTables.CubeCornersZ[1] * zAxis],
                densityField.data[originIndex + LookupTables.CubeCornersX[2] * xAxis + LookupTables.CubeCornersY[2] * yAxis + LookupTables.CubeCornersZ[2] * zAxis],
                densityField.data[originIndex + LookupTables.CubeCornersX[3] * xAxis + LookupTables.CubeCornersY[3] * yAxis + LookupTables.CubeCornersZ[3] * zAxis],
                densityField.data[originIndex + LookupTables.CubeCornersX[4] * xAxis + LookupTables.CubeCornersY[4] * yAxis + LookupTables.CubeCornersZ[4] * zAxis],
                densityField.data[originIndex + LookupTables.CubeCornersX[5] * xAxis + LookupTables.CubeCornersY[5] * yAxis + LookupTables.CubeCornersZ[5] * zAxis],
                densityField.data[originIndex + LookupTables.CubeCornersX[6] * xAxis + LookupTables.CubeCornersY[6] * yAxis + LookupTables.CubeCornersZ[6] * zAxis],
                densityField.data[originIndex + LookupTables.CubeCornersX[7] * xAxis + LookupTables.CubeCornersY[7] * yAxis + LookupTables.CubeCornersZ[7] * zAxis]
            );
        }
    }
}