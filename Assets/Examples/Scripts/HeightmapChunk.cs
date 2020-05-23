using Unity.Jobs;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// A chunk that is generated from a heightmap
    /// </summary>
    public class HeightmapChunk : Chunk
    {
        /// <summary>
        /// Information about how the heightmap world should be generated
        /// </summary>
        public HeightmapTerrainSettings HeightmapTerrainSettings { get; set; }

        /// <summary>
        /// Starts the density calculation
        /// </summary>
        public override void StartDensityCalculation()
        {
            int3 worldPosition = Coordinate * ChunkSize;

            var job = new HeightmapTerrainDensityCalculationJob
            {
                DensityStorage = DensityStorage,
                heightmapData = HeightmapTerrainSettings.HeightmapData,
                offset = worldPosition,
                chunkSize = ChunkSize + 1, // +1 because chunkSize is the amount of "voxels", and that +1 is the amount of density points
                heightmapWidth = HeightmapTerrainSettings.Width,
                heightmapHeight = HeightmapTerrainSettings.Height,
                amplitude = HeightmapTerrainSettings.Amplitude,
                heightOffset = HeightmapTerrainSettings.HeightOffset
            };

            DensityJobHandle = job.Schedule(DensityStorage.Length, 256);
        }
    }
}