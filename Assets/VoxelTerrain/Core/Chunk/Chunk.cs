using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    /// <summary>
    /// The base class for all chunks
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public class Chunk : MonoBehaviour
    {
        /// <summary>
        /// The chunk's MeshFilter
        /// </summary>
        private MeshFilter _meshFilter;

        /// <summary>
        /// The chunk's MeshCollider
        /// </summary>
        private MeshCollider _meshCollider;

        private VoxelWorld _voxelWorld;

        /// <summary>
        /// The chunk's coordinate
        /// </summary>
        public int3 Coordinate { get; set; }

        /// <summary>
        /// Have the densities of this chunk been changed during the last frame
        /// </summary>
        public bool HasChanges { get; set; }

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
        /// Initializes the chunk and starts generating the mesh.
        /// </summary>
        /// <param name="coordinate">The chunk's coordinate</param>
        /// <param name="chunkProvider">The chunk provider for this chunk</param>
        /// <param name="voxelDensityStore">The voxel density store from where the densities should be gotten</param>
        public void Initialize(int3 coordinate, VoxelWorld voxelWorld)
        {
            _voxelWorld = voxelWorld;

            transform.position = coordinate.ToVectorInt() * voxelWorld.WorldSettings.ChunkSize;
            name = GetName(coordinate);

            Coordinate = coordinate;

            GenerateMesh();
        }


        /// <summary>
        /// Forces the regeneration of the mesh
        /// </summary>
        public void GenerateMesh()
        {
            JobHandleWithData<IMesherJob> jobHandleWithData = _voxelWorld.VoxelMesher.CreateMesh(_voxelWorld.VoxelDataStore, Coordinate);

            IMesherJob job = jobHandleWithData.JobData;

            Mesh mesh = new Mesh();
            var subMesh = new SubMeshDescriptor(0, 0, MeshTopology.Triangles);

            jobHandleWithData.JobHandle.Complete();

            int vertexCount = job.VertexCountCounter.Count * 3;
            job.VertexCountCounter.Dispose();

            mesh.SetVertexBufferParams(vertexCount, VoxelMesher.VertexBufferMemoryLayout);
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
        }

        /// <summary>
        /// Generates a chunk name from a chunk coordinate
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public static string GetName(int3 coordinate)
        {
            return $"Chunk_{coordinate.x.ToString()}_{coordinate.y.ToString()}_{coordinate.z.ToString()}";
        }
    }
}