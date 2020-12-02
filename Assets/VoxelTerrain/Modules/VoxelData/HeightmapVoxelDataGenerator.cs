using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
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
        /// <param name="allocator">The allocator for the new <see cref="VoxelDataVolume"/></param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public override JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(BoundsInt bounds, Allocator allocator)
        {
            VoxelDataVolume voxelData = new VoxelDataVolume(bounds.size, allocator);

            HeightmapTerrainVoxelDataCalculationJob job = new HeightmapTerrainVoxelDataCalculationJob
            {
                WorldPositionOffset = bounds.min.ToInt3(),
                OutputVoxelData = voxelData,
                HeightmapData = heightmapWorldGenerator.HeightmapTerrainSettings.HeightmapData,
                HeightmapWidth = heightmapWorldGenerator.HeightmapTerrainSettings.Width,
                HeightmapHeight = heightmapWorldGenerator.HeightmapTerrainSettings.Height,
                Amplitude = heightmapWorldGenerator.HeightmapTerrainSettings.Amplitude,
                HeightOffset = heightmapWorldGenerator.HeightmapTerrainSettings.HeightOffset
            };

            JobHandle jobHandle = job.Schedule();

            JobHandleWithData<IVoxelDataGenerationJob> jobHandleWithData = new JobHandleWithData<IVoxelDataGenerationJob>();
            jobHandleWithData.JobHandle = jobHandle;
            jobHandleWithData.JobData = job;

            return jobHandleWithData;
        }
    }
}