using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes
{
    public struct MeshData : IEquatable<MeshData>
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

        public bool Equals(MeshData other)
        {
            return other.Vertices == Vertices && other.Triangles == Triangles;
        }

        public override bool Equals(object obj)
        {
            return obj is MeshData other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ((_vertices != null ? _vertices.GetHashCode() : 0) * 397) ^ (_triangles != null ? _triangles.GetHashCode() : 0);
        }
    }
}