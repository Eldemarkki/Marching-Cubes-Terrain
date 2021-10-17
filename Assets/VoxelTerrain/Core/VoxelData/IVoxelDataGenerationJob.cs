using Unity.Jobs;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    public interface IVoxelDataGenerationJob<T> : IJob where T : struct
    {
        /// <summary>
        /// The sampling point's world position offset
        /// </summary>
        int3 WorldPositionOffset { get; set; }

        VoxelDataVolume<T> OutputVoxelData { get; set; }
    }
}