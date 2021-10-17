using Eldemarkki.VoxelTerrain.Meshing;
using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    public class ChunkUpdater : MonoBehaviour
    {
        public VoxelWorld VoxelWorld { get; set; }

        private readonly HashSet<ChunkProperties> chunksNeedingUpdate = new HashSet<ChunkProperties>();

        private void Update()
        {
            if (chunksNeedingUpdate.Count == 0)
            {
                return;
            }

            var chunks = new ChunkProperties[chunksNeedingUpdate.Count];
            int jobIndex = 0;
            foreach (ChunkProperties chunk in chunksNeedingUpdate)
            {
                StartGeneratingMesh(chunk);
                chunks[jobIndex] = chunk;
                jobIndex++;
            }

            JobHandle.ScheduleBatchedJobs();

            chunksNeedingUpdate.Clear();

            while (true)
            {
                bool atleastOneJobRunning = false;
                for (int i = 0; i < chunks.Length; i++)
                {
                    JobHandleWithDataAndChunkProperties<IMesherJob> meshingJobHandle = chunks[i].MeshingJobHandle;
                    if (meshingJobHandle == null)
                    {
                        continue;
                    }

                    bool currentJobRunning = !meshingJobHandle.JobHandle.IsCompleted;
                    atleastOneJobRunning |= currentJobRunning;
                }

                if (!atleastOneJobRunning)
                {
                    break;
                }
            }

            FinalizeMultipleChunkJobs(chunks);
        }

        public void SetChunkDirty(ChunkProperties chunk)
        {
            chunksNeedingUpdate.Add(chunk);
        }

        public void GenerateChunkImmediate(ChunkProperties chunk)
        {
            StartGeneratingChunk(chunk);
            FinalizeChunkJob(chunk);
        }

        public JobHandleWithDataAndChunkProperties<IMesherJob> StartGeneratingMesh(ChunkProperties chunk, JobHandle jobHandle = default)
        {
            JobHandle previousMeshingJobHandle = default;
            if (chunk.MeshingJobHandle != null)
            {
                previousMeshingJobHandle = chunk.MeshingJobHandle.JobHandle;
            }

            JobHandle combinedJobHandle = JobHandle.CombineDependencies(jobHandle, previousMeshingJobHandle);
            JobHandleWithDataAndChunkProperties<IMesherJob> meshingJobHandle = VoxelWorld.VoxelMesher.CreateMesh(
                VoxelWorld.VoxelDataStore,
                VoxelWorld.VoxelColorStore,
                chunk,
                combinedJobHandle);
            chunk.MeshingJobHandle = meshingJobHandle;
            return meshingJobHandle;
        }

        private JobHandle StartGeneratingData(ChunkProperties chunk)
        {
            chunk.MeshCollider.enabled = false;
            chunk.MeshRenderer.enabled = false;

            JobHandle voxelDataHandle = VoxelWorld.VoxelDataStore.GenerateDataForChunk(chunk.ChunkCoordinate);
            JobHandle voxelColorHandle = VoxelWorld.VoxelColorStore.GenerateDataForChunk(chunk.ChunkCoordinate);

            return JobHandle.CombineDependencies(voxelDataHandle, voxelColorHandle);
        }

        public JobHandleWithDataAndChunkProperties<IMesherJob> StartGeneratingChunk(ChunkProperties chunk)
        {
            JobHandle jobHandle = StartGeneratingData(chunk);
            return StartGeneratingMesh(chunk, jobHandle);
        }

        public void FinalizeChunkJob(ChunkProperties chunk)
        {
            FinalizeMultipleChunkJobs(new[] { chunk }, 1);
        }

        public void FinalizeMultipleChunkJobs(List<ChunkProperties> chunks)
        {
            FinalizeMultipleChunkJobs(chunks.ToArray(), chunks.Count);
        }

        public void FinalizeMultipleChunkJobs(ChunkProperties[] chunks)
        {
            FinalizeMultipleChunkJobs(chunks, chunks.Length);
        }

        public void FinalizeMultipleChunkJobs(ChunkProperties[] chunks, int count)
        {
            if (chunks.Length < count)
            {
                Debug.LogWarning("Requested count was higher than available chunk count when finalizing chunk jobs");
                count = chunks.Length;
            }

            Mesh.MeshDataArray meshDataArray = Mesh.AllocateWritableMeshData(count);
            Mesh[] meshes = new Mesh[count];
            MeshUpdateFlags flags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices;
            NativeArray<int> ids = new NativeArray<int>(count, Allocator.TempJob);

            // Copy vertices and triangles
            for (int i = 0; i < count; i++)
            {
                var chunk = chunks[i];
                var chunkJob = chunk.MeshingJobHandle;
                IMesherJob job = chunkJob.JobData;

                Mesh.MeshData meshData = meshDataArray[i];

                chunkJob.JobHandle.Complete();

                meshData.SetVertexBufferParams(job.OutputVertices.Length, MeshingVertexData.VertexBufferMemoryLayout);
                meshData.SetIndexBufferParams(job.OutputTriangles.Length, IndexFormat.UInt16);

                var meshDataVertices = meshData.GetVertexData<MeshingVertexData>();
                var meshDataTriangles = meshData.GetIndexData<ushort>();

                NativeArray<MeshingVertexData>.Copy(job.OutputVertices, meshDataVertices);
                NativeArray<ushort>.Copy(job.OutputTriangles, meshDataTriangles);

                Mesh mesh = new Mesh();
                meshes[i] = mesh;
                ids[i] = mesh.GetInstanceID();

                meshData.subMeshCount = 1;
                meshData.SetSubMesh(0, new SubMeshDescriptor(0, job.OutputVertices.Length), flags);
            }

            // Apply vertices and triangles to the meshes and start generating the collision mesh
            Mesh.ApplyAndDisposeWritableMeshData(meshDataArray, meshes, flags);
            JobHandle bakeMeshJobHandle = new BakeMeshJob(ids).Schedule(count, 1);

            // Apply visible mesh
            for (int i = 0; i < count; i++)
            {
                var chunk = chunks[i];
                var chunkJob = chunk.MeshingJobHandle;
                IMesherJob job = chunkJob.JobData;
                ChunkProperties chunkProperties = chunkJob.ChunkProperties;

                Mesh mesh = meshes[i];

                int3 chunkSize = VoxelWorld.WorldSettings.ChunkSize;
                mesh.bounds = new Bounds(((Vector3)chunkSize.ToVectorInt()) * 0.5f, chunkSize.ToVectorInt());

                job.OutputVertices.Clear();
                job.OutputTriangles.Clear();

                chunkProperties.MeshFilter.sharedMesh = mesh;
                chunkProperties.MeshRenderer.enabled = mesh.vertexCount > 0;
                chunkProperties.IsMeshGenerated = true;
            }

            bakeMeshJobHandle.Complete();
            ids.Dispose();

            // Apply collision mesh
            for (int i = 0; i < count; i++)
            {
                var meshingJobHandle = chunks[i].MeshingJobHandle;
                if (meshingJobHandle == null)
                {
                    continue;
                }

                ChunkProperties chunkProperties = meshingJobHandle.ChunkProperties;
                Mesh mesh = meshes[i];
                chunkProperties.MeshCollider.sharedMesh = mesh;
                chunkProperties.MeshCollider.enabled = mesh.vertexCount > 0;
                chunkProperties.MeshingJobHandle = null;
                VoxelWorld.VoxelDataStore.ApplyChunkChanges(chunkProperties.ChunkCoordinate);
            }
        }

        private struct BakeMeshJob : IJobParallelFor
        {
            private NativeArray<int> ids;
            public BakeMeshJob(NativeArray<int> ids) => this.ids = ids;
            public void Execute(int index) => Physics.BakeMesh(ids[index], false);
        }
    }
}
