using Eldemarkki.VoxelTerrain.Settings;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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
        [SerializeField] private ProceduralTerrainSettings proceduralTerrainSettings = new ProceduralTerrainSettings(1, 9, 120, 0, 0);

        /// <inheritdoc/>
        public override JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(int3 worldSpaceOrigin, VoxelDataVolume<byte> outputVoxelDataArray)
        {
            ProceduralTerrainVoxelDataCalculationJob job = new ProceduralTerrainVoxelDataCalculationJob
            {
                WorldPositionOffset = worldSpaceOrigin,
                OutputVoxelData = outputVoxelDataArray,
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