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

            var jobs = new List<JobHandleWithDataAndChunkProperties<IMesherJob>>(chunksNeedingUpdate.Count);
            foreach (ChunkProperties chunk in chunksNeedingUpdate)
            {
                jobs.Add(StartGeneratingMesh(chunk));
            }

            chunksNeedingUpdate.Clear();

            JobHandle.ScheduleBatchedJobs();
            while (jobs.Count > 0)
            {
                JobHandleWithDataAndChunkProperties<IMesherJob> finishedJob = null;
                int jobIndex;
                for (jobIndex = 0; jobIndex < jobs.Count; jobIndex++)
                {
                    JobHandleWithDataAndChunkProperties<IMesherJob> job = jobs[jobIndex];
                    if (job.JobHandle.IsCompleted)
                    {
                        finishedJob = job;
                        break;
                    }
                }

                if (finishedJob == null)
                {
                    continue;
                }

                jobs.RemoveAt(jobIndex); // remove 'finishedJob'
                FinalizeChunkJob(finishedJob);
            }
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
            return VoxelWorld.VoxelMesher.CreateMesh(VoxelWorld.VoxelDataStore, VoxelWorld.VoxelColorStore, chunk.ChunkCoordinate, chunk);
        }

        /// <summary>
        /// Generates the voxel data and colors for this chunk and generates the mesh
        /// </summary>
        public void GenerateChunkImmediate(ChunkProperties chunk)
        {
            VoxelWorld.VoxelDataStore.GenerateDataForChunk(chunk.ChunkCoordinate);
            VoxelWorld.VoxelColorStore.GenerateDataForChunk(chunk.ChunkCoordinate);

            GenerateMeshImmediate(chunk);
        }

        public JobHandleWithDataAndChunkProperties<IMesherJob> StartGeneratingChunk(ChunkProperties chunk)
        {
            VoxelWorld.VoxelDataStore.GenerateDataForChunk(chunk.ChunkCoordinate);
            VoxelWorld.VoxelColorStore.GenerateDataForChunk(chunk.ChunkCoordinate);
            return VoxelWorld.VoxelMesher.CreateMesh(VoxelWorld.VoxelDataStore, VoxelWorld.VoxelColorStore, chunk.ChunkCoordinate, chunk);
        }

        public void FinalizeChunkJob(JobHandleWithDataAndChunkProperties<IMesherJob> chunkJob)
        {
            IMesherJob job = chunkJob.JobData;
            ChunkProperties chunkProperties = chunkJob.ChunkProperties;

            Mesh mesh = chunkProperties.ChunkMesh;
            SubMeshDescriptor subMesh = new SubMeshDescriptor(0, 0);

            chunkJob.JobHandle.Complete();

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

            chunkProperties.IsMeshGenerated = true;
        }
    }
}
