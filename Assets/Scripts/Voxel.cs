using UnityEngine;

public class Voxel
{
    public VoxelCorners<Vector3Int> cornerPositions;
    public VoxelCorners<float> densities;

    public Voxel(VoxelCorners<Vector3Int> cornerPositions, VoxelCorners<float> densities)
    {
        this.cornerPositions = cornerPositions;
        this.densities = densities;
    }

    public int CalculateCubeIndex(float isolevel)
    {
        int cubeIndex = 0;

        if (densities.c1 < isolevel) cubeIndex |= 1;
        if (densities.c2 < isolevel) cubeIndex |= 2;
        if (densities.c3 < isolevel) cubeIndex |= 4;
        if (densities.c4 < isolevel) cubeIndex |= 8;
        if (densities.c5 < isolevel) cubeIndex |= 16;
        if (densities.c6 < isolevel) cubeIndex |= 32;
        if (densities.c7 < isolevel) cubeIndex |= 64;
        if (densities.c8 < isolevel) cubeIndex |= 128;

        return cubeIndex;
    }
}
