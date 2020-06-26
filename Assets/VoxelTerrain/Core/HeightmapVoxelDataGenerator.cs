using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A generator that creates voxel data from a heightmap
    /// </summary>
    public class HeightmapVoxelDataGenerator : VoxelDataGenerator
    {
        /// <summary>
        /// The heightmap world generator which gives this class the HeightmapTerrainSettings
        /// </summary>
        [SerializeField] private HeightmapWorldGenerator heightmapWorldGenerator;

        /// <summary>
        /// Starts generating the voxel data for a specified volume
        /// </summary>
        /// <param name="bounds">The volume to generate the voxel data for</param>
        /// <param name="allocator">The allocator for the new DensityVolume</param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public override JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            DensityVolume voxelData = new DensityVolume(bounds.size.ToInt3(), allocator);

            HeightmapTerrainDensityCalculationJob job = new HeightmapTerrainDensityCalculationJob
            {
                WorldPositionOffset = bounds.min.ToInt3(),
                OutputVoxelData = voxelData,
                heightmapData = heightmapWorldGenerator.HeightmapTerrainSettings.HeightmapData,
                heightmapWidth = heightmapWorldGenerator.HeightmapTerrainSettings.Width,
                heightmapHeight = heightmapWorldGenerator.HeightmapTerrainSettings.Height,
                amplitude = heightmapWorldGenerator.HeightmapTerrainSettings.Amplitude,
                heightOffset = heightmapWorldGenerator.HeightmapTerrainSettings.HeightOffset
            };

            var jobHandle = job.Schedule(voxelData.Length, 256);

            JobHandleWithData<IVoxelDataGenerationJob> jobHandleWithData = new JobHandleWithData<IVoxelDataGenerationJob>();
            jobHandleWithData.JobHandle = jobHandle;
            jobHandleWithData.JobData = job;

            return jobHandleWithData;
        }
    }
}