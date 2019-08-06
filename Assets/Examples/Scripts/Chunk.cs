using System;
using System.Threading.Tasks;
using UnityEngine;

namespace MarchingCubes.Examples
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Chunk : MonoBehaviour
    {
        [HideInInspector] public bool isDirty;
        [HideInInspector] private ValueGrid<float> densityField;
        [HideInInspector] public Vector3Int position;
        [HideInInspector] private int chunkSize;

        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private Mesh mesh;

        private Func<MeshData> meshDataDelegate;
        private World world;
        
        private void Awake(){
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
            mesh = new Mesh();
        }

        private void Update()
        {
            if (isDirty)
            {
                Generate();
                isDirty = false;
            }
        }

        public void Initialize(World world, int chunkSize, Vector3Int position)
        {
            this.world = world;
            this.chunkSize = chunkSize;
            
            densityField = new ValueGrid<float>(chunkSize + 1, chunkSize + 1, chunkSize + 1);
            meshDataDelegate = () => MarchingCubes.CreateMeshData(densityField, world.isolevel);
            
            SetPosition(position);
        }

        public void SetPosition(Vector3Int position)
        {
            this.position = position;
            name = GenerateChunkName(position);

            PopulateDensities();
            
            isDirty = true;
        }

        private void PopulateDensities()
        {
            Task populateTask = Task.Factory.StartNew(() => densityField.Populate(world.densityFunction.CalculateDensity, position.x, position.y, position.z));
            populateTask.Wait();
        }

        public string GenerateChunkName(Vector3Int position)
        {
            return $"Chunk_{position.x.ToString()}_{position.y.ToString()}_{position.z.ToString()}";
        }

        public void Generate()
        {
            var meshTask = new Task<MeshData>(meshDataDelegate);

            meshTask.Start();
            meshTask.Wait();

            var (vertices, triangles) = meshTask.Result;
            
            mesh.Clear();
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.RecalculateNormals();
            
            _meshFilter.sharedMesh = mesh;
            _meshCollider.sharedMesh = mesh;
        }

        public float GetDensity(int x, int y, int z)
        {
            return densityField[x, y, z];
        }

        public void SetDensity(float density, int x, int y, int z)
        {
            densityField[x, y, z] = density;
        }

        public void SetDensity(float density, Vector3Int pos)
        {
            SetDensity(density, pos.x, pos.y, pos.z);
        }
    }
}