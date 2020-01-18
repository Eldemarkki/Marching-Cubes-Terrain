using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes
{
    public static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 FloorToNearestX(this float3 n, int x)
        {
            return (int3)(math.floor(n / x) * x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ToVectorInt(this int3 n)
        {
            return new Vector3Int(n.x, n.y, n.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 Mod(this int3 n, int x)
        {
            return (n % x + x) % x;

        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Map(this float value, float x1, float y1, float x2, float y2)
        {
            return (value - x1) / (y1 - x1) * (y2 - x2) + x2;
        }
        
        // Taken and modified to use NativeSlices from here: https://forum.unity.com/threads/allow-setting-mesh-arrays-with-nativearrays.536736/
        public static unsafe void CopyToFast<T>(this NativeSlice<T> nativeSlice, T[] array) where T : struct
        {
            if (array == null)
            {
                throw new NullReferenceException(nameof(array) + " is null");
            }
 
            int nativeArrayLength = nativeSlice.Length;
            if (array.Length < nativeArrayLength)
            {
                throw new IndexOutOfRangeException(
                    nameof(array) + " is shorter than " + nameof(nativeSlice));
            }
 
            int byteLength = nativeSlice.Length * Marshal.SizeOf(default(T));
            void* managedBuffer = UnsafeUtility.AddressOf(ref array[0]);
            void* nativeBuffer = nativeSlice.GetUnsafePtr();
            Buffer.MemoryCopy(nativeBuffer, managedBuffer, byteLength, byteLength);
        }
    }
}