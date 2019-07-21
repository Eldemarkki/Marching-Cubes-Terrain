using UnityEngine;

namespace MarchingCubes
{
    public static class MarchingCubes
    {
        private static Vector3 VertexInterpolate(Vector3 p1, Vector3 p2, float v1, float v2, float isolevel)
        {
            if (Utils.Abs(isolevel - v1) < 0.000001f)
            {
                return p1;
            }
            if (Utils.Abs(isolevel - v2) < 0.000001f)
            {
                return p2;
            }
            if (Utils.Abs(v1 - v2) < 0.000001f)
            {
                return p1;
            }

            float mu = (isolevel - v1) / (v2 - v1);

            Vector3 p = p1 + mu * (p2 - p1);

            return p;
        }

        private static void March(VertexList vertexList, int cubeIndex, ref Vector3[] vertices, ref int vertexIndex)
        {
            int[] row = LookupTables.TriangleTable[cubeIndex];

            for (int i = 0; i < row.Length; i += 3)
            {
                vertices[vertexIndex] = vertexList[row[i + 0]];
                vertexIndex++;

                vertices[vertexIndex] = vertexList[row[i + 1]];
                vertexIndex++;

                vertices[vertexIndex] = vertexList[row[i + 2]];
                vertexIndex++;
            }
        }

        private static VertexList GenerateVertexList(VoxelCorners<float> densities, VoxelCorners<Vector3> corners, int edgeIndex, float isolevel)
        {
            VertexList vertexList = new VertexList();

            for (int i = 0; i < 12; i++)
            {
                if ((edgeIndex & (1 << i)) != 0)
                {
                    int[] edgePair = LookupTables.EdgeIndexTable[i];
                    int edge1 = edgePair[0];
                    int edge2 = edgePair[1];

                    Vector3 corner1 = corners[edge1];
                    Vector3 corner2 = corners[edge2];

                    float density1 = densities[edge1];
                    float density2 = densities[edge2];

                    vertexList[i] = VertexInterpolate(corner1, corner2, density1, density2, isolevel);
                }
            }

            return vertexList;
        }

        private static int CalculateCubeIndex(VoxelCorners<float> densities, float isolevel)
        {
            int cubeIndex = 0;

            for (int i = 0; i < 8; i++)
                if (densities[i] < isolevel)
                    cubeIndex |= 1 << i;

            return cubeIndex;
        }

        public static Mesh CreateMeshData(ValueGrid<float> densityField, float isolevel)
        {
            ValueGrid<int> cubeIndices = GenerateCubeIndices(densityField, isolevel);
            int vertexCount = GenerateVertexCount(cubeIndices);

            if (vertexCount <= 0)
            {
                return new Mesh();
            }

            Vector3[] vertices = new Vector3[vertexCount];
            int vertexIndex = 0;

            int i = 0;
            for (int x = 0; x < densityField.Width - 1; x++)
            {
                for (int y = 0; y < densityField.Height - 1; y++)
                {
                    for (int z = 0; z < densityField.Depth - 1; z++)
                    {
                        int cubeIndex = cubeIndices[i++];
                        if (cubeIndex == 0 || cubeIndex == 255) continue;

                        VoxelCorners<Vector3> corners = GetCorners(x,y,z);
                        VoxelCorners<float> densities = GetDensities(x, y, z, densityField);
                        int edgeIndex = LookupTables.EdgeTable[cubeIndex];

                        VertexList vertexList = GenerateVertexList(densities, corners, edgeIndex, isolevel);

                        March(vertexList, cubeIndex, ref vertices, ref vertexIndex);
                    }
                }
            }

            // The Marching Cubes algorithm produces vertices in an order that you can 
            // connect them in the order that they were created.
            int[] triangles = new int[vertexCount];
            for (int triangleIndex = 0; triangleIndex < vertexCount; triangleIndex++)
            {
                triangles[triangleIndex] = triangleIndex;
            }

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();

            return mesh;
        }

        private static VoxelCorners<Vector3> GetCorners(int x, int y, int z)
        {
            VoxelCorners<Vector3> corners = new VoxelCorners<Vector3>();

            Vector3 origin = new Vector3(x, y, z);
            for (int i = 0; i < 8; i++)
            {
                corners[i] = origin + LookupTables.CubeCorners[i];
            }

            return corners;
        }

        private static VoxelCorners<float> GetDensities(int x, int y, int z, ValueGrid<float> densityField)
        {
            VoxelCorners<float> densities = new VoxelCorners<float>();
            for (int i = 0; i < 8; i++)
            {
                densities[i] = densityField[x + LookupTables.CubeCornersX[i], y + LookupTables.CubeCornersY[i], z + LookupTables.CubeCornersZ[i]];
            }

            return densities;
        }

        private static ValueGrid<int> GenerateCubeIndices(ValueGrid<float> densityField, float isolevel)
        {
            ValueGrid<int> cubeIndices = new ValueGrid<int>(densityField.Width - 1, densityField.Height - 1, densityField.Depth - 1);

            int i = 0;
            for (int x = 0; x < densityField.Width - 1; x++)
            {
                for (int y = 0; y < densityField.Height - 1; y++)
                {
                    for (int z = 0; z < densityField.Depth - 1; z++)
                    {
                        cubeIndices[i++] = CalculateCubeIndex(GetDensities(x, y, z, densityField), isolevel);
                    }
                }
            }

            return cubeIndices;
        }

        private static int GenerateVertexCount(ValueGrid<int> cubeIndices)
        {
            int vertexCount = 0;

            int i = 0;
            for (int x = 0; x < cubeIndices.Width; x++)
            {
                for (int y = 0; y < cubeIndices.Height; y++)
                {
                    for (int z = 0; z < cubeIndices.Depth; z++)
                    {
                        int cubeIndex = cubeIndices[i++];
                        int[] row = LookupTables.TriangleTable[cubeIndex];
                        vertexCount += row.Length;
                    }
                }
            }

            return vertexCount;
        }
    }
}