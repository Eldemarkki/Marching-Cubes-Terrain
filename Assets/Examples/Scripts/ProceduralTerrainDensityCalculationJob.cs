using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    [BurstCompile]
    struct ProceduralTerrainDensityCalculationJob : IDensityCalculationJob
    {
        [WriteOnly] private NativeArray<float> densities;

        [ReadOnly] public int3 offset;
        [ReadOnly] public int chunkSize;

        [ReadOnly] public ProceduralTerrainSettings proceduralTerrainSettings;

        public NativeArray<float> Densities { get => densities; set => densities = value; }

        public void Execute(int index)
        {
            int worldPositionX = (index / (chunkSize * chunkSize)) + offset.x;
            int worldPositionY = (index / chunkSize % chunkSize) + offset.y;
            int worldPositionZ = (index % chunkSize) + offset.z;

            float density = CalculateDensity(worldPositionX, worldPositionY, worldPositionZ);
            densities[index] = math.clamp(density, -1, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateDensity(int worldPositionX, int worldPositionY, int worldPositionZ)
        {
            return worldPositionY - OctaveNoise(worldPositionX, worldPositionZ, proceduralTerrainSettings.NoiseFrequency * 0.001f, proceduralTerrainSettings.NoiseOctaveCount) * proceduralTerrainSettings.Amplitude - proceduralTerrainSettings.HeightOffset;
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
    }
}