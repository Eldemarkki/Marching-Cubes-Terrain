using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    public class ProceduralVoxelDataGenerator : VoxelDataGenerator
    {
        [SerializeField] private ProceduralTerrainSettings proceduralTerrainSettings = new ProceduralTerrainSettings(1, 9, 120, 0);

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