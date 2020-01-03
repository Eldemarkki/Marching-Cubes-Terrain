using Unity.Jobs;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    public class ProceduralChunk : Chunk
    {
        public ProceduralWorld World { get; set; }

        // These have to be here because Unity doesn't automatically call the base class's Unity-functions
        private new void Awake()
        {
            base.Awake();
        }

        private new void Update()
        {
            base.Update();
        }

        private new void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void StartDensityCalculation()
        {
            int3 worldPosition = Coordinate * ChunkSize;

            var job = new ProceduralTerrainDensityCalculationJob
            {
                Densities = Densities,
                offset = worldPosition,
                chunkSize = ChunkSize + 1, // +1 because ChunkSize is the amount of "voxels", and that +1 is the amount of density points
                proceduralTerrainSettings = World.ProceduralTerrainSettings,
            };

            DensityCalculationJob = job;

            DensityJobHandle = IJobParallelForExtensions.Schedule<ProceduralTerrainDensityCalculationJob>(job, Densities.Length, 256);
        }
    }
}