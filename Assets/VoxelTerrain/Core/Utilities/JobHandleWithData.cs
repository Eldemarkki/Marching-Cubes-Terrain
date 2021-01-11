using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Jobs;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    /// <summary>
    /// A class that associates a JobHandle to some data
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    public class JobHandleWithData<T>
    {
        /// <summary>
        /// The job handle
        /// </summary>
        public JobHandle JobHandle { get; set; }

        /// <summary>
        /// The associated data
        /// </summary>
        public T JobData { get; set; }
    }

    /// <summary>
    /// A class that associates a JobHandle to some data
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    public class JobHandleWithDataAndChunkProperties<T>: JobHandleWithData<T>
    {
        /// <summary>
        /// The chunk properties used for this job
        /// </summary>
        public ChunkProperties ChunkProperties { get; set; }
    }
}