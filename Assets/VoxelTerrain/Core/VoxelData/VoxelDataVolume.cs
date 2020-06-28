using System;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// A 3-dimensional volume of voxel data
    /// </summary>
    public struct VoxelDataVolume : IDisposable
    {
        /// <summary>
        /// The native array which contains the voxel data. Voxel data is stored as bytes (0 to 255), and later mapped to go from 0 to 1
        /// </summary>
        private NativeArray<byte> _voxelData;

        /// <summary>
        /// The width of the volume
        /// </summary>
        public int Width { get; }

        /// <summary>
        /// The height of the volume
        /// </summary>
        public int Height { get; }
        
        /// <summary>
        /// The depth of the volume
        /// </summary>
        public int Depth { get; }

        /// <summary>
        /// The size of this volume
        /// </summary>
        public int3 Size => new int3(Width, Height, Depth);

        /// <summary>
        /// How many voxel data points  does this volume contain
        /// </summary>
        public int Length => Width * Height * Depth;

        /// <summary>
        /// Is the voxel data array allocated in memory
        /// </summary>
        public bool IsCreated => _voxelData.IsCreated;

        /// <summary>
        /// Constructor to create a <see cref="VoxelDataVolume"/>
        /// </summary>
        /// <param name="width">The width of the volume</param>
        /// <param name="height">The height of the volume</param>
        /// <param name="depth">The depth of the volume</param>
        /// <param name="allocator">How the memory should be allocated</param>
        public VoxelDataVolume(int width, int height, int depth, Allocator allocator = Allocator.Persistent)
        {
            _voxelData = new NativeArray<byte>(width * height * depth, allocator);

            Width = width;
            Height = height;
            Depth = depth;
        }

        /// <summary>
        /// Constructor to create a <see cref="VoxelDataVolume"/>
        /// </summary>
        /// <param name="size">Amount of items in 1 dimension of this volume</param>
        /// <param name="allocator">How the memory should be allocated</param>
        public VoxelDataVolume(int size, Allocator allocator = Allocator.Persistent) : this(size, size, size, allocator) { }

        /// <summary>
        /// Constructor to create a <see cref="VoxelDataVolume"/>
        /// </summary>
        /// <param name="size">The 3-dimensional size of this volume</param>
        /// <param name="allocator">How the memory should be allocated</param>
        public VoxelDataVolume(int3 size, Allocator allocator = Allocator.Persistent) : this(size.x, size.y, size.z, allocator) { }

        /// <summary>
        /// Disposes the native voxel data array
        /// </summary>
        public void Dispose()
        {
            _voxelData.Dispose();
        }

        /// <summary>
        /// Sets the voxel data in the specified location. Voxel data is clamped to go from 0 to 1
        /// </summary>
        /// <param name="voxelData">The new voxel data</param>
        /// <param name="localPosition">The location of that voxel data</param>
        public void SetVoxelData(float voxelData, int3 localPosition)
        {
            int index = IndexUtilities.XyzToIndex(localPosition, Width, Height);
            SetVoxelData(voxelData, index);
        }

        /// <summary>
        /// Stores the voxel data in the specified location. Voxel data is clamped to go from 0 to 1
        /// </summary>
        /// <param name="voxelData">The new voxel data</param>
        /// <param name="x">The x value of the voxel data location</param>
        /// <param name="y">The y value of the voxel data location</param>
        /// <param name="z">The z value of the voxel data location</param>
        public void SetVoxelData(float voxelData, int x, int y, int z)
        {
            int index = IndexUtilities.XyzToIndex(x, y, z, Width, Height);
            SetVoxelData(voxelData, index);
        }

        /// <summary>
        /// Stores the voxel data to the specified index. Voxel data is clamped to go from 0 to 1
        /// </summary>
        /// <param name="voxelData">The new voxel data</param>
        /// <param name="index">The index in the native array</param>
        public void SetVoxelData(float voxelData, int index)
        {
            _voxelData[index] = (byte) (255f * math.saturate(voxelData));
        }

        /// <summary>
        /// Gets the voxel data from the local position (x,y,z), voxel data is in range from 0 to 1
        /// </summary>
        /// <param name="localPosition">The local position of the voxel data to get</param>
        /// <returns>A voxel data in the range from 0 to 1 in the specified location</returns>
        public float GetVoxelData(int3 localPosition)
        {
            return GetVoxelData(localPosition.x, localPosition.y, localPosition.z);
        }

        /// <summary>
        /// Gets the voxel data from the local position (x,y,z), voxel data is in range from 0 to 1
        /// </summary>
        /// <param name="x">The x value of the voxel data location</param>
        /// <param name="y">The y value of the voxel data location</param>
        /// <param name="z">The z value of the voxel data location</param>
        /// <returns>A voxel data in the range from 0 to 1 in the specified location</returns>
        public float GetVoxelData(int x, int y, int z)
        {
            int index = IndexUtilities.XyzToIndex(x, y, z, Width, Height);
            return GetVoxelData(index);
        }

        /// <summary>
        /// Gets the voxel data from an index in the native array, voxel data is in range from 0 to 1
        /// </summary>
        /// <param name="index">The index in the native array</param>
        /// <returns>A voxel data in the range from 0 to 1 at the specified index</returns>
        public float GetVoxelData(int index)
        {
            return _voxelData[index] / 255f;
        }

        /// <summary>
        /// Copies the voxel data from the source volume if the volumes are the same size
        /// </summary>
        /// <param name="sourceVolume">The source volume, which should be the same size as this volume</param>
        public void CopyFrom(VoxelDataVolume sourceVolume)
        {
            if (Width == sourceVolume.Width && Height == sourceVolume.Height && Depth == sourceVolume.Depth)
            {
                _voxelData.CopyFrom(sourceVolume._voxelData);
            }
            else
            {
                throw new Exception(
                    $"The chunks are not the same size! Width: {Width}/{sourceVolume.Width}, Height: {Height}/{sourceVolume.Height}, Depth: {Depth}/{sourceVolume.Depth}");
            }
        }
    }
}