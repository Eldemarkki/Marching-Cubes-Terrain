using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using MarchingCubes.Examples.Utilities;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// The base class for all chunks
    /// </summary>
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public abstract class Chunk : MonoBehaviour
    {
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
        /// The chunk's density field
        /// </summary>
        private NativeArray<float> _densities;

        /// <summary>
        /// The vertices from the mesh generation job
        /// </summary>
        private NativeArray<Vector3> _outputVertices;

        /// <summary>
        /// The triangles from the mesh generation job
        /// </summary>
        private NativeArray<int> _outputTriangles;

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
        public NativeArray<float> Densities
        {
            get => _densities;
            private set => _densities = value;
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
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = new Mesh();
            _densityModifications = new List<(int index, float density)>();
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
        private void Dispose()
        {
            MarchingCubesJobHandle.Complete();
            _densities.Dispose();
            _outputVertices.Dispose();
            _outputTriangles.Dispose();
        }

        /// <summary>
        /// Initializes the chunk and starts generating the mesh.
        /// </summary>
        /// <param name="chunkSize">The chunk's size. This represents the width, height and depth in Unity units.</param>
        /// <param name="isolevel">The density level where a surface will be created. Densities below this will be inside the surface (solid), and densities above this will be outside the surface (air)</param>
        /// <param name="coordinate">The chunk's coordinate</param>
        public void Initialize(int chunkSize, float isolevel, int3 coordinate)
        {
            _isolevel = isolevel;
            Coordinate = coordinate;
            ChunkSize = chunkSize;
            
            transform.position = coordinate.ToVectorInt() * ChunkSize;
            name = $"Chunk_{coordinate.x}_{coordinate.y}_{coordinate.z}";

            Densities = new NativeArray<float>((ChunkSize + 1) * (ChunkSize + 1) * (ChunkSize + 1), Allocator.Persistent);
            _outputVertices = new NativeArray<Vector3>(15 * ChunkSize * ChunkSize * ChunkSize, Allocator.Persistent);
            _outputTriangles = new NativeArray<int>(15 * ChunkSize * ChunkSize * ChunkSize, Allocator.Persistent);

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
            _counter = new Counter(Allocator.Persistent);

            for (int i = 0; i < _densityModifications.Count; i++)
            {
                var modification = _densityModifications[i];
                _densities[modification.index] = modification.density;
            }

            _densityModifications.Clear();

            var marchingCubesJob = new MarchingCubesJob
            {
                densities = _densities,
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

            Vector3[] vertices = new Vector3[_counter.Count * 3];
            int[] triangles = new int[_counter.Count * 3];
            
            if (_counter.Count * 3 > 0)
            {
                _outputVertices.Slice(0, vertices.Length).CopyToFast(vertices);
                _outputTriangles.Slice(0, triangles.Length).CopyToFast(triangles);
            }

            _counter.Dispose();

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.SetTriangles(triangles, 0);
            _mesh.RecalculateNormals();

            _meshFilter.sharedMesh = _mesh;
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
            return Densities[x * (ChunkSize + 1) * (ChunkSize + 1) + y * (ChunkSize + 1) + z];
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