using Eldemarkki.VoxelTerrain.MarchingCubes;
using Eldemarkki.VoxelTerrain.Utilities;
using System;
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
    public class Chunk : MonoBehaviour, IDisposable
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
        /// The vertices from the mesh generation job
        /// </summary>
        private NativeArray<MarchingCubesVertexData> _outputVertices;

        /// <summary>
        /// The triangles from the mesh generation job
        /// </summary>
        private NativeArray<ushort> _outputTriangles;

        /// <summary>
        /// An incremental counter that keeps track of a single integer inside the mesh generation job. This is because the jobs
        /// can not modify a shared integer because of race conditions.
        /// </summary>
        private Counter _counter;

        /// <summary>
        /// Is the mesh being generated
        /// </summary>
        private bool _creatingMesh;

        /// <summary>
        /// The submesh for this chunk
        /// </summary>
        private SubMeshDescriptor _subMesh;

        /// <summary>
        /// A temporary (lifespan from start to end of mesh generation) <see cref="DensityVolume"/> volume which contains the densities for this chunk
        /// </summary>
        private DensityVolume _densityVolume;

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
        /// Density Job Calculation job's handle
        /// </summary>
        public JobHandle DensityJobHandle { get; set; }

        /// <summary>
        /// Mesh generation job's handle
        /// </summary>
        public JobHandle MarchingCubesJobHandle { get; set; }

        /// <summary>
        /// Have the densities of this chunk been changed during the last frame
        /// </summary>
        public bool HasChanges { get; set; }

        /// <summary>
        /// This chunk's mesh renderer
        /// </summary>
        public MeshRenderer MeshRenderer { get; private set; }

        protected virtual void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            MeshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = new Mesh();
            _subMesh = new SubMeshDescriptor(0, 0, MeshTopology.Triangles);
        }

        protected virtual void Update()
        {
            if (_creatingMesh)
            {
                CompleteMeshGeneration();
            }

            if (HasChanges)
            {
                StartMeshGeneration();
            }
        }

        protected virtual void OnDestroy()
        {
            Dispose();
        }

        /// <summary>
        /// Disposes the NativeArrays that this chunk has.
        /// </summary>
        public void Dispose()
        {
            if (!DensityJobHandle.IsCompleted) { DensityJobHandle.Complete(); }
            if (!MarchingCubesJobHandle.IsCompleted) { MarchingCubesJobHandle.Complete(); }

            if (_outputVertices.IsCreated) { _outputVertices.Dispose(); }
            if (_outputTriangles.IsCreated) { _outputTriangles.Dispose(); }
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
        /// Starts the mesh generation job
        /// </summary>
        public void StartMeshGeneration()
        {
            _counter = new Counter(Allocator.TempJob);
            _outputVertices = new NativeArray<MarchingCubesVertexData>(15 * ChunkSize * ChunkSize * ChunkSize, Allocator.TempJob);
            _outputTriangles = new NativeArray<ushort>(15 * ChunkSize * ChunkSize * ChunkSize, Allocator.TempJob);

            var densities = _voxelDensityStore.GetDensityChunk(Coordinate);
            if (!_densityVolume.IsCreated)
            {
                _densityVolume = new DensityVolume(densities.Width, densities.Height, densities.Depth);
            }

            _densityVolume.CopyFrom(densities);

            var marchingCubesJob = new MarchingCubesJob
            {
                densityVolume = _densityVolume,
                isolevel = _isolevel,
                chunkSize = ChunkSize,
                counter = _counter,

                vertices = _outputVertices,
                triangles = _outputTriangles
            };

            MarchingCubesJobHandle = marchingCubesJob.Schedule(ChunkSize * ChunkSize * ChunkSize, 128, DensityJobHandle);

            _creatingMesh = true;
        }

        /// <summary>
        /// Completes the mesh generation job and updates the MeshFilter's and the MeshCollider's meshes.
        /// </summary>
        private void CompleteMeshGeneration()
        {
            MarchingCubesJobHandle.Complete();

            _mesh.Clear();

            int vertexCount = _counter.Count * 3;
            _counter.Dispose();

            _mesh.SetVertexBufferParams(vertexCount, VertexBufferMemoryLayout);
            _mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt16);

            _mesh.SetVertexBufferData(_outputVertices, 0, 0, vertexCount, 0, MeshUpdateFlags.DontValidateIndices);
            _mesh.SetIndexBufferData(_outputTriangles, 0, 0, vertexCount, MeshUpdateFlags.DontValidateIndices);

            _outputVertices.Dispose();
            _outputTriangles.Dispose();
            _densityVolume.Dispose();

            _mesh.subMeshCount = 1;
            _subMesh.indexCount = vertexCount;
            _mesh.SetSubMesh(0, _subMesh);

            _mesh.RecalculateBounds();

            _meshFilter.sharedMesh = _mesh;
            MeshRenderer.enabled = true;

            _meshCollider.sharedMesh = _mesh;

            _creatingMesh = false;
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