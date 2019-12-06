using System;
using MarchingCubes.Examples.DensityFunctions;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MarchingCubes.Examples
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Chunk : MonoBehaviour
    {
        private Vector3Int _coordinate;
        private bool _isDirty;
        private float _isolevel;
        private int _chunkSize;

        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private Mesh _mesh;

        private ValueGrid<float> _densityField;

        private Func<MeshData> _meshDataDelegate;
        private World _world;

        private NativeArray<float> densities;
        private JobHandle densityJobHandle;
        private DensityCalculationJob densityCalculationJob;
        private bool densitiesChanged;

        public Vector3Int Coordinate => _coordinate;

        private void Awake()
        {
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = new Mesh();
        }

        private void OnDestroy()
        {
            densities.Dispose();
        }

        private void Update()
        {
            if (!_isDirty) { return; }
            Generate();
        }

        public void Initialize(World world, int chunkSize, float isolevel, Vector3Int coordinate)
        {
            _world = world;
            _isolevel = isolevel;
            _chunkSize = chunkSize;

            _densityField = new ValueGrid<float>(chunkSize + 1, chunkSize + 1, chunkSize + 1);
            densities = new NativeArray<float>((_chunkSize + 1) * (_chunkSize + 1) * (_chunkSize + 1), Allocator.Persistent);

            _meshDataDelegate = () => MarchingCubes.CreateMeshData(_densityField, isolevel);

            SetCoordinate(coordinate);
        }

        public void SetCoordinate(Vector3Int coordinate)
        {
            _coordinate = coordinate;
            transform.position = coordinate * _chunkSize;
            name = $"Chunk_{coordinate.x.ToString()}_{coordinate.y.ToString()}_{coordinate.z.ToString()}";

            PopulateDensities();

            _isDirty = true;
        }

        private void PopulateDensities()
        {
            Vector3Int offset = _coordinate * _chunkSize;

            if (_world.UseJobSystem)
            {
                densityCalculationJob = new DensityCalculationJob
                {
                    densities = densities,
                    xOffset = offset.x,
                    yOffset = offset.y,
                    zOffset = offset.z,
                    chunkSize = _chunkSize + 1, // +1 because chunkSize is the amount of "voxels", and that +1 is the amount of density points
                    terrainSettings = _world.TerrainSettings,
                };

                densityJobHandle = densityCalculationJob.Schedule(densities.Length, 256);

                densitiesChanged = true;
            }
            else
            {
                _densityField.Populate(_world.DensityFunction.CalculateDensity, offset.x, offset.y, offset.z);
            }
        }

        public void Generate()
        {
            MeshData meshData;

            if (_world.UseJobSystem && densitiesChanged)
            {
                densityJobHandle.Complete();
                densities.CopyTo(_densityField.data);
                densitiesChanged = false;
            }

            meshData = MarchingCubes.CreateMeshData(_densityField, _isolevel);

            var vertices = meshData.Vertices;
            var triangles = meshData.Triangles;

            _mesh.Clear();
            _mesh.SetVertices(vertices);
            _mesh.SetTriangles(triangles, 0);
            _mesh.RecalculateNormals();

            _meshFilter.sharedMesh = _mesh;
            _meshCollider.sharedMesh = _mesh;

            _isDirty = false;
        }

        public float GetDensity(int x, int y, int z)
        {
            return _densityField.Get(x, y, z);
        }

        public void SetDensity(float density, int x, int y, int z)
        {
            _densityField.Set(x, y, z, density);
            _isDirty = true;
        }
    }
}