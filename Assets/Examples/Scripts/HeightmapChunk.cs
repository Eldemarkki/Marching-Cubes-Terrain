using Unity.Jobs;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    public class HeightmapChunk : Chunk
    {
        public HeightmapWorld World { get; set; }

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

            DensityJobHandle = job.Schedule(Densities.Length, 256);
        }
    }
}