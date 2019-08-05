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
            VertexList vertexList = new VertexList();

            for (int i = 0; i < 12; i++)
            {
                if ((edgeIndex & (1 << i)) != 0)
                {
                    int[] edgePair = LookupTables.EdgeIndexTable[i];
                    int edge1 = edgePair[0];
                    int edge2 = edgePair[1];

                    Vector3Int corner1 = corners[edge1];
                    Vector3Int corner2 = corners[edge2];

                    float density1 = densities[edge1];
                    float density2 = densities[edge2];

                    vertexList[i] = VertexInterpolate(corner1, corner2, density1, density2, isolevel);
                }
            }

            return vertexList;
        }

        public static Mesh CreateMeshData(ValueGrid<Voxel> voxels, float isolevel)
        {
            ValueGrid<int> cubeIndices = CalculateCubeIndices(voxels, isolevel);
            int vertexCount = CalculateVertexCount(cubeIndices);
            if (vertexCount == 0 || voxels == null)
            {
                return new Mesh();
            }

            Vector3[] vertices = new Vector3[vertexCount];

            int vertexIndex = 0;
            for (int i = 0; i < voxels.Size; i++)
            {
                Voxel voxel = voxels[i];

                int cubeIndex = cubeIndices[i];
                if (cubeIndex != 0 && cubeIndex != 255)
                {
                    March(voxel, cubeIndex, isolevel, ref vertices, ref vertexIndex);
                }
            }

            // The Marching Cubes algorithm produces vertices in an order that you can 
            // connect them in the order that they were created.
            int[] triangles = Enumerable.Range(0, vertices.Length).ToArray();

            Mesh mesh = new Mesh();

            mesh.vertices = vertices;
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();

            return mesh;
        }

        private static int CalculateVertexCount(ValueGrid<int> cubeIndices)
        {
            int vertexCount = 0;
            for (int i = 0; i < cubeIndices.Size; i++)
            {
                vertexCount += LookupTables.TriangleTable[cubeIndices[i]].Length;
            }

            return vertexCount;
        }

        private static ValueGrid<int> CalculateCubeIndices(ValueGrid<Voxel> voxels, float isolevel)
        {
            ValueGrid<int> cubeIndices = new ValueGrid<int>(voxels.Width, voxels.Height, voxels.Depth);
            for (int i = 0; i < voxels.Size; i++)
            {
                cubeIndices[i] = voxels[i].CalculateCubeIndex(isolevel);
            }

            return cubeIndices;
        }

        private static void March(Voxel voxel, int cubeIndex, float isolevel, ref Vector3[] vertices, ref int vertexIndex)
        {
            VoxelCorners<Vector3Int> corners = voxel.cornerPositions;
            VoxelCorners<float> densities = voxel.densities;
            int edgeIndex = LookupTables.EdgeTable[cubeIndex];

            VertexList vertexList = GenerateVertexList(densities, corners, edgeIndex, isolevel);

            int[] row = LookupTables.TriangleTable[cubeIndex];
            for (int i = 0; i < row.Length; i++)
            {
                vertices[vertexIndex++] = vertexList[row[i]];
            }
        }
    }
}