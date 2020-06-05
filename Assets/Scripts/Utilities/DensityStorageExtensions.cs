using Eldemarkki.VoxelTerrain.Data;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    /// <summary>
    /// Extensions methods for <see cref="DensityStorage"/>
    /// </summary>
    public static class DensityStorageExtensions
    {
        /// <summary>
        /// Gets a cube-shaped volume of densities from <paramref name="densityStorage"/>. The size of the cube is 1 unit. 
        /// </summary>
        /// <param name="densityStorage">The density storage to get the densities from</param>
        /// <param name="localPosition">The origin of the cube</param>
        /// <returns></returns>
        public static VoxelCorners<float> GetCubeVolume(this DensityStorage densityStorage, int3 localPosition)
        {
            VoxelCorners<float> densities = new VoxelCorners<float>();
            for (int i = 0; i < 8; i++)
            {
                int3 voxelCorner = localPosition + LookupTables.CubeCorners[i];
                densities[i] = densityStorage.GetDensity(voxelCorner.x, voxelCorner.y, voxelCorner.z);
            }

            return densities;
        }
    }
}