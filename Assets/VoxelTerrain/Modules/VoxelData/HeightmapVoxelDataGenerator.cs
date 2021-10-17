using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    public class HeightmapVoxelDataGenerator : VoxelDataGenerator
    {
        [SerializeField] private HeightmapWorldGenerator heightmapWorldGenerator;

        /// <inheritdoc/>
        public override JobHandleWithData<IVoxelDataGenerationJob<byte>> GenerateVoxelData(int3 worldSpaceOrigin, VoxelDataVolume<byte> outputVoxelDataArray, JobHandle dependency = default)
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

            return new JobHandleWithData<IVoxelDataGenerationJob<byte>>(job.Schedule(dependency), job);
        }
    }
}