using System.Runtime.CompilerServices;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public static class IndexUtilities
    {
        /// <summary>
        /// Converts a 3D location to a 1D index
        /// </summary>
        /// <param name="width">The size of the "container" in the x-direction</param>
        /// <param name="height">The size of the "container" in the y-direction</param>
        /// <returns>The 1D representation of the specified location in the "container"</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int XyzToIndex(int3 xyz, int width, int height)
        {
            return XyzToIndex(xyz.x, xyz.y, xyz.z, width, height);
        }

        /// <inheritdoc cref="XyzToIndex(int3, int, int)" path="/summary"/>
        /// <param name="width"><inheritdoc cref="XyzToIndex(int3, int, int)" path="/param[@name='width']"/></param>
        /// <param name="height"><inheritdoc cref="XyzToIndex(int3, int, int)" path="/param[@name='height']"/></param>
        /// <returns><inheritdoc cref="XyzToIndex(int3, int, int)" path="/returns"/></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int XyzToIndex(int x, int y, int z, int width, int height)
        {
            return z * width * height + y * width + x;
        }

        /// <summary>
        /// Converts a 1D index to a 3D location
        /// </summary>
        /// <param name="index">The 1D index in the "container"</param>
        /// <param name="width"><inheritdoc cref="XyzToIndex(int3, int, int)" path="/param[@name='width']"/></param>
        /// <param name="height"><inheritdoc cref="XyzToIndex(int3, int, int)" path="/param[@name='height']"/></param>
        /// <returns>The 3D representation of the specified index in the "container"</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 IndexToXyz(int index, int width, int height)
        {
            int3 position = new int3(
                index % width,
                index / width % height,
                index / (width * height));
            return position;
        }
    }
}