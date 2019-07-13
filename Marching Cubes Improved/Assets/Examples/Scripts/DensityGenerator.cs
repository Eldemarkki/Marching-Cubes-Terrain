public class DensityGenerator
{
    public FastNoise Noise { get; }

    public DensityGenerator(int seed)
    {
        Noise = new FastNoise(seed);
    }

    public float CalculateDensity(int worldPosX, int worldPosY, int worldPosZ)
    {
        return TerrainDensity(worldPosX, worldPosY, worldPosZ, .1f).Clamp01();
    }

    public float SphereDensity(int worldPosX, int worldPosY, int worldPosZ, int radius)
    {
        return worldPosX * worldPosX + worldPosY * worldPosY + worldPosZ * worldPosZ - radius * radius;
    }

    public float TerrainDensity(int worldPosX, int worldPosY, int worldPosZ, float noiseScale)
    {
        return worldPosY - Noise.GetPerlin(worldPosX / noiseScale, worldPosZ / noiseScale).Map(-1, 1, 0, 1) * 10 - 10;
    }

    public float FlatPlane(int y, float height)
    {
        return y - height + 0.5f;
    }

    public float Union(float d1, float d2)
    {
        return Utils.Min(d1, d2);
    }

    public float Subtract(float d1, float d2)
    {
        return Utils.Max(-d1, d2);
    }

    public float Intersection(float d1, float d2)
    {
        return Utils.Max(d1, d2);
    }
}
