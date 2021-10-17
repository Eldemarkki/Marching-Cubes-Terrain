using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public static class VectorUtilities
    {
        /// <summary>
        /// Floors the <paramref name="n"/> to a multiple of <paramref name="x"/>
        /// </summary>
        /// <param name="n">The value to floor</param>
        /// <param name="x">The multiple to floor to</param>
        /// <returns>The floored value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int3 FloorToMultipleOfX(this float3 n, int3 x) => (int3)(math.floor(n / x) * x);

        /// <inheritdoc cref="FloorToMultipleOfX(float3, int3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int3 FloorToMultipleOfX(this Vector3 n, int3 x) => (int3)(math.floor((float3)n / x) * x);

        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static Vector3Int ToVectorInt(this int3 n) => new Vector3Int(n.x, n.y, n.z);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int3 ToInt3(this Vector3 n) => new int3(n);
        [MethodImpl(MethodImplOptions.AggressiveInlining)] public static int3 ToInt3(this Vector3Int n) => new int3(n.x, n.y, n.z);

        /// <summary>
        /// Calculates the remainder of a division operation for int3. Ensures that the returned value is positive
        /// </summary>
        /// <param name="n">The dividend</param>
        /// <param name="x">The divisor</param>
        /// <returns>The remainder of n/x</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 Mod(this int3 n, int3 x) => (n % x + x) % x;

        /// <summary>
        /// Converts a world position to a chunk coordinate
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 WorldPositionToCoordinate(float3 worldPosition, int3 chunkSize) => worldPosition.FloorToMultipleOfX(chunkSize) / chunkSize;

        /// <inheritdoc cref="WorldPositionToCoordinate(float3, int3)"/>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 WorldPositionToCoordinate(Vector3 worldPosition, int3 chunkSize) => worldPosition.FloorToMultipleOfX(chunkSize) / chunkSize;
    }
}