using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using Unity.Jobs;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    public class VoxelDataStore : PerVoxelStore<byte>
    {
        protected override JobHandleWithData<IVoxelDataGenerationJob<byte>> ScheduleGenerationJob(int3 chunkCoordinate, VoxelDataVolume<byte> existingData, JobHandle dependency = default)
        {
            int3 chunkWorldOrigin = chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize;
            return VoxelWorld.VoxelDataGenerator.GenerateVoxelData(chunkWorldOrigin, existingData, dependency);
        }
    }
}
