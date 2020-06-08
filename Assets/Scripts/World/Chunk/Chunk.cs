using Eldemarkki.VoxelTerrain.MarchingCubes;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.Density;
using Unity.Collections;
using Unity.Jobs;
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
        /// The layout of one vertex in memory
        /// </summary>
        public static readonly VertexAttributeDescriptor[] VertexBufferMemoryLayout =
        {
            new VertexAttributeDescriptor(VertexAttribute.Position, VertexAttributeFormat.Float32, 3),
            new VertexAttributeDescriptor(VertexAttribute.Normal, VertexAttributeFormat.Float32, 3)
        };

        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        private float _isolevel;

        /// <summary>
        /// The chunk's MeshFilter
        /// </summary>
        private MeshFilter _meshFilter;

        /// <summary>
        /// The chunk's MeshCollider
        /// </summary>
        private MeshCollider _meshCollider;

        /// <summary>
        /// The chunk's cached Mesh so a new one doesn't always have to be created
        /// </summary>
        private Mesh _mesh;

        /// <summary>
        /// The submesh for this chunk
        /// </summary>
        private SubMeshDescriptor _subMesh;

        /// <summary>
        /// The voxel density store where the densities will be gotten from
        /// </summary>
        private VoxelDensityStore _voxelDensityStore;

        /// <summary>
        /// The chunk's coordinate
        /// </summary>
        public int3 Coordinate { get; set; }

        /// <summary>
        /// The chunk's size. This represents the width, height and depth in Unity units.
        /// </summary>
        public int ChunkSize { get; private set; }

        /// <summary>
        /// Have the densities of this chunk been changed during the last frame
        /// </summary>
        public bool HasChanges { get; set; }

        protected virtual void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = new Mesh();
            _subMesh = new SubMeshDescriptor(0, 0, MeshTopology.Triangles);
        }

        protected virtual void Update()
        {
            if (HasChanges)
            {
                StartMeshGeneration();
            }
        }

        /// <summary>
        /// Initializes the chunk and starts generating the mesh.
        /// </summary>
        /// <param name="coordinate">The chunk's coordinate</param>
        /// <param name="chunkGenerationParams">The parameters about how this chunk should be generated</param>
        /// <param name="voxelDensityStore">The voxel density store from where the densities should be gotten</param>
        public void Initialize(int3 coordinate, ChunkGenerationParams chunkGenerationParams, VoxelDensityStore voxelDensityStore)
        {
            transform.position = coordinate.ToVectorInt() * chunkGenerationParams.ChunkSize;
            name = GetName(coordinate);

            _isolevel = chunkGenerationParams.Isolevel;
            Coordinate = coordinate;
            ChunkSize = chunkGenerationParams.ChunkSize;

            _voxelDensityStore = voxelDensityStore;

            StartMeshGeneration();
        }

        /// <summary>
        /// Forces the regeneration of the mesh
        /// </summary>
        public void StartMeshGeneration()
        {
            Counter counter = new Counter(Allocator.TempJob);
            var outputVertices = new NativeArray<MarchingCubesVertexData>(15 * ChunkSize * ChunkSize * ChunkSize, Allocator.TempJob);
            var outputTriangles = new NativeArray<ushort>(15 * ChunkSize * ChunkSize * ChunkSize, Allocator.TempJob);

            var densities = _voxelDensityStore.GetDensityChunk(Coordinate);

            var marchingCubesJob = new MarchingCubesJob
            {
                densityVolume = densities,
                isolevel = _isolevel,
                chunkSize = ChunkSize,
                counter = counter,

                vertices = outputVertices,
                triangles = outputTriangles
            };

            JobHandle jobHandle = marchingCubesJob.Schedule(ChunkSize * ChunkSize * ChunkSize, 128);

            _mesh.Clear();

            jobHandle.Complete();

            int vertexCount = counter.Count * 3;
            counter.Dispose();

            _mesh.SetVertexBufferParams(vertexCount, VertexBufferMemoryLayout);
            _mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt16);

            _mesh.SetVertexBufferData(outputVertices, 0, 0, vertexCount, 0, MeshUpdateFlags.DontValidateIndices);
            _mesh.SetIndexBufferData(outputTriangles, 0, 0, vertexCount, MeshUpdateFlags.DontValidateIndices);

            outputVertices.Dispose();
            outputTriangles.Dispose();

            _mesh.subMeshCount = 1;
            _subMesh.indexCount = vertexCount;
            _mesh.SetSubMesh(0, _subMesh);

            _mesh.RecalculateBounds();

            _meshFilter.sharedMesh = _mesh;
            _meshCollider.sharedMesh = _mesh;

            HasChanges = false;
        }

        /// <summary>
        /// Exports this chunk to a .obj file
        /// </summary>
        [ContextMenu("Export selected chunk to .obj")]
        public void ExportToObjFile()
        {
            ObjExporter.Export(gameObject);
        }

        /// <summary>
        /// Generates a chunk name from a chunk coordinate
        /// </summary>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public static string GetName(int3 coordinate)
        {
            return $"Chunk_{coordinate.x}_{coordinate.y}_{coordinate.z}";
        }
    }
}