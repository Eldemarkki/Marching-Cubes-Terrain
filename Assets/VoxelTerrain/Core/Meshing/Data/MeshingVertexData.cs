using System;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Eldemarkki.VoxelTerrain.Meshing.Data
{
    /// <summary>
    /// A struct to hold the data every vertex should have
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshingVertexData : IEquatable<MeshingVertexData>
    {
        /// <summary>
        /// The vertex's local position
        /// </summary>
        public float3 position;

        /// <summary>
        /// The vertex's normal
        /// </summary>
        public float3 normal;

        /// <summary>
        /// The constructor to create a <see cref="MeshingVertexData"/>
        /// </summary>
        /// <param name="position">The vertex's local position</param>
        /// <param name="normal">The vertex's normal</param>
        public MeshingVertexData(float3 position, float3 normal)
        {
            this.position = position;
            this.normal = normal;
        }

        /// <summary>
        /// The memory layout of a single vertex in memory
        /// </summary>
        public static readonly VertexAttributeDescriptor[] VertexBufferMemoryLayout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position),
            new VertexAttributeDescriptor(VertexAttribute.Normal)
        };

        public bool Equals(MeshingVertexData other)
        {
            if(other == null) { return false; }

            return position.Equals(other.position) &&
                   normal.Equals(other.position);
        }

        public override bool Equals(object obj)
        {
            if(obj == null) { return false; }

            if(obj is MeshingVertexData other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + position.GetHashCode();
                hash = hash * 23 + normal.GetHashCode();

                return hash;
            }
        }

        public static bool operator ==(MeshingVertexData left, MeshingVertexData right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MeshingVertexData left, MeshingVertexData right)
        {
            return !(left == right);
        }
    }
}