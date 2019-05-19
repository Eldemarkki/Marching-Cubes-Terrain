using UnityEngine;

public static class Utils
{
    public static float Abs(this float n)
    {
        if (n < 0)
            return -n;
        else
            return n;
    }

    public static int Abs(this int n)
    {
        return (int)Abs((float)n);
    }

    public static int Floor(this float n)
    {
        return (int)Mathf.Floor(n);
    }

    public static int Ceil(this float n)
    {
        return Floor(n) + 1;
    }

    public static int Round(this float n)
    {
        int floored = Floor(n);

        float diff = n - floored;
        if (diff < 0.5f)
            return floored;
        else
            return floored + 1;
    }

    public static int RoundToNearestX(this float n, int x)
    {
        return Round(n / x) * x;
    }

    public static int FloorToNearestX(this float n, int x)
    {
        return Floor(n / x) * x;
    }

    public static int CeilToNearestX(this float n, int x)
    {
        return Ceil(n / x) * x;
    }

    public static Vector3 Abs(this Vector3 n)
    {
        float x = Abs(n.x);
        float y = Abs(n.y);
        float z = Abs(n.z);

        return new Vector3(x, y, z);
    }

    public static Vector3Int Floor(this Vector3 n)
    {
        int x = Floor(n.x);
        int y = Floor(n.y);
        int z = Floor(n.z);

        return new Vector3Int(x, y, z);
    }

    public static Vector3Int Ceil(this Vector3 n)
    {
        int x = Ceil(n.x);
        int y = Ceil(n.y);
        int z = Ceil(n.z);

        return new Vector3Int(x, y, z);
    }

    public static Vector3Int Round(this Vector3 n)
    {
        int x = Round(n.x);
        int y = Round(n.y);
        int z = Round(n.z);

        return new Vector3Int(x, y, z);
    }

    public static Vector3Int Abs(this Vector3Int n)
    {
        int x = Abs(n.x);
        int y = Abs(n.y);
        int z = Abs(n.z);

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

    public static float Pow(this int @base, int exponent)
    {
        bool exponentNegative = exponent < 0;

        if (exponentNegative)
            exponent = -exponent;

        int result = 1;

        for (int i = 0; i < exponent; i++)
        {
            result *= @base;
        }

        if (exponentNegative)
            return 1f / result;
        else
            return result;
    }

    public static int Clamp(this int number, int min, int max)
    {
        if (number < min)
            return min;
        else if (number > max)
            return max;
        else
            return number;
    }

    public static float Clamp(this float number, float min, float max)
    {
        if (number < min)
            return min;
        else if (number > max)
            return max;
        else
            return number;
    }

    public static float Clamp01(this float number)
    {
        return Clamp(number, 0, 1);
    }

    public static int Mod(this int n, int x)
    {
        return (n % x + x) % x;
    }

    public static float Max(this float a, float b)
    {
        return a > b ? a : b;
    }

    public static float Min(this float a, float b)
    {
        return a < b ? a : b;
    }

    public static float SqrDistance(this Vector3 p1, Vector3 p2)
    {
        Vector3 p3 = p1 - p2;

        float x = p3.x;
        float y = p3.y;
        float z = p3.z;

        float result = x * x + y * y + z * z;

        return result;
    }

    public static float SqrDistance(float x, float y, float z, Vector3 p1)
    {
        float _x = x - p1.x;
        float _y = y - p1.y;
        float _z = z - p1.z;

        float result = _x * _x + _y * _y + _z * _z;

        return result;
    }

    public static float Distance(this Vector3 p1, Vector3 p2)
    {
        float sqrD = SqrDistance(p1, p2);
        float result = Mathf.Sqrt(sqrD);

        return result;
    }

    public static float Distance(float x, float y, float z, Vector3 p2)
    {
        float sqrD = SqrDistance(x, y, z, p2);
        float result = Mathf.Sqrt(sqrD);

        return result;
    }

    public static readonly int[] PowerOf2s =
    {
        1,2,4,8,16,32,64,128,256,512,1024,2048,4096,8192,
    };

    public static float Map(this float value, float x1, float y1, float x2, float y2)
    {
        return (value - x1) / (y1 - x1) * (y2 - x2) + x2;
    }
}
