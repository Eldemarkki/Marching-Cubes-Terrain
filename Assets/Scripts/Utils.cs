using UnityEngine;

namespace MarchingCubes
{
    public static class Utils
    {
        public static int RoundToNearestX(this float n, int x)
        {
            return Mathf.RoundToInt(n / x) * x;
        }

        public static int FloorToNearestX(this float n, int x)
        {
            return Mathf.FloorToInt(n / x) * x;
        }

        public static int CeilToNearestX(this float n, int x)
        {
            return Mathf.CeilToInt(n / x) * x;
        }

        public static Vector3 Abs(this Vector3 n)
        {
            float x = Mathf.Abs(n.x);
            float y = Mathf.Abs(n.y);
            float z = Mathf.Abs(n.z);

            return new Vector3(x, y, z);
        }

        public static Vector3Int Floor(this Vector3 n)
        {
            int x = Mathf.FloorToInt(n.x);
            int y = Mathf.FloorToInt(n.y);
            int z = Mathf.FloorToInt(n.z);

            return new Vector3Int(x, y, z);
        }

        public static Vector3Int Ceil(this Vector3 n)
        {
            int x = Mathf.CeilToInt(n.x);
            int y = Mathf.CeilToInt(n.y);
            int z = Mathf.CeilToInt(n.z);

            return new Vector3Int(x, y, z);
        }

        public static Vector3Int Round(this Vector3 n)
        {
            int x = Mathf.RoundToInt(n.x);
            int y = Mathf.RoundToInt(n.y);
            int z = Mathf.RoundToInt(n.z);

            return new Vector3Int(x, y, z);
        }

        public static Vector3Int Abs(this Vector3Int n)
        {
            int x = Mathf.Abs(n.x);
            int y = Mathf.Abs(n.y);
            int z = Mathf.Abs(n.z);

            return new Vector3Int(x, y, z);
        }

        public static Vector3Int RoundToNearestX(this Vector3 n, int x)
        {
            int _x = RoundToNearestX(n.x, x);
            int _y = RoundToNearestX(n.y, x);
            int _z = RoundToNearestX(n.z, x);

            return new Vector3Int(_x, _y, _z);
        }

        public static Vector3Int FloorToNearestX(this Vector3 n, int x)
        {
            int _x = FloorToNearestX(n.x, x);
            int _y = FloorToNearestX(n.y, x);
            int _z = FloorToNearestX(n.z, x);

            return new Vector3Int(_x, _y, _z);
        }

        public static Vector3Int FloorToNearestX(this Vector3Int n, int x)
        {
            int _x = FloorToNearestX(n.x, x);
            int _y = FloorToNearestX(n.y, x);
            int _z = FloorToNearestX(n.z, x);

            return new Vector3Int(_x, _y, _z);
        }

        public static Vector3Int CeilToNearestX(this Vector3 n, int x)
        {
            int _x = CeilToNearestX(n.x, x);
            int _y = CeilToNearestX(n.y, x);
            int _z = CeilToNearestX(n.z, x);

            return new Vector3Int(_x, _y, _z);
        }

        public static Vector3Int Mod(this Vector3Int n, int x)
        {
            int _x = Mod(n.x, x);
            int _y = Mod(n.y, x);
            int _z = Mod(n.z, x);

            return new Vector3Int(_x, _y, _z);
        }

        public static bool IsBetween(this int value, int min, int max)
        {
            return value >= min && value <= max;
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