using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    public class HeightmapVoxelDataGenerator : VoxelDataGenerator
    {
        [SerializeField] private HeightmapWorldGenerator heightmapWorldGenerator;

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