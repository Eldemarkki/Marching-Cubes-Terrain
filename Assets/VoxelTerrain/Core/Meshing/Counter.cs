using System;
using System.Threading;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Eldemarkki.VoxelTerrain.Meshing
{
    /// <summary>
    /// An incremental counter made for the Unity Job System
    /// </summary>
    public unsafe struct NativeCounter : IDisposable, IEquatable<NativeCounter>
    {
        /// <summary>
        /// The allocator for the counter
        /// </summary>
        private readonly Allocator _allocator;
        
        /// <summary>
        /// The pointer to the value
        /// </summary>
        [NativeDisableUnsafePtrRestriction] private readonly int* _counter;

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
        /// Disposes the counter
        /// </summary>
        public void Dispose()
        {
            UnsafeUtility.Free(_counter, _allocator);
        }

        public bool Equals(NativeCounter other)
        {
            if (other == null) { return false; }

            return _allocator == other._allocator &&
                   _counter == other._counter &&
                   Count == other.Count;
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }

            if(obj is NativeCounter other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {

            unchecked
            {
                int hash = 17;

                hash = hash * 23 + ((int)_allocator).GetHashCode();
                hash = hash * 23 + Count.GetHashCode();

                return hash;
            }
        }

        public static bool operator ==(NativeCounter left, NativeCounter right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NativeCounter left, NativeCounter right)
        {
            return !(left == right);
        }
    }
}