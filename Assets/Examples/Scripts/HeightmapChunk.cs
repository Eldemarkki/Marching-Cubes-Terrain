using Unity.Jobs;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    public class HeightmapChunk : Chunk
    {
        public HeightmapWorld World { get; set; }

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

        public override void StartDensityCalculation()
        {
            int3 worldPosition = Coordinate * ChunkSize;

            var job = new HeightmapTerrainDensityCalculationJob
            {
                Densities = Densities,
                heightmapData = World.HeightmapTerrainSettings.HeightmapData,
                offset = worldPosition,
                chunkSize = ChunkSize + 1, // +1 because chunkSize is the amount of "voxels", and that +1 is the amount of density points
                heightmapWidth = World.HeightmapTerrainSettings.Width,
                heightmapHeight = World.HeightmapTerrainSettings.Height,
                amplitude = World.HeightmapTerrainSettings.Amplitude,
                heightOffset = World.HeightmapTerrainSettings.HeightOffset
            };

            DensityCalculationJob = job;

            DensityJobHandle = IJobParallelForExtensions.Schedule<HeightmapTerrainDensityCalculationJob>(job, Densities.Length, 256);
        }
    }
}