using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MarchingCubes.Examples;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
        
        // Taken and modified to use NativeSlices from here: https://forum.unity.com/threads/allow-setting-mesh-arrays-with-nativearrays.536736/
        /// <summary>
        /// Directly copies the memory for faster copying of arrays
        /// </summary>
        /// <param name="source">The source array to copy from</param>
        /// <param name="target">The target array to copy to</param>
        /// <typeparam name="T">The element type of the source and target arrays</typeparam>
        /// <exception cref="NullReferenceException">Thrown when target is null</exception>
        /// <exception cref="IndexOutOfRangeException">Thrown when target is shorter than source.</exception>
        public static unsafe void CopyToFast<T>(this NativeSlice<T> source, T[] target) where T : struct
        {
            if (target == null)
            {
                throw new NullReferenceException(nameof(target) + " is null");
            }
 
            int nativeArrayLength = source.Length;
            if (target.Length < nativeArrayLength)
            {
                throw new IndexOutOfRangeException(nameof(target) + " is shorter than " + nameof(source));
            }
 
            int byteLength = source.Length * Marshal.SizeOf(default(T));
            void* managedBuffer = UnsafeUtility.AddressOf(ref target[0]);
            void* nativeBuffer = source.GetUnsafePtr();
            Buffer.MemoryCopy(nativeBuffer, managedBuffer, byteLength, byteLength);
        }
        
        /// <summary>
        /// Intersects a plane with a line
        /// </summary>
        /// <param name="planeOrigin">The origin of the plane</param>
        /// <param name="planeNormal">The normal of the plane</param>
        /// <param name="lineOrigin">The origin of the line</param>
        /// <param name="lineDirection">The direction of the line</param>
        /// <param name="intersectionPoint">The point where the line hit the plane (if any)</param>
        /// <returns>The collision result</returns>
        public static PlaneLineIntersectionResult PlaneLineIntersection(float3 planeOrigin, float3 planeNormal, float3 lineOrigin,
            float3 lineDirection, out float3 intersectionPoint)
        {
            planeNormal = math.normalize(planeNormal);
            lineDirection = math.normalize(lineDirection);

            if (math.dot(planeNormal, lineDirection) == 0)
            {
                intersectionPoint = float3.zero;
                return (planeOrigin - lineOrigin).Equals(float3.zero) ? PlaneLineIntersectionResult.ParallelInsidePlane : PlaneLineIntersectionResult.NoHit;
            }

            var d = math.dot(planeOrigin, -planeNormal);
            var t = -(d + lineOrigin.z * planeNormal.z + lineOrigin.y * planeNormal.y + lineOrigin.x * planeNormal.x) / (lineDirection.z * planeNormal.z + lineDirection.y * planeNormal.y + lineDirection.x * planeNormal.x);
            intersectionPoint = lineOrigin + t * lineDirection;
            return PlaneLineIntersectionResult.OneHit;
        }
    }
}