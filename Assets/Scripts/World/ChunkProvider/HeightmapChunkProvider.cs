using Eldemarkki.VoxelTerrain.Density;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A chunk provider for heightmap chunks
    /// </summary>
    public class HeightmapChunkProvider : ChunkProvider
    {
        /// <summary>
        /// Information about how the heightmap world should be generated
        /// </summary>
        [SerializeField] private HeightmapTerrainSettings heightmapTerrainSettings = null;

        /// <summary>
        /// Information about how the heightmap world should be generated
        /// </summary>
        public HeightmapTerrainSettings HeightmapTerrainSettings => heightmapTerrainSettings;

        protected override void Awake()
        {
            base.Awake();
            heightmapTerrainSettings.Initialize(heightmapTerrainSettings.Heightmap, heightmapTerrainSettings.Amplitude, heightmapTerrainSettings.HeightOffset);
        }

        private void OnDestroy()
        {
            heightmapTerrainSettings.Dispose();
        }

        /// <summary>
        /// Calculates the densities for a chunk at a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose densities will be calculated</param>
        /// <returns>A density volume containing the densities. The volume's size is (chunkSize+1)^3</returns>
        protected override DensityVolume CalculateChunkDensities(int3 chunkCoordinate)
        {
            DensityVolume densityVolume = new DensityVolume(ChunkGenerationParams.ChunkSize + 1);

            var job = new HeightmapTerrainDensityCalculationJob
            {
                DensityVolume = densityVolume,
                heightmapData = heightmapTerrainSettings.HeightmapData,
                offset = chunkCoordinate * ChunkGenerationParams.ChunkSize,
                chunkSize = ChunkGenerationParams.ChunkSize + 1, // +1 because chunkSize is the amount of "voxels", and that +1 is the amount of density points
                heightmapWidth = heightmapTerrainSettings.Width,
                heightmapHeight = heightmapTerrainSettings.Height,
                amplitude = heightmapTerrainSettings.Amplitude,
                heightOffset = heightmapTerrainSettings.HeightOffset
            };

            job.Schedule(
                (ChunkGenerationParams.ChunkSize + 1) * (ChunkGenerationParams.ChunkSize + 1) *
                (ChunkGenerationParams.ChunkSize + 1), 256).Complete();

            return job.DensityVolume;
        }
    }
}