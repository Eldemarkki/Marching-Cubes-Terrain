using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A generator that creates voxel data procedurally
    /// </summary>
    public class ProceduralVoxelDataGenerator : VoxelDataGenerator
    {
        /// <summary>
        /// The settings for the procedural generation
        /// </summary>
        [SerializeField] private ProceduralTerrainSettings proceduralTerrainSettings = new ProceduralTerrainSettings(1, 9, 120, 0);

        /// <summary>
        /// Starts generating the voxel data for a specified volume
        /// </summary>
        /// <param name="bounds">The volume to generate the voxel data for</param>
        /// <param name="allocator">The allocator for the new DensityVolume</param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public override JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            DensityVolume voxelData = new DensityVolume(bounds.size.ToInt3(), allocator);
            var job = new ProceduralTerrainDensityCalculationJob
            {
                WorldPositionOffset = bounds.min.ToInt3(),
                OutputVoxelData = voxelData,
                proceduralTerrainSettings = proceduralTerrainSettings
            };
            
            var jobHandle = job.Schedule(voxelData.Length, 256);

            JobHandleWithData<IVoxelDataGenerationJob> jobHandleWithData = new JobHandleWithData<IVoxelDataGenerationJob>();
            jobHandleWithData.JobHandle = jobHandle;
            jobHandleWithData.JobData = job;

            return jobHandleWithData;
        }
    }
}