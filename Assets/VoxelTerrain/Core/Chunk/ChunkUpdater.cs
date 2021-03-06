using System.Collections.Generic;
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
            if (chunksNeedingUpdate.Count == 0)
            {
                return;
            }

            var jobs = new JobHandleWithDataAndChunkProperties<IMesherJob>[chunksNeedingUpdate.Count];
            int jobIndex = 0;
            foreach (ChunkProperties chunk in chunksNeedingUpdate)
            {
                jobs[jobIndex] = StartGeneratingMesh(chunk);
                jobIndex++;
            }

            JobHandle.ScheduleBatchedJobs();

            chunksNeedingUpdate.Clear();

            while (true)
            {
                bool atleastOneJobRunning = false;
                for (int i = 0; i < jobs.Length; i++)
                {
                    bool currentJobRunning = !jobs[i].JobHandle.IsCompleted;
                    atleastOneJobRunning |= currentJobRunning;
                }

                if (!atleastOneJobRunning)
                {
                    break;
                }
            }

            FinalizeMultipleChunkJobs(jobs);
        }

        public void SetChunkDirty(ChunkProperties chunk)
        {
            chunksNeedingUpdate.Add(chunk);
        }

        public void GenerateMeshImmediate(ChunkProperties chunk)
        {
            var handle = StartGeneratingChunk(chunk);
            FinalizeChunkJob(handle);
        }

        public JobHandleWithDataAndChunkProperties<IMesherJob> StartGeneratingMesh(ChunkProperties chunk)
        {
            return VoxelWorld.VoxelMesher.CreateMesh(VoxelWorld.VoxelDataStore, VoxelWorld.VoxelColorStore, chunk);
        }

        private void StartGeneratingData(ChunkProperties chunk)
        {
            VoxelWorld.VoxelDataStore.GenerateDataForChunk(chunk.ChunkCoordinate);
            VoxelWorld.VoxelColorStore.GenerateDataForChunk(chunk.ChunkCoordinate);
        }

        /// <summary>
        /// Generates the voxel data and colors for this chunk and generates the mesh
        /// </summary>
        public void GenerateChunkImmediate(ChunkProperties chunk)
        {
            StartGeneratingData(chunk);
            GenerateMeshImmediate(chunk);
        }

        public JobHandleWithDataAndChunkProperties<IMesherJob> StartGeneratingChunk(ChunkProperties chunk)
        {
            StartGeneratingData(chunk);
            return StartGeneratingMesh(chunk);
        }

        public void FinalizeChunkJob(JobHandleWithDataAndChunkProperties<IMesherJob> chunkJob)
        {
            IMesherJob job = chunkJob.JobData;
            ChunkProperties chunkProperties = chunkJob.ChunkProperties;

            Mesh mesh = chunkProperties.ChunkMesh;
            SubMeshDescriptor subMesh = new SubMeshDescriptor(0, 0);

            chunkJob.JobHandle.Complete();

            int vertexCount = job.VertexCountCounter.Count;
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

            chunkProperties.IsMeshGenerated = true;
        }

        public void FinalizeMultipleChunkJobs(JobHandleWithDataAndChunkProperties<IMesherJob>[] jobs)
        {
            FinalizeMultipleChunkJobs(jobs, jobs.Length);
        }

        public void FinalizeMultipleChunkJobs(JobHandleWithDataAndChunkProperties<IMesherJob>[] jobs, int count)
        {
            for (int i = 0; i < count; i++)
            {
                FinalizeChunkJob(jobs[i]);
            }
        }
    }
}
