using Eldemarkki.VoxelTerrain.Utilities;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// A heightmap terrain voxel data calculation job
    /// </summary>
    [BurstCompile]
    public struct HeightmapTerrainVoxelDataCalculationJob : IVoxelDataGenerationJob
    {
        /// <summary>
        /// The height data from the heightmap
        /// </summary>
        [ReadOnly] private NativeArray<float> _heightmapData;

        /// <summary>
        /// How wide the heightmap is (in pixels). 1 pixel = 1 Unity unit
        /// </summary>
        public int HeightmapWidth { get; set; }

        /// <summary>
        /// How high the heightmap is (in pixels). 1 pixel = 1 Unity unit
        /// </summary>
        public int HeightmapHeight { get; set; }

        /// <summary>
        /// The value to multiply the height with
        /// </summary>
        public float Amplitude { get; set; }

        /// <summary>
        /// The offset to move the sampling point up and down
        /// </summary>
        public float HeightOffset { get; set; }

        /// <inheritdoc/>
        public int3 WorldPositionOffset { get; set; }

        /// <inheritdoc/>
        public VoxelDataVolume<byte> OutputVoxelData { get; set; }

        /// <summary>
        /// The height data from the heightmap
        /// </summary>
        public NativeArray<float> HeightmapData { get => _heightmapData; set => _heightmapData = value; }

        /// <summary>
        /// The execute method required for Unity's IJobParallelFor job type
        /// </summary>
        public void Execute()
        {
            int index = 0;
            for (int z = 0; z < OutputVoxelData.Depth; z++)
            {
                for (int y = 0; y < OutputVoxelData.Height; y++)
                {
                    for (int x = 0; x < OutputVoxelData.Width; x++)
                    {
                        int3 worldPosition = new int3(x, y, z) + WorldPositionOffset;
                        int worldPositionX = worldPosition.x;
                        int worldPositionY = worldPosition.y;
                        int worldPositionZ = worldPosition.z;

                        float voxelData = 1; // 1, because the default voxel data should be air
                        if (worldPositionX < HeightmapWidth && worldPositionZ < HeightmapHeight)
                        {
                            voxelData = CalculateVoxelData(worldPositionX, worldPositionY, worldPositionZ);
                        }

                        OutputVoxelData.SetVoxelData((byte)math.clamp(voxelData * 255, 0, 255), index);
                        index++;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the voxel data at the world-space position
        /// </summary>
        /// <param name="worldPositionX">Sampling point's world-space x position</param>
        /// <param name="worldPositionY">Sampling point's world-space y position</param>
        /// <param name="worldPositionZ">Sampling point's world-space z position</param>
        /// <returns>The voxel data sampled from the world-space position</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateVoxelData(int worldPositionX, int worldPositionY, int worldPositionZ)
        {
            float heightmapValue = _heightmapData[worldPositionX + worldPositionZ * HeightmapWidth];
            float h = Amplitude * heightmapValue;
            return worldPositionY - h - HeightOffset;
        }
    }
}