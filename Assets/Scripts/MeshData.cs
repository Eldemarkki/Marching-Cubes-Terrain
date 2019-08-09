using System;
using System.Collections.Generic;
using UnityEngine;

namespace MarchingCubes
{
    public struct MeshData : IEquatable<MeshData>
    {
        private readonly List<Vector3> _vertices;
        private readonly int[] _triangles;

        public MeshData(List<Vector3> vertices, int[] triangles)
        {
            _vertices = vertices;
            _triangles = triangles;
        }

        public void Deconstruct(out List<Vector3> vertices, out int[] triangles)
        {
            vertices = _vertices;
            triangles = _triangles;
        }

        public bool Equals(MeshData other)
        {
            var (vertices, triangles) = other;
            return Equals(_vertices, vertices) && Equals(_triangles, triangles);
        }

        public override bool Equals(object obj)
        {
            return obj is MeshData other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((_vertices != null ? _vertices.GetHashCode() : 0) * 397) ^ (_triangles != null ? _triangles.GetHashCode() : 0);
            }
        }
    }
}