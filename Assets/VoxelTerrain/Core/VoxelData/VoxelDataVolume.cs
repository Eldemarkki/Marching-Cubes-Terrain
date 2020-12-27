using System;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// A 3-dimensional volume of voxel data
    /// </summary>
    public struct VoxelDataVolume<T> : IDisposable where T : struct
    {
        /// <summary>
        /// The native array which contains the voxel data. 
        /// </summary>
        private NativeArray<T> _voxelData;

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
        /// How many voxel data points does this volume contain
        /// </summary>
        public int Length => Width * Height * Depth;

        /// <summary>
        /// Is the voxel data array allocated in memory
        /// </summary>
        public bool IsCreated => _voxelData.IsCreated;

        /// <summary>
        /// Creates a <see cref="VoxelDataVolume"/> with a persistent allocator
        /// </summary>
        /// <param name="width">The width of the volume</param>
        /// <param name="height">The height of the volume</param>
        /// <param name="depth">The depth of the volume</param>
        /// <exception cref="ArgumentException">Thrown when any of the dimensions is negative</exception>
        public VoxelDataVolume(int width, int height, int depth) : this(width, height, depth, Allocator.Persistent) { }

        /// <summary>
        /// Creates a <see cref="VoxelDataVolume"/>
        /// </summary>
        /// <param name="width">The width of the volume</param>
        /// <param name="height">The height of the volume</param>
        /// <param name="depth">The depth of the volume</param>
        /// <param name="allocator">How the memory should be allocated</param>
        /// <exception cref="ArgumentException">Thrown when any of the dimensions is negative</exception>
        public VoxelDataVolume(int width, int height, int depth, Allocator allocator)
        {
            if (width < 0 || height < 0 || depth < 0)
            {
                throw new ArgumentException("The dimensions of this volume must all be positive!");
            }

            _voxelData = new NativeArray<T>(width * height * depth, allocator);

            Width = width;
            Height = height;
            Depth = depth;
        }

        /// <summary>
        /// Creates a <see cref="VoxelDataVolume"/> with a persistent allocator
        /// </summary>
        /// <param name="size">Amount of items in 1 dimension of this volume</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="size"/> is negative</exception>
        public VoxelDataVolume(int size) : this(size, Allocator.Persistent) { }

        /// <summary>
        /// Creates a <see cref="VoxelDataVolume"/>
        /// </summary>
        /// <param name="size">Amount of items in 1 dimension of this volume</param>
        /// <param name="allocator">How the memory should be allocated</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="size"/> is negative</exception>
        public VoxelDataVolume(int size, Allocator allocator) : this(size, size, size, allocator) { }

        /// <summary>
        /// Creates a <see cref="VoxelDataVolume"/> with a persistent allocator
        /// </summary>
        /// <param name="size">The 3-dimensional size of this volume</param>
        /// <exception cref="ArgumentException">Thrown when any of the dimensions is negative</exception>
        public VoxelDataVolume(int3 size) : this(size, Allocator.Persistent) { }

        /// <summary>
        /// Creates a <see cref="VoxelDataVolume"/>
        /// </summary>
        /// <param name="size">The 3-dimensional size of this volume</param>
        /// <param name="allocator">How the memory should be allocated</param>
        /// <exception cref="ArgumentException">Thrown when any of the dimensions is negative</exception>
        public VoxelDataVolume(int3 size, Allocator allocator) : this(size.x, size.y, size.z, allocator) { }

        /// <summary>
        /// Creates a <see cref="VoxelDataVolume"/> with a persistent allocator
        /// </summary>
        /// <param name="size">The 3-dimensional size of this volume</param>
        /// <exception cref="ArgumentException">Thrown when any of the dimensions is negative</exception>
        public VoxelDataVolume(Vector3Int size) : this(size, Allocator.Persistent) { }

        /// <summary>
        /// Creates a <see cref="VoxelDataVolume"/>
        /// </summary>
        /// <param name="size">The 3-dimensional size of this volume</param>
        /// <param name="allocator">How the memory should be allocated</param>
        /// <exception cref="ArgumentException">Thrown when any of the dimensions is negative</exception>
        public VoxelDataVolume(Vector3Int size, Allocator allocator) : this(size.ToInt3(), allocator) { }

        /// <summary>
        /// Disposes the native voxel data array
        /// </summary>
        public void Dispose()
        {
            _voxelData.Dispose();
        }

        /// <summary>
        /// Stores the <paramref name="voxelData"/> at <paramref name="localPosition"/>. The <paramref name="voxelData"/> will be clamped to be in range [0, 1]
        /// </summary>
        /// <param name="voxelData">The new voxel data</param>
        /// <param name="localPosition">The location of that voxel data</param>
        public void SetVoxelData(T voxelData, int3 localPosition)
        {
            int index = IndexUtilities.XyzToIndex(localPosition, Width, Height);
            SetVoxelData(voxelData, index);
        }

        /// <summary>
        /// Stores the <paramref name="voxelData"/> at <paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>. The <paramref name="voxelData"/> will be clamped to be in range [0, 1]
        /// </summary>
        /// <param name="voxelData">The new voxel data</param>
        /// <param name="x">The x value of the voxel data location</param>
        /// <param name="y">The y value of the voxel data location</param>
        /// <param name="z">The z value of the voxel data location</param>
        public void SetVoxelData(T voxelData, int x, int y, int z)
        {
            int index = IndexUtilities.XyzToIndex(x, y, z, Width, Height);
            SetVoxelData(voxelData, index);
        }

        /// <summary>
        /// Stores the <paramref name="voxelData"/> at <paramref name="index"/>. The <paramref name="voxelData"/> will be clamped to be in range [0, 1]
        /// </summary>
        /// <param name="voxelData">The new voxel data</param>
        /// <param name="index">The index in the native array</param>
        public void SetVoxelData(T voxelData, int index)
        {
            _voxelData[index] = voxelData;
        }

        /// <summary>
        /// Tries to get the voxel data at <paramref name="localPosition"/>. If the data exists at <paramref name="localPosition"/>, true will be returned and <paramref name="voxelData"/> will be set to the value (range [0, 1]). If it doesn't exist, false will be returned and <paramref name="voxelData"/> will be set to 0.
        /// </summary>
        /// <param name="localPosition">The local position of the voxel data to get</param>
        /// <param name="voxelData">A voxel data in the range [0, 1] at <paramref name="localPosition"/></param>
        /// <returns>Does a voxel data point exist at <paramref name="localPosition"/></returns>
        public bool TryGetVoxelData(int3 localPosition, out T voxelData)
        {
            return TryGetVoxelData(localPosition.x, localPosition.y, localPosition.z, out voxelData);
        }

        /// <summary>
        /// Tries to get the voxel data at <paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>. If the data exists at <paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>, true will be returned and <paramref name="voxelData"/> will be set to the value (range [0, 1]). If it doesn't exist, false will be returned and <paramref name="voxelData"/> will be set to 0.
        /// </summary>
        /// <param name="x">The x value of the voxel data location</param>
        /// <param name="y">The y value of the voxel data location</param>
        /// <param name="z">The z value of the voxel data location</param>
        /// <param name="voxelData">A voxel data in the range [0, 1] at <paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/></param>
        /// <returns>Does a voxel data point exist at <paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/></returns>
        public bool TryGetVoxelData(int x, int y, int z, out T voxelData)
        {
            int index = IndexUtilities.XyzToIndex(x, y, z, Width, Height);
            return TryGetVoxelData(index, out voxelData);
        }

        /// <summary>
        /// Gets the voxel data at <paramref name="index"/>. If the data exists at <paramref name="index"/>, true will be returned and <paramref name="voxelData"/> will be set to the value (range [0, 1]). If it doesn't exist, false will be returned and <paramref name="voxelData"/> will be set to 0.
        /// </summary>
        /// <param name="index">The index in the native array</param>
        /// <param name="voxelData">A voxel data in the range [0, 1] at <paramref name="index"/></param>
        /// <returns>Does a voxel data point exist at <paramref name="index"/></returns>
        public bool TryGetVoxelData(int index, out T voxelData)
        {
            if (index >= 0 && index < _voxelData.Length)
            {
                voxelData = GetVoxelData(index);
                return true;
            }

            voxelData = default;
            return false;
        }

        /// <summary>
        /// Gets the voxel data at <paramref name="localPosition"/>. If the data doesn't exist at <paramref name="localPosition"/>, an <see cref="IndexOutOfRangeException"/> will be thrown
        /// </summary>
        /// <param name="localPosition">The local position of the voxel data to get</param>
        /// <returns>The voxel data at <paramref name="localPosition"/></returns>
        public T GetVoxelData(int3 localPosition)
        {
            return GetVoxelData(localPosition.x, localPosition.y, localPosition.z);
        }

        /// <summary>
        /// Gets the voxel data at <paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>. If the data doesn't exist at <paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>, an <see cref="IndexOutOfRangeException"/> will be thrown
        /// </summary>
        /// <param name="x">The x value of the voxel data location</param>
        /// <param name="y">The y value of the voxel data location</param>
        /// <param name="z">The z value of the voxel data location</param>
        /// <returns>The voxel data at <paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/></returns>
        public T GetVoxelData(int x, int y, int z)
        {
            int index = IndexUtilities.XyzToIndex(x, y, z, Width, Height);
            return GetVoxelData(index);
        }

        /// <summary>
        /// Gets the voxel data at <paramref name="index"/>. If the data doesn't exist at <paramref name="index"/>, an <see cref="IndexOutOfRangeException"/> will be thrown
        /// </summary>
        /// <param name="index">The index in the native array</param>
        /// <returns>The voxel data at <paramref name="index"/></returns>
        public T GetVoxelData(int index)
        {
            return _voxelData[index];
        }

        /// <summary>
        /// Copies the voxel data from the source volume if the volumes are the same size
        /// </summary>
        /// <param name="sourceVolume">The source volume, which should be the same size as this volume</param>
        public void CopyFrom(VoxelDataVolume<T> sourceVolume)
        {
            if (Width == sourceVolume.Width && Height == sourceVolume.Height && Depth == sourceVolume.Depth)
            {
                _voxelData.CopyFrom(sourceVolume._voxelData);
            }
            else
            {
                throw new ArgumentException($"The chunks are not the same size! Width: {Width}/{sourceVolume.Width}, Height: {Height}/{sourceVolume.Height}, Depth: {Depth}/{sourceVolume.Depth}");
            }
        }

        /// <summary>
        /// Gets the index for point <paramref name="voxelDataLocalPosition"/> for this container.
        /// </summary>
        /// <param name="voxelDataLocalPosition">The voxel data position inside of this container to get the index for</param>
        /// <returns>The voxel data index for <paramref name="voxelDataLocalPosition"/></returns>
        public int GetIndex(int3 voxelDataLocalPosition)
        {
            return IndexUtilities.XyzToIndex(voxelDataLocalPosition, Width, Height);
        }

        public unsafe void* GetUnsafePtr()
        {
            return _voxelData.GetUnsafePtr();
        }
    }
}