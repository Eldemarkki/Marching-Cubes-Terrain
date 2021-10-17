using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Jobs;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public class JobHandleWithDataAndChunkProperties<T> : JobHandleWithData<T>
    {
        public ChunkProperties ChunkProperties { get; set; }

        public JobHandleWithDataAndChunkProperties(JobHandle jobHandle, T jobData, ChunkProperties chunkProperties) : base(jobHandle, jobData)
        {
            ChunkProperties = chunkProperties;
        }
    }
}