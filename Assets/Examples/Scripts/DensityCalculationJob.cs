using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MarchingCubes.Examples.DensityFunctions
{
    [BurstCompile]
    struct DensityCalculationJob : IJobParallelFor, IEquatable<DensityCalculationJob>
    {
        [WriteOnly] public NativeArray<float> densities;

        [ReadOnly] public int xOffset, yOffset, zOffset;
        [ReadOnly] public int chunkSize;

        [ReadOnly] public TerrainSettings terrainSettings;

        public void Execute(int index)
        {
            int x = (index / (chunkSize * chunkSize)) + xOffset;
            int y = (index / chunkSize % chunkSize) + yOffset;
            int z = (index % chunkSize) + zOffset;

            float density = y - OctaveNoise(x, z, terrainSettings.NoiseFrequency, terrainSettings.NoiseOctaveCount) * terrainSettings.Amplitude - terrainSettings.HeightOffset;

            densities[index] = density;
        }

        private float OctaveNoise(float x, float y, float frequency, int octaveCount){
            float value = 0;

            for (int i = 0; i < octaveCount; i++)
            {
                int octaveModifier = (int)math.pow(2, i);
                value += noise.snoise(new float2(octaveModifier * x * frequency, octaveModifier * y * frequency)) / octaveModifier;
            }

            return value;
        }

        public bool Equals(DensityCalculationJob other)
        {
            return densities == other.densities && xOffset == other.xOffset && yOffset == other.yOffset && zOffset == other.zOffset && chunkSize == other.chunkSize && terrainSettings.Equals(other.terrainSettings);
        }
    }
}