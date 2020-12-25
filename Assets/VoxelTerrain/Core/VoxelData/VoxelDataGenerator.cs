using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// A base class for all voxel data generators
    /// </summary>
    public abstract class VoxelDataGenerator : MonoBehaviour
    {
        /// <summary>
        /// Starts generating the voxel data for a specified volume with a persistent allocator
        /// </summary>
        /// <param name="bounds">The world-space volume to generate the voxel data for</param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(BoundsInt bounds)
        {
            return GenerateVoxelData(bounds, Allocator.Persistent);
        }

        /// <summary>
        /// Starts generating the voxel data for a specified volume
        /// </summary>
        /// <param name="bounds">The world-space volume to generate the voxel data for</param>
        /// <param name="allocator">The allocator for the new <see cref="VoxelDataVolume"/></param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(BoundsInt bounds, Allocator allocator)
        {
            VoxelDataVolume voxelDataVolume = new VoxelDataVolume(bounds.size, allocator);
            int3 worldSpaceOrigin = bounds.min.ToInt3();
            return GenerateVoxelData(worldSpaceOrigin, voxelDataVolume);
        }

        /// <summary>
        /// Starts generating the voxel data for the given volume, where the origin of the volume is at <paramref name="worldSpaceOrigin"/>
        /// </summary>
        /// <param name="worldSpaceOrigin">The world space origin of <paramref name="outputVoxelDataVolume"/></param>
        /// <param name="outputVoxelDataVolume">The volume where the new voxel data should be generated to</param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public abstract JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(int3 worldSpaceOrigin, VoxelDataVolume outputVoxelDataVolume);
    }
}