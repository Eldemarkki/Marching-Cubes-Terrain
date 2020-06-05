using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Data
{
    /// <summary>
    /// A 3 dimensional wrapper for NativeArrays
    /// </summary>
    public struct DensityStorage : IDisposable
    {
        /// <summary>
        /// The native array which contains the density values. Densities are stored as bytes (0 to 255), and later mapped to go from -1 to 1
        /// </summary>
        private NativeArray<byte> _densities;

        /// <summary>
        /// Amount of items in 1 dimension of this container
        /// </summary>
        private int chunkSize;

        /// <summary>
        /// The size of this container
        /// </summary>
        public int Length => chunkSize * chunkSize * chunkSize;

        /// <summary>
        /// Is the densities array allocated
        /// </summary>
        public bool IsCreated => _densities.IsCreated;

        /// <summary>
        /// Constructor to create a DensityStorage
        /// </summary>
        /// <param name="chunkSize">Amount of items in 1 dimension of this container</param>
        public DensityStorage(int chunkSize)
        {
            this.chunkSize = chunkSize;
            _densities = new NativeArray<byte>(chunkSize * chunkSize * chunkSize, Allocator.Persistent);
        }

        /// <summary>
        /// Disposes the native array
        /// </summary>
        public void Dispose()
        {
            _densities.Dispose();
        }

        /// <summary>
        /// Gets the density value from the local position (x,y,z) in the range from -1 to 1
        /// </summary>
        /// <param name="x">The x value of the density location</param>
        /// <param name="y">The y value of the density location</param>
        /// <param name="z">The z value of the density location</param>
        /// <returns>A density value in the range from -1 to 1 in the specified location</returns>
        public float GetDensity(int x, int y, int z)
        {
            int index = XYZToIndex(x, y, z);
            return GetDensity(index);
        }

        /// <summary>
        /// Gets the density value from an index in the native array, return value is in the range from -1 to 1
        /// </summary>
        /// <param name="index">The index in the native array</param>
        /// <returns>A density value in the range from -1 to 1 at the specified index</returns>
        public float GetDensity(int index)
        {
            return _densities[index] / 127.5f - 1;
        }

        /// <summary>
        /// Stores the density in the specified location. Density is clamped to go from -1 to 1
        /// </summary>
        /// <param name="density">The new density</param>
        /// <param name="x">The x value of the density location</param>
        /// <param name="y">The y value of the density location</param>
        /// <param name="z">The z value of the density location</param>
        public void SetDensity(float density, int x, int y, int z)
        {
            int index = XYZToIndex(x, y, z);
            SetDensity(density, index);
        }

        /// <summary>
        /// Stores the density to the specified index. Density is clamped to go from -1 to 1
        /// </summary>
        /// <param name="density">The new density</param>
        /// <param name="index">The index in the native array</param>
        public void SetDensity(float density, int index)
        {
            _densities[index] = (byte)(127.5 * (math.clamp(density, -1, 1) + 1));
        }

        /// <summary>
        /// Converts a 3D location to a 1D index
        /// </summary>
        /// <param name="x">The x value of the location</param>
        /// <param name="y">The y value of the location</param>
        /// <param name="z">The z value of the location</param>
        /// <returns>The 1D representation of the specified location</returns>
        private int XYZToIndex(int x, int y, int z)
        {
            return x * chunkSize * chunkSize + y * chunkSize + z;
        }
    }
}