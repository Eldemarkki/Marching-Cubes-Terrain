using Eldemarkki.VoxelTerrain.World;
using Unity.Jobs;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// A store which handles getting and setting the voxel data for the world
    /// </summary>
    public class VoxelDataStore : PerVoxelStore<byte>
    {
        public override JobHandle GenerateDataForChunkUnchecked(int3 chunkCoordinate, VoxelDataVolume<byte> existingData, JobHandle dependency = default)
        {
            int3 chunkWorldOrigin = chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize;
            var jobHandleWithData = VoxelWorld.VoxelDataGenerator.GenerateVoxelData(chunkWorldOrigin, existingData, dependency);
            _generationJobHandles.Add(chunkCoordinate, jobHandleWithData);
            return jobHandleWithData.JobHandle;
        }
    }
}
