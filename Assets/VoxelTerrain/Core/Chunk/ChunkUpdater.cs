using Eldemarkki.VoxelTerrain.Meshing;
using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    public class ChunkUpdater : MonoBehaviour
    {
        /// <summary>
        /// The world for which to provide chunks for
        /// </summary>
        public VoxelWorld VoxelWorld { get; set; }

        private void Update()
        {
            foreach (ChunkProperties chunkProperties in VoxelWorld.ChunkStore.Chunks)
            {
                if (chunkProperties.HasChanges)
                {
                    GenerateMeshImmediate(chunkProperties);
                }
            }
        }

        /// <summary>
        /// Generates the voxel data and colors for this chunk and generates the mesh
        /// </summary>
        public void GenerateVoxelDataAndMeshImmediate(ChunkProperties chunkProperties)
        {
            VoxelWorld.VoxelDataStore.GenerateDataForChunk(chunkProperties.ChunkCoordinate);
            VoxelWorld.VoxelColorStore.GenerateDataForChunk(chunkProperties.ChunkCoordinate);
            GenerateMeshImmediate(chunkProperties);
        }

        /// <summary>
        /// Forces the regeneration of the mesh
        /// </summary>
        public void GenerateMeshImmediate(ChunkProperties chunkProperties)
        {
            JobHandleWithData<IMesherJob> jobHandleWithData = VoxelWorld.VoxelMesher.CreateMesh(VoxelWorld.VoxelDataStore, VoxelWorld.VoxelColorStore, chunkProperties.ChunkCoordinate);
            if (jobHandleWithData == null) { return; }

            IMesherJob job = jobHandleWithData.JobData;

            Mesh mesh = new Mesh();
            SubMeshDescriptor subMesh = new SubMeshDescriptor(0, 0);

            jobHandleWithData.JobHandle.Complete();

            int vertexCount = job.VertexCountCounter.Count * 3;
            job.VertexCountCounter.Dispose();

            mesh.SetVertexBufferParams(vertexCount, MeshingVertexData.VertexBufferMemoryLayout);
            mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt16);

            mesh.SetVertexBufferData(job.OutputVertices, 0, 0, vertexCount, 0, MeshUpdateFlags.DontValidateIndices);
            mesh.SetIndexBufferData(job.OutputTriangles, 0, 0, vertexCount, MeshUpdateFlags.DontValidateIndices);

            job.OutputVertices.Dispose();
            job.OutputTriangles.Dispose();

            mesh.subMeshCount = 1;
            subMesh.indexCount = vertexCount;
            mesh.SetSubMesh(0, subMesh);

            mesh.RecalculateBounds();

            chunkProperties.MeshFilter.sharedMesh = mesh;
            chunkProperties.MeshCollider.sharedMesh = mesh;

            chunkProperties.MeshCollider.enabled = true;
            chunkProperties.MeshRenderer.enabled = true;

            chunkProperties.HasChanges = false;

            chunkProperties.IsMeshGenerated = true;
        }
    }
}
