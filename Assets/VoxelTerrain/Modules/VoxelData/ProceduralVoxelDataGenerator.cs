using Eldemarkki.VoxelTerrain.Settings;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
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

        /// <inheritdoc/>
        public override JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(BoundsInt bounds, Allocator allocator)
        {
            VoxelDataVolume voxelData = new VoxelDataVolume(bounds.size, allocator);
            ProceduralTerrainVoxelDataCalculationJob job = new ProceduralTerrainVoxelDataCalculationJob
            {
                WorldPositionOffset = bounds.min.ToInt3(),
                OutputVoxelData = voxelData,
                ProceduralTerrainSettings = proceduralTerrainSettings
            };
            
            JobHandle jobHandle = job.Schedule();

            JobHandleWithData<IVoxelDataGenerationJob> jobHandleWithData = new JobHandleWithData<IVoxelDataGenerationJob>
            {
                JobHandle = jobHandle,
                JobData = job
            };

            return jobHandleWithData;
        }
    }
}