using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Chunk : MonoBehaviour
    {
        private int3 _coordinate;
        private float _isolevel;
        private int _chunkSize;

        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private Mesh _mesh;

        private World _world;

        private NativeArray<float> _densities;

        private JobHandle _densityJobHandle;
        private IDensityCalculationJob _densityCalculationJob;
        private MarchingCubesJob _marchingCubesJob;
        private JobHandle _marchingCubesJobHandle;

        private NativeArray<Vector3> _outputVertices;
        private NativeArray<int> _outputTriangles;

        // Stores the density modifications because the densities can not be modified while a job that requires them is running.
        private List<(int index, float density)> _densityModifications;

        // An incremental counter that keeps track of a single integer inside the Marching Cubes job. This is because the jobs
        // can not modify a shared integer because of race conditions.
        private Counter _counter;

        private bool _areDensitiesDirty => _densityModifications.Count >= 1;

        private bool _creatingMesh;

        public int3 Coordinate => _coordinate;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = new Mesh();
            _densityModifications = new List<(int index, float density)>();
        }

        private void OnDestroy()
        {
            if (!_marchingCubesJobHandle.IsCompleted)
                _marchingCubesJobHandle.Complete();

            _densities.Dispose();
            _outputVertices.Dispose();
            _outputTriangles.Dispose();
        }

        private void Update()
        {
            if (_creatingMesh)
            {
                CompleteMeshGeneration();
            }

            if (_areDensitiesDirty)
            {
                StartMeshGeneration();
            }
        }

        public void Initialize(World world, int chunkSize, float isolevel, int3 coordinate)
        {
            _world = world;
            _isolevel = isolevel;
            _chunkSize = chunkSize;

            _densities = new NativeArray<float>((_chunkSize + 1) * (_chunkSize + 1) * (_chunkSize + 1), Allocator.Persistent);
            _outputVertices = new NativeArray<Vector3>(15 * _chunkSize * _chunkSize * _chunkSize, Allocator.Persistent);
            _outputTriangles = new NativeArray<int>(15 * _chunkSize * _chunkSize * _chunkSize, Allocator.Persistent);

            SetCoordinate(coordinate);
        }

        public void SetCoordinate(int3 coordinate)
        {
            _coordinate = coordinate;
            transform.position = coordinate.ToVectorInt() * _chunkSize;
            name = $"Chunk_{coordinate.x.ToString()}_{coordinate.y.ToString()}_{coordinate.z.ToString()}";

            StartDensityCalculation();
            StartMeshGeneration();
        }

        public void StartDensityCalculation()
        {
            int3 worldPosition = _coordinate * _chunkSize;

            if (_world.TerrainType == TerrainType.Procedural)
            {
                var job = new ProceduralTerrainDensityCalculationJob
                {
                    Densities = _densities,
                    offset = worldPosition,
                    chunkSize = _chunkSize + 1, // +1 because chunkSize is the amount of "voxels", and that +1 is the amount of density points
                    proceduralTerrainSettings = _world.ProceduralTerrainSettings,
                };

                _densityCalculationJob = job;

                _densityJobHandle = IJobParallelForExtensions.Schedule<ProceduralTerrainDensityCalculationJob>(job, _densities.Length, 256);
            }
            else if (_world.TerrainType == TerrainType.Heightmap)
            {
                var job = new HeightmapTerrainDensityCalculationJob
                {
                    Densities = _densities,
                    heightmapData = _world.HeightmapTerrainSettings.HeightmapData,
                    offset = worldPosition,
                    chunkSize = _chunkSize + 1, // +1 because chunkSize is the amount of "voxels", and that +1 is the amount of density points
                    heightmapWidth = _world.HeightmapTerrainSettings.Width,
                    heightmapHeight = _world.HeightmapTerrainSettings.Height,
                    amplitude = _world.HeightmapTerrainSettings.Amplitude,
                    heightOffset = _world.HeightmapTerrainSettings.HeightOffset
                };

                _densityCalculationJob = job;

                _densityJobHandle = IJobParallelForExtensions.Schedule<HeightmapTerrainDensityCalculationJob>(job, _densities.Length, 256);
            }
        }

        private void StartMeshGeneration()
        {
            _counter = new Counter(Allocator.Persistent);

            for (int i = 0; i < _densityModifications.Count; i++)
            {
                var modification = _densityModifications[i];
                _densities[modification.index] = modification.density;
            }

            _densityModifications.Clear();

            _marchingCubesJob = new MarchingCubesJob
            {
                densities = _densities,
                isolevel = _isolevel,
                chunkSize = _chunkSize,
                counter = _counter,

                vertices = _outputVertices,
                triangles = _outputTriangles
            };

            _marchingCubesJobHandle = _marchingCubesJob.Schedule(_chunkSize * _chunkSize * _chunkSize, 128, _densityJobHandle);

            _creatingMesh = true;
        }

        private void CompleteMeshGeneration()
        {
            _marchingCubesJobHandle.Complete();

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
            return _densities[x * (_chunkSize + 1) * (_chunkSize + 1) + y * (_chunkSize + 1) + z];
        }

        public void SetDensity(float density, int x, int y, int z)
        {
            _densityModifications.Add((x * (_chunkSize + 1) * (_chunkSize + 1) + y * (_chunkSize + 1) + z, density));
        }
    }
}