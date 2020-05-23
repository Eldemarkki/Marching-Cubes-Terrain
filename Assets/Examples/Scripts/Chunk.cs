using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using MarchingCubes.Examples.Utilities;
using UnityEngine.Rendering;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// The base class for all chunks
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public abstract class Chunk : MonoBehaviour, IDisposable
    {
        public static VertexAttributeDescriptor[] VertexBufferMemoryLayout = new VertexAttributeDescriptor[]
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
        /// This chunk's mesh renderer
        /// </summary>
        protected MeshRenderer _meshRenderer;

        /// <summary>
        /// The chunk's MeshCollider
        /// </summary>
        private MeshCollider _meshCollider;

        /// <summary>
        /// The chunk's cached Mesh so a new one doesn't always have to be created
        /// </summary>
        private Mesh _mesh;

        /// <summary>
        /// The chunk's density field
        /// </summary>
        private DensityStorage _densityStorage;

        /// <summary>
        /// The vertices from the mesh generation job
        /// </summary>
        private NativeArray<MarchingCubesVertexData> _outputVertices;

        /// <summary>
        /// The triangles from the mesh generation job
        /// </summary>
        private NativeArray<ushort> _outputTriangles;

        /// <summary>
        /// Stores the density modifications because the densities can not be modified while a job that requires them is running.
        /// </summary>
        protected List<(int index, float density)> _densityModifications;

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
        /// The chunk's coordinate
        /// </summary>
        public int3 Coordinate { get; set; }

        /// <summary>
        /// The chunk's size. This represents the width, height and depth in Unity units.
        /// </summary>
        public int ChunkSize { get; private set; }

        /// <summary>
        /// The chunk's density field
        /// </summary>
        public DensityStorage DensityStorage
        {
            get => _densityStorage;
        }

        /// <summary>
        /// Density Job Calculation job's handle
        /// </summary>
        public JobHandle DensityJobHandle { get; set; }

        /// <summary>
        /// Mesh generation job's handle
        /// </summary>
        public JobHandle MarchingCubesJobHandle { get; set; }

        protected virtual void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshRenderer = GetComponent<MeshRenderer>();
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = new Mesh();
            _densityModifications = new List<(int index, float density)>();
            _subMesh = new SubMeshDescriptor(0, 0, MeshTopology.Triangles);
        }

        protected virtual void Update()
        {
            if (_creatingMesh)
            {
                CompleteMeshGeneration();
            }

            if (_densityModifications.Count >= 1)
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

            if (_densityStorage.IsCreated) { _densityStorage.Dispose(); }
            if (_outputVertices.IsCreated) { _outputVertices.Dispose(); }
            if (_outputTriangles.IsCreated) { _outputTriangles.Dispose(); }
        }

        /// <summary>
        /// Initializes the chunk and starts generating the mesh.
        /// </summary>
        /// <param name="coordinate">The chunk's coordinate</param>
        /// <param name="chunkGenerationParams">The parameters about how this chunk should be generated</param>
        public void Initialize(int3 coordinate, ChunkGenerationParams chunkGenerationParams)
        {
            transform.position = coordinate.ToVectorInt() * chunkGenerationParams.ChunkSize;
            name = $"Chunk_{coordinate.x}_{coordinate.y}_{coordinate.z}";

            _isolevel = chunkGenerationParams.Isolevel;
            Coordinate = coordinate;
            ChunkSize = chunkGenerationParams.ChunkSize;

            _densityStorage = new DensityStorage(ChunkSize + 1);

            StartDensityCalculation();
            StartMeshGeneration();
        }

        /// <summary>
        /// Starts the density calculation job
        /// </summary>
        public abstract void StartDensityCalculation();

        /// <summary>
        /// Starts the mesh generation job
        /// </summary>
        public void StartMeshGeneration()
        {
            for (int i = 0; i < _densityModifications.Count; i++)
            {
                var modification = _densityModifications[i];
                _densityStorage.SetDensity(modification.density, modification.index);
            }

            _densityModifications.Clear();

            _counter = new Counter(Allocator.TempJob);
            _outputVertices = new NativeArray<MarchingCubesVertexData>(15 * ChunkSize * ChunkSize * ChunkSize, Allocator.TempJob);
            _outputTriangles = new NativeArray<ushort>(15 * ChunkSize * ChunkSize * ChunkSize, Allocator.TempJob);

            var marchingCubesJob = new MarchingCubesJob
            {
                densityStorage = _densityStorage,
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

            _mesh.subMeshCount = 1;
            _subMesh.indexCount = vertexCount;
            _mesh.SetSubMesh(0, _subMesh);

            _mesh.RecalculateBounds();

            _meshFilter.sharedMesh = _mesh;
            _meshRenderer.enabled = true;

            _meshCollider.sharedMesh = _mesh;

            _creatingMesh = false;
        }

        /// <summary>
        /// Gets the density at a local-space position
        /// </summary>
        /// <param name="x">The density's x position inside the chunk (valid values: 0 to chunkSize+1)</param>
        /// <param name="y">The density's y position inside the chunk (valid values: 0 to chunkSize+1)</param>
        /// <param name="z">The density's z position inside the chunk (valid values: 0 to chunkSize+1)</param>
        /// <returns>The density at that local-space position</returns>
        public float GetDensity(int x, int y, int z)
        {
            return _densityStorage.GetDensity(x, y, z);
        }

        /// <summary>
        /// Gets the density at a local space position
        /// </summary>
        /// <param name="localPosition">The density's position inside the chunk</param>
        /// <returns>The density at that local-space position</returns>
        public float GetDensity(int3 localPosition)
        {
            return GetDensity(localPosition.x, localPosition.y, localPosition.z);
        }

        /// <summary>
        /// Sets the density at a local-space position
        /// </summary>
        /// <param name="density">The new density value</param>
        /// <param name="x">The density's x position inside the chunk (valid values: 0 to chunkSize+1)</param>
        /// <param name="y">The density's y position inside the chunk (valid values: 0 to chunkSize+1)</param>
        /// <param name="z">The density's z position inside the chunk (valid values: 0 to chunkSize+1)</param>
        public void SetDensity(float density, int x, int y, int z)
        {
            _densityModifications.Add((x * (ChunkSize + 1) * (ChunkSize + 1) + y * (ChunkSize + 1) + z, density));
        }

        /// <summary>
        /// Sets the density at a local-space position
        /// </summary>
        /// <param name="density">The new density value</param>
        /// <param name="localPos">The density's position inside the chunk</param>
        public void SetDensity(float density, int3 localPos)
        {
            SetDensity(density, localPos.x, localPos.y, localPos.z);
        }

        /// <summary>
        /// Exports this chunk to a .obj file
        /// </summary>
        [ContextMenu("Export selected chunk to .obj")]
        public void ExportToObjFile()
        {
            ObjExporter.Export(gameObject);
        }
    }
}