using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    public abstract class VoxelDataGenerator : MonoBehaviour
    {
        /// <summary>
        /// Starts generating the voxel data for the specified volume with <see cref="Allocator.Persistent"/>
        /// </summary>
        /// <param name="bounds">The world-space volume to generate the voxel data for</param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public JobHandleWithData<IVoxelDataGenerationJob<byte>> GenerateVoxelData(BoundsInt bounds)
        {
            return GenerateVoxelData(bounds, Allocator.Persistent);
        }

        /// <summary>
        /// Starts generating the voxel data for the specified volume
        /// </summary>
        /// <inheritdoc cref="GenerateVoxelData(BoundsInt)" path="/param[@name='bounds']"/>
        /// <param name="allocator">The allocator for the new voxel data array</param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public JobHandleWithData<IVoxelDataGenerationJob<byte>> GenerateVoxelData(BoundsInt bounds, Allocator allocator)
        {
            VoxelDataVolume<byte> voxelDataArray = new VoxelDataVolume<byte>(bounds.size, allocator);
            int3 worldSpaceOrigin = bounds.min.ToInt3();
            return GenerateVoxelData(worldSpaceOrigin, voxelDataArray, default);
        }

        /// <summary>
        /// Starts generating the voxel data for the given volume, where the origin of the volume is at <paramref name="worldSpaceOrigin"/>
        /// </summary>
        /// <param name="worldSpaceOrigin">The world space origin of <paramref name="outputVoxelDataArray"/></param>
        /// <param name="outputVoxelDataArray">The volume where the new voxel data should be generated to</param>
        /// <returns>The job handle and the voxel data generation job</returns>
        public abstract JobHandleWithData<IVoxelDataGenerationJob<byte>> GenerateVoxelData(int3 worldSpaceOrigin, VoxelDataVolume<byte> outputVoxelDataArray, JobHandle dependency);
    }
}