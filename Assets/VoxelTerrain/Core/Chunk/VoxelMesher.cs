using Eldemarkki.VoxelTerrain.Density;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    public abstract class VoxelMesher : MonoBehaviour
    {
        public static readonly VertexAttributeDescriptor[] VertexBufferMemoryLayout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3)
        };

        public abstract Mesh CreateMesh(VoxelDataStore voxelDataStore, int3 chunkCoordinate);
    }
}