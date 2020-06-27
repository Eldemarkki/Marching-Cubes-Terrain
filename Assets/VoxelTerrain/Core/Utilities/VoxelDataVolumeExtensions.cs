using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.VoxelData;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    /// <summary>
    /// Extensions methods for <see cref="VoxelDataVolume"/>
    /// </summary>
    public static class VoxelDataVolumeExtensions
    {
        /// <summary>
        /// Gets a cube-shaped volume of voxel data from <paramref name="voxelDataVolume"/>. The size of the cube is 1 unit. 
        /// </summary>
        /// <param name="voxelDataVolume">The voxel data volume to get the voxel data from</param>
        /// <param name="localPosition">The origin of the cube</param>
        /// <returns>A cube-shaped volume of voxel data. The size of the cube is 1 unit.</returns>
        public static VoxelCorners<float> GetVoxelDataUnitCube(this VoxelDataVolume voxelDataVolume, int3 localPosition)
        {
            VoxelCorners<float> voxelDataCorners = new VoxelCorners<float>();
            for (int i = 0; i < 8; i++)
            {
                int3 voxelCorner = localPosition + LookupTables.CubeCorners[i];
                voxelDataCorners[i] = voxelDataVolume.GetVoxelData(voxelCorner.x, voxelCorner.y, voxelCorner.z);
            }

            return voxelDataCorners;
        }
    }
}