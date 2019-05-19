using UnityEngine;

public struct Point
{
    public Vector3Int localPosition;
    public float density;

    public Point(Vector3Int localPosition, float density)
    {
        this.localPosition = localPosition;
        this.density = density;
    }
}