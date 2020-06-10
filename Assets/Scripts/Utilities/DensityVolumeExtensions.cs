using Eldemarkki.VoxelTerrain.Data;
using Eldemarkki.VoxelTerrain.Density;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    /// <summary>
    /// Extensions methods for <see cref="DensityVolume"/>
    /// </summary>
    public static class DensityVolumeExtensions
    {
        /// <summary>
        /// Gets a cube-shaped volume of densities from <paramref name="densityVolume"/>. The size of the cube is 1 unit. 
        /// </summary>
        /// <param name="densityVolume">The density volume to get the densities from</param>
        /// <param name="localPosition">The origin of the cube</param>
        /// <returns>A cube-shaped volume of densities. The size of the cube is 1 unit.</returns>
        public static VoxelCorners<float> GetDensitiesUnitCube(this DensityVolume densityVolume, int3 localPosition)
        {
            VoxelCorners<float> densities = new VoxelCorners<float>();
            for (int i = 0; i < 8; i++)
            {
                int3 voxelCorner = localPosition + LookupTables.CubeCorners[i];
                densities[i] = densityVolume.GetDensity(voxelCorner.x, voxelCorner.y, voxelCorner.z);
            }

            return densities;
        }
    }
}