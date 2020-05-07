using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes
{
    public static class Utils
    {
        /// <summary>
        /// Floors the value to a multiple of x
        /// </summary>
        /// <param name="n">The value to floor</param>
        /// <param name="x">The multiple to floor to</param>
        /// <returns>The floored value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 FloorToMultipleOfX(this float3 n, int x)
        {
            return (int3)(math.floor(n / x) * x);
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
        /// Calculates the remainder of a division operation for int3. Ensures that the returned value is positive
        /// </summary>
        /// <param name="n">The divident</param>
        /// <param name="x">The divisor</param>
        /// <returns>The remainder of n/x</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 Mod(this int3 n, int x)
        {
            return (n % x + x) % x;
        }
    }
}