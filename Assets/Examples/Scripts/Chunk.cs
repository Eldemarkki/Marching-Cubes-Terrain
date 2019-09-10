using System;
using System.Threading.Tasks;
using MarchingCubes.Examples.DensityFunctions;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace MarchingCubes.Examples
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Chunk : MonoBehaviour
    {
        private Vector3Int _position;
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

        public void Initialize(World world, int chunkSize, float isolevel, Vector3Int position)
        {
            _world = world;
            _isolevel = isolevel;
            _chunkSize = chunkSize;

            _densityField = new ValueGrid<float>(chunkSize + 1, chunkSize + 1, chunkSize + 1);
            densities = new NativeArray<float>((_chunkSize + 1) * (_chunkSize + 1) * (_chunkSize + 1), Allocator.Persistent);

            _meshDataDelegate = () => MarchingCubes.CreateMeshData(_densityField, isolevel);

            SetPosition(position);
        }

        public void SetPosition(Vector3Int position)
        {
            _position = position;
            name = $"Chunk_{position.x.ToString()}_{position.y.ToString()}_{position.z.ToString()}";

            PopulateDensities();

            _isDirty = true;
        }

        private void PopulateDensities()
        {
            if (_world.UseThreading)
            {
                densityCalculationJob = new DensityCalculationJob
                {
                    densities = densities,
                    offsetX = _position.x,
                    offsetY = _position.y,
                    offsetZ = _position.z,
                    chunkSize = _chunkSize + 1, // +1 because chunkSize is the amount of "voxels", and that +1 is the amount of density points
                };

                densityJobHandle = densityCalculationJob.Schedule(densities.Length, 256);

                densitiesChanged = true;
            }
            else
            {
                _densityField.Populate(_world.DensityFunction.CalculateDensity, _position.x, _position.y, _position.z);
            }
        }

        public void Generate()
        {
            MeshData meshData;

            if (_world.UseThreading)
            {
                if (densitiesChanged)
                {
                    densityJobHandle.Complete();
                    densities.CopyTo(_densityField.data);
                    densitiesChanged = false;
                }

                Task<MeshData> meshTask = Task.Factory.StartNew(_meshDataDelegate);

                meshTask.Wait();

                meshData = meshTask.Result;
            }
            else
            {
                meshData = MarchingCubes.CreateMeshData(_densityField, _isolevel);
            }

            var (vertices, triangles) = meshData;

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
            return _densityField[x, y, z];
        }

        private void SetDensity(float density, int x, int y, int z)
        {
            _densityField[x, y, z] = density;
            _isDirty = true;
        }

        public void SetDensity(float density, Vector3Int pos)
        {
            SetDensity(density, pos.x, pos.y, pos.z);
        }
    }
}