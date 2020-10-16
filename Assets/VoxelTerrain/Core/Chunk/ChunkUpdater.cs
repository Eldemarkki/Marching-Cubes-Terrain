using Eldemarkki.VoxelTerrain.Meshing;
using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.VoxelData;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
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

        protected void Update()
        {
            foreach (ChunkProperties chunkProperties in VoxelWorld.ChunkStore.Chunks)
            {
                if (chunkProperties.HasChanges)
                {
                    GenerateMesh(chunkProperties);
                }
            }
        }

        /// <summary>
        /// Initializes the chunk's properties.
        /// </summary>
        /// <param name="chunkProperties">The chunk to initialize</param>
        /// <param name="chunkCoordinate">The new coordinate of the chunk</param>
        public void Initialize(ChunkProperties chunkProperties, int3 chunkCoordinate)
        {
            chunkProperties.ChunkGameObject.transform.position = chunkCoordinate.ToVectorInt() * VoxelWorld.WorldSettings.ChunkSize;
            chunkProperties.ChunkGameObject.name = ChunkProperties.GetName(chunkCoordinate);
            chunkProperties.ChunkCoordinate = chunkCoordinate;
        }

        /// <summary>
        /// Generates the voxel data and colors for this chunk and generates the mesh
        /// </summary>
        public void GenerateVoxelDataAndMesh(ChunkProperties chunkProperties)
        {
            Bounds chunkBounds = BoundsUtilities.GetChunkBounds(chunkProperties.ChunkCoordinate, VoxelWorld.WorldSettings.ChunkSize);
            JobHandleWithData<IVoxelDataGenerationJob> jobHandleWithData = VoxelWorld.VoxelDataGenerator.GenerateVoxelData(chunkBounds);
            VoxelWorld.VoxelDataStore.SetVoxelDataJobHandle(jobHandleWithData, chunkProperties.ChunkCoordinate);

            NativeArray<Color32> colors = new NativeArray<Color32>((VoxelWorld.WorldSettings.ChunkSize + 1) * (VoxelWorld.WorldSettings.ChunkSize + 1) * (VoxelWorld.WorldSettings.ChunkSize + 1), Allocator.Persistent);

            Color32 defaultColor = new Color32(11, 91, 33, 255);
            NativeArray<Color32> defaultColorArray = new NativeArray<Color32>(new Color32[] { defaultColor }, Allocator.Temp);

            unsafe
            {
                UnsafeUtility.MemCpyReplicate(colors.GetUnsafePtr(), defaultColorArray.GetUnsafeReadOnlyPtr(), sizeof(Color32), colors.Length);
            }

            VoxelWorld.VoxelColorStore.SetVoxelColorsChunk(chunkProperties.ChunkCoordinate, colors);

            GenerateMesh(chunkProperties);
        }

        /// <summary>
        /// Forces the regeneration of the mesh
        /// </summary>
        public void GenerateMesh(ChunkProperties chunkProperties)
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

            chunkProperties.HasChanges = false;

            chunkProperties.IsMeshGenerated = true;
        }
    }
}
