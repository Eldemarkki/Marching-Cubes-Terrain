using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace MarchingCubes
{
    /// <summary>
    /// An incremental counter made for the Unity Job System
    /// </summary>
    public unsafe struct Counter : IDisposable
    {
        /// <summary>
        /// The allocator for the counter
        /// </summary>
        private Allocator allocator;
        
        /// <summary>
        /// The pointer to the value
        /// </summary>
        [NativeDisableUnsafePtrRestriction] private int* _counter;

        /// <summary>
        /// The counter's value
        /// </summary>
        public int Count
        {
            get => *_counter;
            set => (*_counter) = value;
        }

        /// <summary>
        /// The constructor
        /// </summary>
        /// <param name="allocator">What type of allocator to use</param>
        public Counter(Allocator allocator)
        {
            this.allocator = allocator;
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
        /// Disposes the counter
        /// </summary>
        public void Dispose()
        {
            UnsafeUtility.Free(_counter, allocator);
        }
    }
}