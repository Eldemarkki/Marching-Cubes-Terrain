using Unity.Jobs;

namespace Eldemarkki.VoxelTerrain
{
    public class JobHandleWithData<T>
    {
        public JobHandle JobHandle { get; set; }
        public T JobData { get; set; }
    }
}