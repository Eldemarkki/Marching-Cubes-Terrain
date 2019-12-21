using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MarchingCubes.Examples
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
            int worldPositionX = (index / (chunkSize * chunkSize)) + xOffset;
            int worldPositionY = (index / chunkSize % chunkSize) + yOffset;
            int worldPositionZ = (index % chunkSize) + zOffset;

            float density = CalculateDensity(worldPositionX, worldPositionY, worldPositionZ);

            densities[index] = density;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float CalculateDensity(int worldPositionX, int worldPositionY, int worldPositionZ)
        {
            return worldPositionY - OctaveNoise(worldPositionX, worldPositionZ, terrainSettings.NoiseFrequency * 0.001f, terrainSettings.NoiseOctaveCount) * terrainSettings.Amplitude - terrainSettings.HeightOffset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private float OctaveNoise(float x, float y, float frequency, int octaveCount){
            float value = 0;

            for (int i = 0; i < octaveCount; i++)
            {
                int octaveModifier = (int)math.pow(2, i);

                // (x+1)/2 because noise.snoise returns a value from -1 to 1 so it needs to be scaled to go from 0 to 1.
                float pureNoise = (noise.snoise(new float2(octaveModifier * x * frequency, octaveModifier * y * frequency)) + 1) / 2f;
                value += pureNoise / octaveModifier;
            }

            return value;
        }

        public bool Equals(DensityCalculationJob other)
        {
            return densities == other.densities && xOffset == other.xOffset && yOffset == other.yOffset && zOffset == other.zOffset && chunkSize == other.chunkSize && terrainSettings.Equals(other.terrainSettings);
        }
    }
}