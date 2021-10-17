using Eldemarkki.VoxelTerrain.Settings;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    public class ProceduralVoxelDataGenerator : VoxelDataGenerator
    {
        [SerializeField] private ProceduralTerrainSettings proceduralTerrainSettings = new ProceduralTerrainSettings(1, 9, 120, 0, 0);

        /// <inheritdoc/>
        public override JobHandleWithData<IVoxelDataGenerationJob<byte>> GenerateVoxelData(int3 worldSpaceOrigin, VoxelDataVolume<byte> outputVoxelDataArray, JobHandle dependency = default)
        {
            ProceduralTerrainVoxelDataCalculationJob job = new ProceduralTerrainVoxelDataCalculationJob
            {
                WorldPositionOffset = worldSpaceOrigin,
                OutputVoxelData = outputVoxelDataArray,
                ProceduralTerrainSettings = proceduralTerrainSettings
            };

            return new JobHandleWithData<IVoxelDataGenerationJob<byte>>(job.Schedule(dependency), job);
        }
    }
}