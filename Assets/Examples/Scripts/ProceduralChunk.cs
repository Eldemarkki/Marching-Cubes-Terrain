using Unity.Jobs;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    public class ProceduralChunk : Chunk
    {
        public ProceduralWorld World { get; set; }

        // These have to be here because Unity doesn't automatically call the base class's Unity-functions
        protected override void Awake()
        {
            base.Awake();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void Initialize(int chunkSize, float isolevel, int3 coordinate)
        {
            base.Initialize(chunkSize, isolevel, coordinate);
            SetCoordinate(Coordinate);
        }

        public void SetCoordinate(int3 coordinate)
        {
            Coordinate = coordinate;
            transform.position = coordinate.ToVectorInt() * ChunkSize;
            name = $"Chunk_{coordinate.x.ToString()}_{coordinate.y.ToString()}_{coordinate.z.ToString()}";

            StartDensityCalculation();
            StartMeshGeneration();
        }

        public override void StartDensityCalculation()
        {
            MarchingCubesJobHandle.Complete();
            
            int3 worldPosition = Coordinate * ChunkSize;

            var job = new ProceduralTerrainDensityCalculationJob
            {
                Densities = Densities,
                offset = worldPosition,
                chunkSize = ChunkSize + 1, // +1 because ChunkSize is the amount of "voxels", and that +1 is the amount of density points
                proceduralTerrainSettings = World.ProceduralTerrainSettings
            };

            DensityCalculationJob = job;

            DensityJobHandle = job.Schedule(Densities.Length, 256);
        }
    }
}