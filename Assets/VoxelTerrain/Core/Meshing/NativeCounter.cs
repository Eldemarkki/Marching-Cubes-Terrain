using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Eldemarkki.VoxelTerrain.Meshing
{
    /// <summary>
    /// An incremental counter made for the Unity Job System
    /// </summary>
    public unsafe struct NativeCounter : IDisposable
    {
        private readonly Allocator _allocator;
        [NativeDisableUnsafePtrRestriction] private readonly int* _counter;

        public int Count
        {
            get => *_counter;
            set => *_counter = value;
        }

        public NativeCounter(Allocator allocator)
        {
            _allocator = allocator;
            _counter = (int*)UnsafeUtility.Malloc(sizeof(int), 4, allocator);
            Count = 0;
        }

        /// <summary>
        /// Increments the count by 1
        /// </summary>
        /// <returns>The original count</returns>
        public int Increment()
        {
            return Interlocked.Increment(ref *_counter) - 1;
        }

        /// <summary>
        /// Increments the count by <paramref name="increase"/>
        /// </summary>
        /// <param name="increase">How much the count should be increased by</param>
        /// <returns>The original count</returns>
        public int Add(int increase)
        {
            return Interlocked.Add(ref *_counter, increase) - increase;
        }

        public void Dispose()
        {
            UnsafeUtility.Free(_counter, _allocator);
        }
    }
}