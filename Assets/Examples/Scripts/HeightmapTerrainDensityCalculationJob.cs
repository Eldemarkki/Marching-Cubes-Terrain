using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// A heightmap terrain density calculation job
    /// </summary>
    [BurstCompile]
    struct HeightmapTerrainDensityCalculationJob : IDensityCalculationJob
    {
        /// <summary>
        /// The output densities
        /// </summary>
        [WriteOnly] private NativeArray<float> densities;

        /// <summary>
        /// The height data from the heightmap
        /// </summary>
        [ReadOnly] public NativeArray<float> heightmapData;

        /// <summary>
        /// Offset the sampling point
        /// </summary>
        [ReadOnly] public int3 offset;

        /// <summary>
        /// The chunk size
        /// </summary>
        [ReadOnly] public int chunkSize;

        /// <summary>
        /// How wide the heightmap is (in pixels). 1 pixel = 1 Unity unit
        /// </summary>
        [ReadOnly] public int heightmapWidth;

        /// <summary>
        /// How high the heightmap is (in pixels). 1 pixel = 1 Unity unit
        /// </summary>
        [ReadOnly] public int heightmapHeight;

        /// <summary>
        /// The value to multiply the height with
        /// </summary>
        [ReadOnly] public float amplitude;

        /// <summary>
        /// The offset to move the sampling point up and down
        /// </summary>
        [ReadOnly] public float heightOffset;

        /// <summary>
        /// The chunk's density field
        /// </summary>
        public NativeArray<float> Densities
        {
            get => densities;
            set => densities = value;
        }

        /// <summary>
        /// The execute method required for Unity's IJobParallelFor job type
        /// </summary>
        /// <param name="index">The iteration index provided by Unity's Job System</param>
        public void Execute(int index)
        {
            int worldPositionX = index / (chunkSize * chunkSize) + offset.x;
            int worldPositionY = index / chunkSize % chunkSize + offset.y;
            int worldPositionZ = index % chunkSize + offset.z;

            float density = 0;
            if (worldPositionX < heightmapWidth && worldPositionZ < heightmapHeight)
            {
                density = CalculateDensity(worldPositionX, worldPositionY, worldPositionZ);
            }

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
            float heightmapValue = heightmapData[worldPositionX + worldPositionZ * heightmapWidth];
            float h = amplitude * heightmapValue;
            return worldPositionY - h - heightOffset;
        }
    }
}