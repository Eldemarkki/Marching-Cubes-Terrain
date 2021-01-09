using System.Runtime.CompilerServices;
using Eldemarkki.VoxelTerrain.Settings;
using Unity.Burst;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// A procedural terrain voxel data calculation job
    /// </summary>
    [BurstCompile]
    public struct ProceduralTerrainVoxelDataCalculationJob : IVoxelDataGenerationJob
    {
        /// <summary>
        /// The procedural terrain generation settings
        /// </summary>
        public ProceduralTerrainSettings ProceduralTerrainSettings { get; set; }

        /// <inheritdoc/>
        public int3 WorldPositionOffset { get; set; }

        /// <inheritdoc/>
        public VoxelDataVolume<byte> OutputVoxelData { get; set; }

        /// <summary>
        /// The execute method required for Unity's IJobParallelFor job type
        /// </summary>
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
                        OutputVoxelData.SetVoxelData((byte)(math.saturate(voxelData) * 255), new int3(x, y, z));
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