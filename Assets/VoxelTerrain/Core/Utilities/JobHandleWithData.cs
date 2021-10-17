using Unity.Jobs;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public class JobHandleWithData<T>
    {
        public JobHandle JobHandle { get; set; }
        public T JobData { get; set; }

        public JobHandleWithData(JobHandle jobHandle, T jobData)
        {
            JobHandle = jobHandle;
            JobData = jobData;
        }
    }
}