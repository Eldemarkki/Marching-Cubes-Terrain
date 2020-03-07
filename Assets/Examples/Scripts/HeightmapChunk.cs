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
        /// The HeightmapWorld that owns this chunk
        /// </summary>
        public HeightmapWorld World { get; set; }

        /// <summary>
        /// Starts the density calculation
        /// </summary>
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