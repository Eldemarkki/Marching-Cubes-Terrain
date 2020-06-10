using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.MarchingCubes
{
    /// <summary>
    /// A struct to hold the data every vertex should have
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MarchingCubesVertexData
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
        /// The constructor to create a MarchingCubesVertexData
        /// </summary>
        /// <param name="position">The vertex's local position</param>
        /// <param name="normal">The vertex's normal</param>
        public MarchingCubesVertexData(float3 position, float3 normal)
        {
            this.position = position;
            this.normal = normal;
        }
    }
}