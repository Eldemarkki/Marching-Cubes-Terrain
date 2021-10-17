using Eldemarkki.VoxelTerrain.Settings;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    [BurstCompile]
    public struct ProceduralTerrainVoxelDataCalculationJob : IVoxelDataGenerationJob<byte>
    {
        public ProceduralTerrainSettings ProceduralTerrainSettings { get; set; }

        /// <inheritdoc/>
        public int3 WorldPositionOffset { get; set; }

        private VoxelDataVolume<byte> _outputVoxelData;
        public VoxelDataVolume<byte> OutputVoxelData { get => _outputVoxelData; set => _outputVoxelData = value; }

        public void Execute()
        {
            float actualHeightOffset = WorldPositionOffset.y - ProceduralTerrainSettings.HeightOffset;

            for (int x = 0; x < OutputVoxelData.Width; x++)
            {
                for (int z = 0; z < OutputVoxelData.Depth; z++)
                {
                    int2 terrainPosition = new int2(x, z) + WorldPositionOffset.xz;

                    float terrainNoise = OctaveNoise(terrainPosition, ProceduralTerrainSettings.NoiseFrequency * 0.001f, ProceduralTerrainSettings.NoiseOctaveCount, ProceduralTerrainSettings.NoiseSeed) * ProceduralTerrainSettings.Amplitude;

                    float offset = actualHeightOffset - terrainNoise;

                    for (int y = 0; y < OutputVoxelData.Height; y++)
                    {
                        float voxelData = (y + offset) * 0.5f;
                        _outputVoxelData[x, y, z] = (byte)(math.saturate(voxelData) * byte.MaxValue);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates octave noise
        /// </summary>
        /// <param name="position">Sampling point's position</param>
        /// <param name="frequency">The frequency of the noise</param>
        /// <param name="octaveCount">How many layers of noise to combine</param>
        /// <returns>The sampled noise value</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static float OctaveNoise(float2 position, float frequency, int octaveCount, int seed)
        {
            float value = 0;
            float2 scaledPosition = position * frequency;

            for (int i = 0; i < octaveCount; i++)
            {
                int octaveModifier = 1 << i;

                // (x+1)/2 because noise.snoise returns a value from -1 to 1 so it needs to be scaled to go from 0 to 1.
                float pureNoise = (noise.snoise(new float3(scaledPosition * octaveModifier, seed)) + 1) / 2f;
                value += pureNoise / octaveModifier;
            }

            return value;
        }
    }
}