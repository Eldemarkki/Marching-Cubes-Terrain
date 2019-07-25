using UnityEngine;

namespace MarchingCubes.Examples.DensityFunctions
{
    [CreateAssetMenu(fileName = "New Terrain Density Function", menuName = "Density Functions/Terrain Density Function")]
    public class TerrainDensity : InitializedDensityFunction
    {
        public float groundLevel;
        public float heightScale;
        public float noiseScale;
        public int seed;

        private FastNoise noise;

        public override void Initialize()
        {
            noise = new FastNoise(seed);
        }

        public override float CalculateDensity(int x, int y, int z)
        {
            return y - noise.GetPerlin(x / noiseScale, z / noiseScale).Map(-1, 1, 0, 1) * heightScale - groundLevel + 0.5f;
        }
    }
}