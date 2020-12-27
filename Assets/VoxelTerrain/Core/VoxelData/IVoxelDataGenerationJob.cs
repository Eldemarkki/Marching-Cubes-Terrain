using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// An interface for voxel data generation jobs
    /// </summary>
    public interface IVoxelDataGenerationJob : IJob
    {
        /// <summary>
        /// The sampling point's world position offset
        /// </summary>
        int3 WorldPositionOffset { get; set; }

        /// <summary>
        /// The generated voxel data
        /// </summary>
        NativeArray<byte> OutputVoxelData { get; set; }

        /// <summary>
        /// The 3-dimensional size of <see cref="OutputVoxelData"/>
        /// </summary>
        int3 OutputVoxelDataDimensions { get; set; }
    }
}