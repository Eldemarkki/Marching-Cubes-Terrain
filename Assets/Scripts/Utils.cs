using System.Runtime.CompilerServices;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes
{
    public static class Utils
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 Abs(this float3 n)
        {
            float x = math.abs(n.x);
            float y = math.abs(n.y);
            float z = math.abs(n.z);

            return new float3(x, y, z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorToNearestX(this float n, int x)
        {
            return ((int)math.floor(n / x)) * x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 FloorToNearestX(this float3 n, int x)
        {
            var flooredX = FloorToNearestX(n.x, x);
            var flooredY = FloorToNearestX(n.y, x);
            var flooredZ = FloorToNearestX(n.z, x);

            return new int3(flooredX, flooredY, flooredZ);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 FloorToNearestX(this int3 n, int x)
        {
            return FloorToNearestX(new Vector3(n.x, n.y, n.z), x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 Floor(this float3 n)
        {
            return new int3((int)math.floor(n.x), (int)math.floor(n.y), (int)math.floor(n.z));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 ToMathematicsInt(this Vector3Int n)
        {
            return new int3(n.x, n.y, n.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 ToMathematicsInt(this Vector3 n)
        {
            return new int3((int)n.x, (int)n.y, (int)n.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ToMathematicsFloat(this Vector3Int n)
        {
            return new float3(n.x, n.y, n.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float3 ToMathematicsFloat(this Vector3 n)
        {
            return new float3(n.x, n.y, n.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ToVectorInt(this int3 n)
        {
            return new Vector3Int(n.x, n.y, n.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3Int ToVectorInt(this float3 n)
        {
            return new Vector3Int((int)n.x, (int)n.y, (int)n.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVectorFloat(this int3 n)
        {
            return new Vector3(n.x, n.y, n.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ToVectorFloat(this float3 n)
        {
            return new Vector3(n.x, n.y, n.z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int3 Mod(this int3 n, int x)
        {
            var modX = Mod(n.x, x);
            var modY = Mod(n.y, x);
            var modZ = Mod(n.z, x);

            return new int3(modX, modY, modZ);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Mod(this int n, int x)
        {
            return (n % x + x) % x;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Map(this float value, float x1, float y1, float x2, float y2)
        {
            return (value - x1) / (y1 - x1) * (y2 - x2) + x2;
        }
    }
}