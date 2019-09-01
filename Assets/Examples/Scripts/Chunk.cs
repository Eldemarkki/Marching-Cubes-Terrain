using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MarchingCubes.Examples
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Chunk : MonoBehaviour
    {
        private Vector3Int _position;
        private bool _isDirty;
        private float _isolevel;

        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private Mesh _mesh;
        
        private ValueGrid<float> _densityField;

        private Func<MeshData> _meshDataDelegate;
        private World _world;
        
        private void Awake(){
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            _mesh = new Mesh();
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

            _densityField = new ValueGrid<float>(chunkSize + 1, chunkSize + 1, chunkSize + 1);
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
                var populateTask = Task.Factory.StartNew(() => _densityField.Populate(_world.DensityFunction.CalculateDensity, _position.x, _position.y, _position.z));
                populateTask.Wait();
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