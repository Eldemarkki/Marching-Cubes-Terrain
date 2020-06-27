using Eldemarkki.VoxelTerrain.VoxelData;
using Unity.Collections;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
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
        public abstract JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(Bounds bounds, Allocator allocator = Allocator.Persistent);
    }
}