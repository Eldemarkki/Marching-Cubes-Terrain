using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public abstract class Chunk : MonoBehaviour
    {
        private float _isolevel;

        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private Mesh _mesh;

        private NativeArray<float> _densities;
        private NativeArray<Vector3> _outputVertices;
        private NativeArray<int> _outputTriangles;

        // Stores the density modifications because the densities can not be modified while a job that requires them is running.
        private List<(int index, float density)> _densityModifications;

        // An incremental counter that keeps track of a single integer inside the Marching Cubes job. This is because the jobs
        // can not modify a shared integer because of race conditions.
        private Counter _counter;

        private bool _creatingMesh;

        public int3 Coordinate { get; set; }
        public int ChunkSize { get; private set; }
        public NativeArray<float> Densities { get => _densities; private set => _densities = value; }

        public IDensityCalculationJob DensityCalculationJob { get; set; }
        public JobHandle DensityJobHandle { get; set; }

        public MarchingCubesJob MarchingCubesJob { get; set; }
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

        private void Dispose()
        {
            MarchingCubesJobHandle.Complete();
            _densities.Dispose();
            _outputVertices.Dispose();
            _outputTriangles.Dispose();
        }

        public virtual void Initialize(int chunkSize, float isolevel, int3 coordinate)
        {
            _isolevel = isolevel;
            Coordinate = coordinate;
            ChunkSize = chunkSize;

            Densities = new NativeArray<float>((ChunkSize + 1) * (ChunkSize + 1) * (ChunkSize + 1), Allocator.Persistent);
            _outputVertices = new NativeArray<Vector3>(15 * ChunkSize * ChunkSize * ChunkSize, Allocator.Persistent);
            _outputTriangles = new NativeArray<int>(15 * ChunkSize * ChunkSize * ChunkSize, Allocator.Persistent);

            StartDensityCalculation();
            StartMeshGeneration();
        }

        public abstract void StartDensityCalculation();

        public void StartMeshGeneration()
        {
            _counter = new Counter(Allocator.Persistent);

            for (int i = 0; i < _densityModifications.Count; i++)
            {
                var modification = _densityModifications[i];
                _densities[modification.index] = modification.density;
            }

            _densityModifications.Clear();

            MarchingCubesJob = new MarchingCubesJob
            {
                densities = _densities,
                isolevel = _isolevel,
                chunkSize = ChunkSize,
                counter = _counter,

                vertices = _outputVertices,
                triangles = _outputTriangles
            };

            MarchingCubesJobHandle = MarchingCubesJob.Schedule(ChunkSize * ChunkSize * ChunkSize, 128, DensityJobHandle);

            _creatingMesh = true;
        }

        private void CompleteMeshGeneration()
        {
            MarchingCubesJobHandle.Complete();

            Vector3[] vertices = _outputVertices.Slice(0, _counter.Count * 3).ToArray();
            int[] triangles = _outputTriangles.Slice(0, _counter.Count * 3).ToArray();

            _counter.Dispose();

            _mesh.Clear();
            _mesh.vertices = vertices;
            _mesh.SetTriangles(triangles, 0);
            _mesh.RecalculateNormals();

            _meshFilter.sharedMesh = _mesh;
            _meshCollider.sharedMesh = _mesh;

            _creatingMesh = false;
        }

        public float GetDensity(int x, int y, int z)
        {
            return Densities[x * (ChunkSize + 1) * (ChunkSize + 1) + y * (ChunkSize + 1) + z];
        }

        public float GetDensity(int3 localPosition)
        {
            return GetDensity(localPosition.x, localPosition.y, localPosition.z);
        }

        public void SetDensity(float density, int x, int y, int z)
        {
            _densityModifications.Add((x * (ChunkSize + 1) * (ChunkSize + 1) + y * (ChunkSize + 1) + z, density));
        }
    }
}