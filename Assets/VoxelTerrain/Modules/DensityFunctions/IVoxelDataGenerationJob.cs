using Unity.Jobs;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Density
{
    /// <summary>
    /// An interface for voxel data generation jobs
    /// </summary>
    public interface IVoxelDataGenerationJob : IJobParallelFor
    {
        /// <summary>
        /// The sampling point's world position offset
        /// </summary>
        int3 WorldPositionOffset { get; set; }

        /// <summary>
        /// The generated voxel data
        /// </summary>
        DensityVolume OutputVoxelData { get; set; }
    }
}