using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
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

        /// <inheritdoc/>
        public override JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(int3 worldSpaceOrigin, VoxelDataVolume<byte> outputVoxelDataArray)
        {
            HeightmapTerrainVoxelDataCalculationJob job = new HeightmapTerrainVoxelDataCalculationJob
            {
                WorldPositionOffset = worldSpaceOrigin,
                OutputVoxelData = outputVoxelDataArray,
                HeightmapData = heightmapWorldGenerator.HeightmapTerrainSettings.HeightmapData,
                HeightmapWidth = heightmapWorldGenerator.HeightmapTerrainSettings.Width,
                HeightmapHeight = heightmapWorldGenerator.HeightmapTerrainSettings.Height,
                Amplitude = heightmapWorldGenerator.HeightmapTerrainSettings.Amplitude,
                HeightOffset = heightmapWorldGenerator.HeightmapTerrainSettings.HeightOffset
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