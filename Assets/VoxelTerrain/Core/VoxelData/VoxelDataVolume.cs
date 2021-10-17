using Eldemarkki.VoxelTerrain.Utilities;
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    public struct VoxelDataVolume<T> : IDisposable where T : struct
    {
        private NativeArray<T> _voxelData;

        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }

        public int3 Size => new int3(Width, Height, Depth);

        public int Length => Width * Height * Depth;

        /// <summary>
        /// Is the voxel data array allocated in memory
        /// </summary>
        public bool IsCreated => _voxelData.IsCreated;

        /// <summary>Creates a <see cref="VoxelDataVolume{T}"/> with a persistent allocator</summary>
        /// <exception cref="ArgumentException">Thrown when any of the dimensions is negative</exception>
        public VoxelDataVolume(int width, int height, int depth) : this(width, height, depth, Allocator.Persistent) { }

        /// <summary>Creates a <see cref="VoxelDataVolume{T}"/> with <paramref name="allocator"/></summary>
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

        /// <inheritdoc cref="VoxelDataVolume(int, int, int)"/>
        public VoxelDataVolume(int3 size) : this(size, Allocator.Persistent) { }

        /// <inheritdoc cref="VoxelDataVolume(int, int, int, Allocator)"/>
        public VoxelDataVolume(int3 size, Allocator allocator) : this(size.x, size.y, size.z, allocator) { }

        /// <inheritdoc cref="VoxelDataVolume(int3)"/>
        public VoxelDataVolume(Vector3Int size) : this(size, Allocator.Persistent) { }

        /// <inheritdoc cref="VoxelDataVolume(int3, Allocator)"/>
        public VoxelDataVolume(Vector3Int size, Allocator allocator) : this(size.ToInt3(), allocator) { }

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
        /// Copies the voxel data from <paramref name="sourceVolume"/>, if the volumes are the same size
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