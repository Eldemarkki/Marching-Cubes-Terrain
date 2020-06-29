using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// A base class for all voxel data generators
    /// </summary>
    public abstract class VoxelDataGenerator : MonoBehaviour
    {
        /// <summary>
        /// Starts generating the voxel data for a specified volume
        /// </summary>
        /// <param name="bounds">The volume to generate the voxel data for</param>
        /// <param name="allocator">The allocator for the new <see cref="VoxelDataVolume"/></param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public abstract JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(Bounds bounds, Allocator allocator);


        /// <summary>
        /// Starts generating the voxel data for a specified volume with a persistent allocator
        /// </summary>
        /// <param name="bounds">The volume to generate the voxel data for</param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(Bounds bounds)
        {
            return GenerateVoxelData(bounds, Allocator.Persistent);
        }
    }
}