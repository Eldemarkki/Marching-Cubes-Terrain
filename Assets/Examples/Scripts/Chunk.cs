using UnityEngine;

namespace MarchingCubes.Examples
{
    public class Chunk : MonoBehaviour
    {
        [HideInInspector] public bool readyForUpdate;
        [HideInInspector] public DensityField densityField;
        [HideInInspector] public int chunkSize;
        [HideInInspector] public Vector3Int position;

        private float _isolevel;

        private MarchingCubes _marchingCubes;
        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;
        private DensityGenerator _densityGenerator;

        private void Awake(){
            _meshFilter = GetComponent<MeshFilter>();
            _meshCollider = GetComponent<MeshCollider>();
        }

        private void Start()
        {
            Generate();
        }

        private void Update()
        {
            if (readyForUpdate)
            {
                Generate();
                readyForUpdate = false;
            }
        }

        public void Initialize(World world, int chunkSize, Vector3Int position)
        {
            this.chunkSize = chunkSize;
            this.position = position;
            _isolevel = world.isolevel;

            _densityGenerator = world.densityGenerator;

            int worldPosX = position.x;
            int worldPosY = position.y;
            int worldPosZ = position.z;

            densityField = new DensityField(chunkSize + 1, chunkSize + 1, chunkSize + 1);
            densityField.Populate(_densityGenerator.CalculateDensity, position);

            _marchingCubes = new MarchingCubes(densityField, _isolevel);
        }

        public void Generate()
        {
            Mesh mesh = _marchingCubes.CreateMeshData(densityField);

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