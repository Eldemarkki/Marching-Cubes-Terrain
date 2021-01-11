using System.Collections.Generic;
using System.Linq;
using Eldemarkki.VoxelTerrain.Meshing;
using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    public class ChunkUpdater : MonoBehaviour
    {
        private readonly HashSet<ChunkProperties> chunksNeedingUpdate = new HashSet<ChunkProperties>();

        /// <summary>
        /// The world for which to provide chunks for
        /// </summary>
        public VoxelWorld VoxelWorld { get; set; }

        private void Update()
        {
            if (!chunksNeedingUpdate.Any()) return;

            var lst = new List<JobHandleWithDataAndChunkProperties<IMesherJob>>();
            foreach (var itm in chunksNeedingUpdate)
            {
                lst.Add(GenerateMeshDelay(itm));
            }

            chunksNeedingUpdate.Clear();

            JobHandle.ScheduleBatchedJobs();
            while (lst.Any())
            {
                var finishedJob = lst.FirstOrDefault(f => f.JobHandle.IsCompleted);
                if (finishedJob == null) continue;

                lst.Remove(finishedJob);
                this.FinalizeChunkJob(finishedJob);
            }

            //foreach (ChunkProperties chunkProperties in VoxelWorld.ChunkStore.Chunks)
            //{
            //    if (chunkProperties.HasChanges)
            //    {
            //        GenerateMeshImmediate(chunkProperties);
            //    }
            //}
        }

        public void SetChunkDirty(ChunkProperties properties)
        {
            chunksNeedingUpdate.Add(properties);
        }

        public void GenerateMeshImmediate(ChunkProperties chunkProperties)
        {
            var handle = this.GenerateVoxelDataAndMeshDelay(chunkProperties);
            this.FinalizeChunkJob(handle);
        }

        public JobHandleWithDataAndChunkProperties<IMesherJob> GenerateMeshDelay(ChunkProperties chunkProperties)
        {
            var jobHandleWithData = VoxelWorld.VoxelMesher.CreateMesh(VoxelWorld.VoxelDataStore, VoxelWorld.VoxelColorStore, chunkProperties.ChunkCoordinate, chunkProperties);
            return jobHandleWithData;
        }

        /// <summary>
        /// Generates the voxel data and colors for this chunk and generates the mesh
        /// </summary>
        public void GenerateVoxelDataAndMeshImmediate(ChunkProperties chunkProperties)
        {
            VoxelWorld.VoxelDataStore.GenerateDataForChunk(chunkProperties.ChunkCoordinate);
            VoxelWorld.VoxelColorStore.GenerateDataForChunk(chunkProperties.ChunkCoordinate);

            var handle = this.GenerateVoxelDataAndMeshDelay(chunkProperties);
            this.FinalizeChunkJob(handle);
        }

        public JobHandleWithDataAndChunkProperties<IMesherJob> GenerateVoxelDataAndMeshDelay(ChunkProperties chunkProperties)
        {
            VoxelWorld.VoxelDataStore.GenerateDataForChunk(chunkProperties.ChunkCoordinate);
            VoxelWorld.VoxelColorStore.GenerateDataForChunk(chunkProperties.ChunkCoordinate);
            var jobHandleWithData = VoxelWorld.VoxelMesher.CreateMesh(VoxelWorld.VoxelDataStore, VoxelWorld.VoxelColorStore, chunkProperties.ChunkCoordinate, chunkProperties);
            return jobHandleWithData;
        }

        public void FinalizeChunkJob(JobHandleWithDataAndChunkProperties<IMesherJob> jobHandleWithData)
        {
            IMesherJob job = jobHandleWithData.JobData;
            var chunkProperties = jobHandleWithData.ChunkProperties;

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

            //chunkProperties.HasChanges = false;

            chunkProperties.IsMeshGenerated = true;
        }
    }
}
