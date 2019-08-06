using System.Collections.Generic;
using UnityEngine;

public struct MeshData
{
    public List<Vector3> vertices;
    public int[] triangles;

    public MeshData(List<Vector3> vertices, int[] triangles)
    {
        this.vertices = vertices;
        this.triangles = triangles;
    }

    public void Deconstruct(out List<Vector3> vertices, out int[] triangles)
    {
        vertices = this.vertices;
        triangles = this.triangles;
    }
}
