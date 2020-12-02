using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    /// <summary>
    /// A collection of utilities that operate on vectors (int3/float3)
    /// </summary>
    public static class VectorUtilities
    {
        /// <summary>
        /// Floors the value to a multiple of x
        /// </summary>
        /// <param name="n">The value to floor</param>
        /// <param name="x">The multiple to floor to</param>
        /// <returns>The floored value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 FloorToMultipleOfX(this float3 n, int3 x)
        {
            return (int3)(math.floor(n / x) * x);
        }

        /// <summary>
        /// Floors the value to a multiple of x
        /// </summary>
        /// <param name="n">The value to floor</param>
        /// <param name="x">The multiple to floor to</param>
        /// <returns>The floored value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 FloorToMultipleOfX(this Vector3 n, int3 x)
        {
            return (int3)(math.floor(new float3(n.x / x.x, n.y / x.y, n.z / x.z)) * x);
        }

        /// <summary>
        /// Converts an int3 value to Vector3Int
        /// </summary>
        /// <param name="n">The int3 value to convert</param>
        /// <returns>The converted value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ToVectorInt(this int3 n)
        {
            return new Vector3Int(n.x, n.y, n.z);
        }

        /// <summary>
        /// Converts a Vector3 to a int3 by casting the xyz values to ints
        /// </summary>
        /// <param name="n">The Vector3 to be converted</param>
        /// <returns>The converted int3</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 ToInt3(this Vector3 n)
        {
            return new int3((int)n.x, (int)n.y, (int)n.z);
        }

        /// <summary>
        /// Converts a Vector3 to a int3 by casting the xyz values to ints
        /// </summary>
        /// <param name="n">The Vector3 to be converted</param>
        /// <returns>The converted int3</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 ToInt3(this Vector3Int n)
        {
            return new int3(n.x, n.y, n.z);
        }

        /// <summary>
        /// Calculates the remainder of a division operation for int3. Ensures that the returned value is positive
        /// </summary>
        /// <param name="n">The dividend</param>
        /// <param name="x">The divisor</param>
        /// <returns>The remainder of n/x</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 Mod(this int3 n, int3 x)
        {
            return (n % x + x) % x;
        }

        /// <summary>
        /// Converts a world position to a chunk coordinate
        /// </summary>
        /// <param name="worldPosition">The world-position that should be converted</param>
        /// <param name="chunkSize">The size of a chunk in the world</param>
        /// <returns>The chunk coordinate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 WorldPositionToCoordinate(float3 worldPosition, int3 chunkSize)
        {
            return worldPosition.FloorToMultipleOfX(chunkSize) / chunkSize;
        }

        /// <summary>
        /// Converts a world position to a chunk coordinate
        /// </summary>
        /// <param name="worldPosition">The world-position that should be converted</param>
        /// <param name="chunkSize">The size of a chunk in the world</param>
        /// <returns>The chunk coordinate</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 WorldPositionToCoordinate(Vector3 worldPosition, int3 chunkSize)
        {
            return worldPosition.FloorToMultipleOfX(chunkSize) / chunkSize;
        }
    }
}