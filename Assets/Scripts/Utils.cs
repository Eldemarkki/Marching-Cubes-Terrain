using UnityEngine;

namespace MarchingCubes
{
    public static class Utils
    {
        public static int FloorToNearestX(this float n, int x)
        {
            return Mathf.FloorToInt(n / x) * x;
        }

        public static Vector3 Abs(this Vector3 n)
        {
            var x = Mathf.Abs(n.x);
            var y = Mathf.Abs(n.y);
            var z = Mathf.Abs(n.z);

            return new Vector3(x, y, z);
        }

        public static Vector3Int FloorToNearestX(this Vector3 n, int x)
        {
            var flooredX = FloorToNearestX(n.x, x);
            var flooredY = FloorToNearestX(n.y, x);
            var flooredZ = FloorToNearestX(n.z, x);

            return new Vector3Int(flooredX, flooredY, flooredZ);
        }

        public static Vector3Int FloorToNearestX(this Vector3Int n, int x)
        {
            return FloorToNearestX(new Vector3(n.x, n.y, n.z), x);
        }

        public static Vector3Int Floor(this Vector3 n)
        {
            return new Vector3Int(Mathf.FloorToInt(n.x), Mathf.FloorToInt(n.y), Mathf.FloorToInt(n.z));
        }

        public static Vector3Int Mod(this Vector3Int n, int x)
        {
            var modX = Mod(n.x, x);
            var modY = Mod(n.y, x);
            var modZ = Mod(n.z, x);

            return new Vector3Int(modX, modY, modZ);
        }

        public static int Mod(this int n, int x)
        {
            return (n % x + x) % x;
        }

        public static float Map(this float value, float x1, float y1, float x2, float y2)
        {
            return (value - x1) / (y1 - x1) * (y2 - x2) + x2;
        }
    }
}