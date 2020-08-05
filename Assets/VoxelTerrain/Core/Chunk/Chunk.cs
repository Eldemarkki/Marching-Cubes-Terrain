using Eldemarkki.VoxelTerrain.Meshing;
using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.VoxelData;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    /// <summary>
    /// A component used for visualizing a chunk of the world 
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        /// <summary>
        /// The world that "owns" this chunk
        /// </summary>
        private VoxelWorld _voxelWorld;

        private MeshFilter _meshFilter;

        private MeshCollider _meshCollider;

        public int3 ChunkCoordinate { get; set; }

        /// <summary>
        /// Has the voxel data of this chunk been changed during the last frame
        /// </summary>
        public bool HasChanges { get; set; }

        public bool IsMeshGenerated { get; set; }

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
        }

        private void Update()
        {
            if (HasChanges)
            {
                GenerateMesh();
            }
        }

        /// <summary>
        /// Initializes the chunk's properties.
        /// </summary>
        /// <param name="coordinate">The coordinate of this chunk</param>
        /// <param name="voxelWorld">The world that "owns" this chunk</param>
        public void Initialize(int3 coordinate, VoxelWorld voxelWorld)
        {
            _voxelWorld = voxelWorld;

            transform.position = coordinate.ToVectorInt() * voxelWorld.WorldSettings.ChunkSize;
            name = GetName(coordinate);

            ChunkCoordinate = coordinate;
        }

        /// <summary>
        /// Forces the regeneration of the mesh
        /// </summary>
        public void GenerateMesh()
        {
            JobHandleWithData<IMesherJob> jobHandleWithData = _voxelWorld.VoxelMesher.CreateMesh(_voxelWorld.VoxelDataStore, ChunkCoordinate);
            if(jobHandleWithData == null) { return; }

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

            _meshFilter.sharedMesh = mesh;
            _meshCollider.sharedMesh = mesh;

            HasChanges = false;

            IsMeshGenerated = true;
        }

        /// <summary>
        /// Generates the voxel data for this chunk and generates the mesh
        /// </summary>
        public void GenerateVoxelDataAndMesh()
        {
            Bounds chunkBounds = BoundsUtilities.GetChunkBounds(ChunkCoordinate, _voxelWorld.WorldSettings.ChunkSize);
            JobHandleWithData<IVoxelDataGenerationJob> jobHandleWithData = _voxelWorld.VoxelDataGenerator.GenerateVoxelData(chunkBounds);
            _voxelWorld.VoxelDataStore.SetVoxelDataJobHandle(jobHandleWithData, ChunkCoordinate);

            GenerateMesh();
        }

        /// <summary>
        /// Generates a chunk name from a chunk coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk</param>
        /// <returns>The name of the chunk</returns>
        public static string GetName(int3 chunkCoordinate)
        {
            return $"Chunk_{chunkCoordinate.x.ToString()}_{chunkCoordinate.y.ToString()}_{chunkCoordinate.z.ToString()}";
        }
    }
}