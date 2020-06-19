using System.Runtime.CompilerServices;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Density
{
    /// <summary>
    /// A heightmap terrain density calculation job
    /// </summary>
    [BurstCompile]
    struct HeightmapTerrainDensityCalculationJob : VoxelDataGenerationJob
    {
        /// <summary>
        /// The output densities
        /// </summary>
        [WriteOnly] private DensityVolume _voxelData;

        /// <summary>
        /// The height data from the heightmap
        /// </summary>
        [ReadOnly] public NativeArray<float> heightmapData;

        [ReadOnly] private int3 _worldPositionOffset;

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

        public int3 WorldPositionOffset { get => _worldPositionOffset; set => _worldPositionOffset = value; }
        public DensityVolume OutputVoxelData { get => _voxelData; set => _voxelData = value; }

        /// <summary>
        /// The execute method required for Unity's IJobParallelFor job type
        /// </summary>
        /// <param name="index">The iteration index provided by Unity's Job System</param>
        public void Execute(int index)
        {
            int3 worldPosition = IndexUtilities.IndexToXyz(index, _voxelData.Width, _voxelData.Height) + _worldPositionOffset;
            int worldPositionX = worldPosition.x;
            int worldPositionY = worldPosition.y;
            int worldPositionZ = worldPosition.z;

            float density = 1; // 1, because the default density should be air
            if (worldPositionX < heightmapWidth && worldPositionZ < heightmapHeight)
            {
                density = CalculateDensity(worldPositionX, worldPositionY, worldPositionZ);
            }

            _voxelData.SetDensity(density, index);
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