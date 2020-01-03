using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes
{
    public struct MeshData
    {
        private readonly List<Vector3> _vertices;
        private readonly int[] _triangles;

        public List<Vector3> Vertices => _vertices;
        public int[] Triangles => _triangles;

        public MeshData(List<Vector3> vertices, int[] triangles)
        {
            _vertices = vertices;
            _triangles = triangles;
        }
    }
}