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

        public T this[int index]
        {
            get => _voxelData[index];
            set => _voxelData[index] = value;
        }

        public T this[int x, int y, int z]
        {
            get => _voxelData[IndexUtilities.XyzToIndex(x, y, z, Width, Height)];
            set => _voxelData[IndexUtilities.XyzToIndex(x, y, z, Width, Height)] = value;
        }


        public T this[int3 localPosition]
        {
            get => _voxelData[IndexUtilities.XyzToIndex(localPosition, Width, Height)];
            set => _voxelData[IndexUtilities.XyzToIndex(localPosition, Width, Height)] = value;
        }

        public bool TryGetVoxelData(int3 localPosition, out T voxelData) => TryGetVoxelData(localPosition.x, localPosition.y, localPosition.z, out voxelData);
        public bool TryGetVoxelData(int x, int y, int z, out T voxelData) => TryGetVoxelData(IndexUtilities.XyzToIndex(x, y, z, Width, Height), out voxelData);
        public bool TryGetVoxelData(int index, out T voxelData)
        {
            if (index >= 0 && index < _voxelData.Length)
            {
                voxelData = this[index];
                return true;
            }

            voxelData = default;
            return false;
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

        public unsafe void* GetUnsafePtr()
        {
            return _voxelData.GetUnsafePtr();
        }
    }
}