using UnityEngine;

namespace MarchingCubes.Examples
{
    public class Chunk : MonoBehaviour
    {
        [HideInInspector] public bool readyForUpdate;
        [HideInInspector] public Point[,,] points;
        [HideInInspector] public int chunkSize;
        [HideInInspector] public Vector3Int position;

        private float _isolevel;
        private int _seed;

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

            points = new Point[chunkSize + 1, chunkSize + 1, chunkSize + 1];

            _seed = world.seed;
            _marchingCubes = new MarchingCubes(points, _isolevel, _seed);

            for (int x = 0; x < points.GetLength(0); x++)
            {
                for (int y = 0; y < points.GetLength(1); y++)
                {
                    for (int z = 0; z < points.GetLength(2); z++)
                    {
                        points[x, y, z] = new Point(
                            new Vector3Int(x, y, z),
                            _densityGenerator.CalculateDensity(x + worldPosX, y + worldPosY, z + worldPosZ)
                        );
                    }
                }
            }
        }

        public void Generate()
        {
            Mesh mesh = _marchingCubes.CreateMeshData(points);

            _meshFilter.sharedMesh = mesh;
            _meshCollider.sharedMesh = mesh;
        }

        public Point GetPoint(int x, int y, int z)
        {
            return points[x, y, z];
        }

        public void SetDensity(float density, int x, int y, int z)
        {
            points[x, y, z].density = density;
        }

        public void SetDensity(float density, Vector3Int pos)
        {
            SetDensity(density, pos.x, pos.y, pos.z);
        }
    }
}