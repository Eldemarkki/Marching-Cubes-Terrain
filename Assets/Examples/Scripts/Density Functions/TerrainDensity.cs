using UnityEngine;

namespace MarchingCubes.Examples.DensityFunctions
{
    [CreateAssetMenu(fileName = "New Terrain Density Function", menuName = "Density Functions/Terrain Density Function")]
    public class TerrainDensity : InitializedDensityFunction
    {
        [SerializeField] private float groundLevel;
        [SerializeField] private float heightScale;
        [SerializeField] private float noiseScale;
        [SerializeField] private int seed;

        private FastNoise _noise;

        public override void Initialize()
        {
            _noise = new FastNoise(seed);
        }

        public override float CalculateDensity(float x, float y, float z)
        {
            return y - _noise.GetPerlin(x / noiseScale, z / noiseScale).Map(-1, 1, 0, 1) * heightScale - groundLevel + 0.5f;
        }
    }
}