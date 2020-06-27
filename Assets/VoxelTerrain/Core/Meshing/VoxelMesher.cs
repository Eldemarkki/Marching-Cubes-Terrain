using Eldemarkki.VoxelTerrain.VoxelData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    public abstract class VoxelMesher : MonoBehaviour
    {
        /// <summary>
        /// The memory layout of a single vertex in memory
        /// </summary>
        public static readonly VertexAttributeDescriptor[] VertexBufferMemoryLayout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3)
        };
        
        /// <summary>
        /// Starts a mesh generation job
        /// </summary>
        /// <param name="voxelDataStore">The store where to retrieve the voxel data from</param>
        /// <param name="chunkCoordinate">The coordinate of the chunk that will be generated</param>
        /// <returns>The job handle and the actual mesh generation job</returns>
        public abstract JobHandleWithData<IMesherJob> CreateMesh(VoxelDataStore voxelDataStore, int3 chunkCoordinate);
    }
}