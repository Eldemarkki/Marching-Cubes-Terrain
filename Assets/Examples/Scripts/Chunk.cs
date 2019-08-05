using UnityEngine;

namespace MarchingCubes.Examples
{
    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
    public class Chunk : MonoBehaviour
    {
        [HideInInspector] public bool isDirty;
        [HideInInspector] private ValueGrid<Voxel> voxels;
        [HideInInspector] public Vector3Int position;
        [HideInInspector] private int chunkSize;

        private MeshFilter _meshFilter;
        private MeshCollider _meshCollider;

        private World world;

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
            if (isDirty)
            {
                Generate();
                isDirty = false;
            }
        }

        public void Initialize(World world, int chunkSize, Vector3Int position)
        {
            this.position = position;
            this.world = world;
            this.chunkSize = chunkSize;

            voxels = new ValueGrid<Voxel>(chunkSize, chunkSize, chunkSize);
            InitializeVoxels();
        }

        private void InitializeVoxels()
        {
            int chunkPositionX = position.x;
            int chunkPositionY = position.y;
            int chunkPositionZ = position.z;

            ValueGrid<Vector3Int> corners = new ValueGrid<Vector3Int>(chunkSize+1, chunkSize+1, chunkSize+1);
            corners.Populate((x, y, z) => new Vector3Int(x, y, z));

            ValueGrid<float> densities = new ValueGrid<float>(chunkSize+1, chunkSize+1, chunkSize+1);
            densities.Populate(world.densityFunction.CalculateDensity, chunkPositionX, chunkPositionY, chunkPositionZ);

            int xAxis = corners.Width * corners.Height;
            int yAxis = corners.Width;
            int zAxis = 1;

            int i = 0;
            for (int x = 0; x < chunkSize; x++)
            {
                for (int y = 0; y < chunkSize; y++)
                {
                    for (int z = 0; z < chunkSize; z++)
                    {
                        Voxel voxel = CalculateVoxel(x, y, z, xAxis, yAxis, zAxis, corners, densities);
                        voxels[i++] = voxel;
                    }
                }
            }
        }

        private Voxel CalculateVoxel(int x, int y, int z, int xAxis, int yAxis, int zAxis,
            ValueGrid<Vector3Int> corners, ValueGrid<float> densities)
        {
            int originIndex = corners.GetIndex(x, y, z);

            int index1 = originIndex + LookupTables.CubeCornersX[0] * xAxis + LookupTables.CubeCornersY[0] * yAxis + LookupTables.CubeCornersZ[0] * zAxis;
            int index2 = originIndex + LookupTables.CubeCornersX[1] * xAxis + LookupTables.CubeCornersY[1] * yAxis + LookupTables.CubeCornersZ[1] * zAxis;
            int index3 = originIndex + LookupTables.CubeCornersX[2] * xAxis + LookupTables.CubeCornersY[2] * yAxis + LookupTables.CubeCornersZ[2] * zAxis;
            int index4 = originIndex + LookupTables.CubeCornersX[3] * xAxis + LookupTables.CubeCornersY[3] * yAxis + LookupTables.CubeCornersZ[3] * zAxis;
            int index5 = originIndex + LookupTables.CubeCornersX[4] * xAxis + LookupTables.CubeCornersY[4] * yAxis + LookupTables.CubeCornersZ[4] * zAxis;
            int index6 = originIndex + LookupTables.CubeCornersX[5] * xAxis + LookupTables.CubeCornersY[5] * yAxis + LookupTables.CubeCornersZ[5] * zAxis;
            int index7 = originIndex + LookupTables.CubeCornersX[6] * xAxis + LookupTables.CubeCornersY[6] * yAxis + LookupTables.CubeCornersZ[6] * zAxis;
            int index8 = originIndex + LookupTables.CubeCornersX[7] * xAxis + LookupTables.CubeCornersY[7] * yAxis + LookupTables.CubeCornersZ[7] * zAxis;

            VoxelCorners<Vector3Int> voxelCorners = new VoxelCorners<Vector3Int>(
               corners[index1],
               corners[index2],
               corners[index3],
               corners[index4],
               corners[index5],
               corners[index6],
               corners[index7],
               corners[index8]);


            VoxelCorners<float> voxelDensities = new VoxelCorners<float>(
                densities[index1],
                densities[index2],
                densities[index3],
                densities[index4],
                densities[index5],
                densities[index6],
                densities[index7],
                densities[index8]);

            return new Voxel(voxelCorners, voxelDensities);
        }

        public void Generate()
        {
            Mesh mesh = MarchingCubes.CreateMeshData(voxels, world.isolevel);

            _meshFilter.sharedMesh = mesh;
            if(_meshCollider != null)
                _meshCollider.sharedMesh = mesh;
        }

        public float GetDensity(int x, int y, int z)
        {
            return voxels[x, y, z].densities.c1;
        }

        public void SetDensity(float density, int x, int y, int z)
        {
            for (int i = 0; i < 8; i++)
            {
                int newX = x - LookupTables.CubeCornersX[i];
                int newY = y - LookupTables.CubeCornersY[i];
                int newZ = z - LookupTables.CubeCornersZ[i];

                if (newX.IsBetween(0, chunkSize - 1) &&
                    newY.IsBetween(0, chunkSize - 1) &&
                    newZ.IsBetween(0, chunkSize - 1))
                {
                    Voxel voxel = voxels[newX, newY, newZ];
                    voxel.densities[i] = density;
                    isDirty = true;
                }
            }
        }

        public void SetDensity(float density, Vector3Int pos)
        {
            SetDensity(density, pos.x, pos.y, pos.z);
        }
    }
}