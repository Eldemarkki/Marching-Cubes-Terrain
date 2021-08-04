using Eldemarkki.VoxelTerrain.World.Chunks;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    /// <summary>
    /// A class that associates a JobHandle to a ChunkProperties and some data
    /// </summary>
    /// <typeparam name="T">The type of the data</typeparam>
    public class JobHandleWithDataAndChunkProperties<T> : JobHandleWithData<T>
    {
        /// <summary>
        /// The chunk properties used for this job
        /// </summary>
        public ChunkProperties ChunkProperties { get; set; }
    }
}