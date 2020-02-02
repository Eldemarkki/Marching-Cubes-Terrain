using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// A procedural terrain density calculation job
    /// </summary>
    [BurstCompile]
    struct ProceduralTerrainDensityCalculationJob : IDensityCalculationJob
    {
        /// <summary>
        /// The output densities
        /// </summary>
        [WriteOnly] private NativeArray<float> densities;

        /// <summary>
        /// The sampling point's offset
        /// </summary>
        [ReadOnly] public int3 offset;
        
        /// <summary>
        /// The chunk size
        /// </summary>
        [ReadOnly] public int chunkSize;

        /// <summary>
        /// The procedural terrain generation settings
        /// </summary>
        [ReadOnly] public ProceduralTerrainSettings proceduralTerrainSettings;

        /// <summary>
        /// The chunk's density field
        /// </summary>
        public NativeArray<float> Densities { get => densities; set => densities = value; }

        /// <summary>
        /// The execute method required for Unity's IJobParallelFor job type
        /// </summary>
        /// <param name="index">The iteration index provided by Unity's Job System</param>
        public void Execute(int index)
        {
            int worldPositionX = index / (chunkSize * chunkSize) + offset.x;
            int worldPositionY = index / chunkSize % chunkSize + offset.y;
            int worldPositionZ = index % chunkSize + offset.z;

            float density = CalculateDensity(worldPositionX, worldPositionY, worldPositionZ);
            densities[index] = math.clamp(density, -1, 1);
        }

        /// <summary>
        /// Calculates the density at the world-space position
        /// </summary>
        /// <param name="worldPositionX">Sampling point's world-space x position</param>
        /// <param name="worldPositionY">Sampling point's world-space y position</param>
        /// <param name="worldPositionZ">Sampling point's world-space z position</param>
        /// <returns>The density sampled from the world-space position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateDensity(int worldPositionX, int worldPositionY, int worldPositionZ)
        {
            return worldPositionY - OctaveNoise(worldPositionX, worldPositionZ, proceduralTerrainSettings.NoiseFrequency * 0.001f, proceduralTerrainSettings.NoiseOctaveCount) * proceduralTerrainSettings.Amplitude - proceduralTerrainSettings.HeightOffset;
        }

        /// <summary>
        /// Calculates octave noise
        /// </summary>
        /// <param name="x">Sampling point's x position</param>
        /// <param name="y">Sampling point's y position</param>
        /// <param name="frequency">The frequency of the noise</param>
        /// <param name="octaveCount">How many layers of noise to combine</param>
        /// <returns>The sampled noise value</returns>
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