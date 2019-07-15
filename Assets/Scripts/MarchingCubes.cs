using UnityEngine;

namespace MarchingCubes
{
    public class MarchingCubes
    {
        private Vector3[] _vertices;
        private int[] _triangles;
        private float _isolevel;

        private int _vertexIndex;

        private Vector3[] _vertexList;
        private Vector3[] _initCorners;
        private float[] _initDensities;
        private Mesh _mesh;
        private int[,,] _cubeIndexes;

        public MarchingCubes(DensityField densityField, float isolevel)
        {
            _isolevel = isolevel;

            _mesh = new Mesh();

            _vertexIndex = 0;

            _vertexList = new Vector3[12];
            _initCorners = new Vector3[8];
            _initDensities = new float[8];
            _cubeIndexes = new int[densityField.Width - 1, densityField.Height - 1, densityField.Depth - 1];
        }

        private Vector3 VertexInterpolate(Vector3 p1, Vector3 p2, float v1, float v2)
        {
            if (Utils.Abs(_isolevel - v1) < 0.000001f)
            {
                return p1;
            }
            if (Utils.Abs(_isolevel - v2) < 0.000001f)
            {
                return p2;
            }
            if (Utils.Abs(v1 - v2) < 0.000001f)
            {
                return p1;
            }

            float mu = (_isolevel - v1) / (v2 - v1);

            Vector3 p = p1 + mu * (p2 - p1);

            return p;
        }

        private void March(Vector3[] vertexList, int cubeIndex)
        {
            int[] row = LookupTables.TriangleTable[cubeIndex];

            for (int i = 0; i < row.Length; i += 3)
            {
                _vertices[_vertexIndex] = vertexList[row[i + 0]];
                _triangles[_vertexIndex] = _vertexIndex;
                _vertexIndex++;

                _vertices[_vertexIndex] = vertexList[row[i + 1]];
                _triangles[_vertexIndex] = _vertexIndex;
                _vertexIndex++;

                _vertices[_vertexIndex] = vertexList[row[i + 2]];
                _triangles[_vertexIndex] = _vertexIndex;
                _vertexIndex++;
            }
        }

        private Vector3[] GenerateVertexList(float[] densities, Vector3[] corners, int edgeIndex)
        {
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

                    _vertexList[i] = VertexInterpolate(corner1, corner2, density1, density2);
                }
            }

            return _vertexList;
        }

        private int CalculateCubeIndex(float[] densities, float iso)
        {
            int cubeIndex = 0;

            for (int i = 0; i < 8; i++)
                if (densities[i] < iso)
                    cubeIndex |= 1 << i;

            return cubeIndex;
        }

        public Mesh CreateMeshData(DensityField densityField)
        {
            _cubeIndexes = GenerateCubeIndexes(densityField);
            int vertexCount = GenerateVertexCount(_cubeIndexes);

            if (vertexCount <= 0)
            {
                return new Mesh();
            }

            _vertices = new Vector3[vertexCount];
            _triangles = new int[vertexCount];

            for (int x = 0; x < densityField.Width - 1; x++)
            {
                for (int y = 0; y < densityField.Height - 1; y++)
                {
                    for (int z = 0; z < densityField.Depth - 1; z++)
                    {
                        int cubeIndex = _cubeIndexes[x, y, z];
                        if (cubeIndex == 0 || cubeIndex == 255) continue;

                        _initCorners = GetCorners(x,y,z);
                        _initDensities = GetDensities(x, y, z, densityField);

                        int edgeIndex = LookupTables.EdgeTable[cubeIndex];
                        _vertexList = GenerateVertexList(_initDensities, _initCorners, edgeIndex);

                        March(_vertexList, cubeIndex);
                    }
                }
            }

            _vertexIndex = 0;

            _mesh.Clear();

            _mesh.vertices = _vertices;
            _mesh.SetTriangles(_triangles, 0);
            _mesh.RecalculateNormals();

            return _mesh;
        }

        private Vector3[] GetCorners(int x, int y, int z)
        {
            Vector3 origin = new Vector3(x, y, z);
            for (int i = 0; i < 8; i++)
            {
                _initCorners[i] = origin + LookupTables.CubeCorners[i];
            }

            return _initCorners;
        }

        private float[] GetDensities(int x, int y, int z, DensityField densityField){
            for (int i = 0; i < 8; i++)
            {
                _initDensities[i] = densityField[x + LookupTables.CubeCornersX[i], y + LookupTables.CubeCornersY[i], z + LookupTables.CubeCornersZ[i]];
            }

            return _initDensities;
        }

        private int[,,] GenerateCubeIndexes(DensityField densityField)
        {
            for (int x = 0; x < densityField.Width - 1; x++)
            {
                for (int y = 0; y < densityField.Height - 1; y++)
                {
                    for (int z = 0; z < densityField.Depth - 1; z++)
                    {
                        _initDensities = GetDensities(x, y, z, densityField);

                        _cubeIndexes[x, y, z] = CalculateCubeIndex(_initDensities, _isolevel);
                    }
                }
            }

            return _cubeIndexes;
        }

        private int GenerateVertexCount(int[,,] cubeIndexes)
        {
            int vertexCount = 0;

            for (int x = 0; x < cubeIndexes.GetLength(0); x++)
            {
                for (int y = 0; y < cubeIndexes.GetLength(1); y++)
                {
                    for (int z = 0; z < cubeIndexes.GetLength(2); z++)
                    {
                        int cubeIndex = cubeIndexes[x, y, z];
                        int[] row = LookupTables.TriangleTable[cubeIndex];
                        vertexCount += row.Length;
                    }
                }
            }

            return vertexCount;
        }
    }
}