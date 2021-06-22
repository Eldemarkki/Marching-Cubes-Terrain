using System.Collections.Generic;
using Eldemarkki.VoxelTerrain.Meshing;
using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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

        public void GenerateChunkImmediate(ChunkProperties chunk)
        {
            var handle = StartGeneratingChunk(chunk);
            FinalizeChunkJob(handle);
        }

        public JobHandleWithDataAndChunkProperties<IMesherJob> StartGeneratingMesh(ChunkProperties chunk)
        {
            return VoxelWorld.VoxelMesher.CreateMesh(VoxelWorld.VoxelDataStore, VoxelWorld.VoxelColorStore, chunk, chunk.DataGenerationJobHandle);
        }

        private void StartGeneratingData(ChunkProperties chunk)
        {
            JobHandle voxelDataHandle = VoxelWorld.VoxelDataStore.GenerateDataForChunk(chunk.ChunkCoordinate);
            JobHandle voxelColorHandle = VoxelWorld.VoxelColorStore.GenerateDataForChunk(chunk.ChunkCoordinate);

            JobHandle dataHandleDependency = JobHandle.CombineDependencies(voxelDataHandle, voxelColorHandle);
            chunk.DataGenerationJobHandle = dataHandleDependency;
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

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(1);
            Mesh.MeshData meshData = meshDataArray[0];

            chunkJob.JobHandle.Complete();

            meshData.SetVertexBufferParams(job.OutputVertices.Length, MeshingVertexData.VertexBufferMemoryLayout);
            meshData.SetIndexBufferParams(job.OutputTriangles.Length, IndexFormat.UInt16);

            var meshDataVertices = meshData.GetVertexData<MeshingVertexData>();
            var meshDataTriangles = meshData.GetIndexData<ushort>();

            NativeArray<MeshingVertexData>.Copy(job.OutputVertices, meshDataVertices);
            NativeArray<ushort>.Copy(job.OutputTriangles, meshDataTriangles);

            MeshUpdateFlags flags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;

            Mesh mesh = new Mesh();
            meshData.subMeshCount = 1;
            meshData.SetSubMesh(0, new SubMeshDescriptor(0, job.OutputVertices.Length), flags);
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, mesh, flags);

            int3 chunkSize = VoxelWorld.WorldSettings.ChunkSize;
            mesh.bounds = new Bounds(((Vector3)chunkSize.ToVectorInt()) * 0.5f, chunkSize.ToVectorInt());

            job.OutputVertices.Clear();
            job.OutputTriangles.Clear();

            chunkProperties.MeshFilter.sharedMesh = mesh;
            chunkProperties.MeshCollider.sharedMesh = mesh;

            chunkProperties.MeshCollider.enabled = true;
            chunkProperties.MeshRenderer.enabled = true;

            chunkProperties.IsMeshGenerated = true;

            VoxelWorld.VoxelDataStore.ApplyChunkChanges(chunkProperties.ChunkCoordinate);
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
