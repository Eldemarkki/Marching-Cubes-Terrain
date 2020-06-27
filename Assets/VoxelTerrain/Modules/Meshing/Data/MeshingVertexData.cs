using System.Runtime.InteropServices;
using Unity.Mathematics;
using UnityEngine.Rendering;

namespace Eldemarkki.VoxelTerrain.Meshing.Data
{
    /// <summary>
    /// A struct to hold the data every vertex should have
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshingVertexData
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
    }
}