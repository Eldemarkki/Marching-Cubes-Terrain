using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Eldemarkki.VoxelTerrain.Settings;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    [BurstCompile]
    public struct HeightmapTerrainVoxelDataCalculationJob : IVoxelDataGenerationJob<byte>
    {
        /// <inheritdoc cref="HeightmapTerrainSettings.HeightmapData"/>
        [ReadOnly] private NativeArray<float> _heightmapData;

        /// <inheritdoc cref="_heightmapData"/>
        public NativeArray<float> HeightmapData { get => _heightmapData; set => _heightmapData = value; }

        /// <inheritdoc cref="HeightmapTerrainSettings.Width"/>
        public int HeightmapWidth { get; set; }

        /// <inheritdoc cref="HeightmapTerrainSettings.Height"/>
        public int HeightmapHeight { get; set; }

        /// <inheritdoc cref="HeightmapTerrainSettings.Amplitude"/>
        public float Amplitude { get; set; }

        /// <inheritdoc cref="HeightmapTerrainSettings.HeightOffset"/>
        public float HeightOffset { get; set; }

        /// <inheritdoc/>
        public int3 WorldPositionOffset { get; set; }

        private VoxelDataVolume<byte> _outputVoxelData;
        public VoxelDataVolume<byte> OutputVoxelData { get => _outputVoxelData; set => _outputVoxelData = value; }

        public void Execute()
        {
            for (int z = 0; z < OutputVoxelData.Depth; z++)
            {
                for (int y = 0; y < OutputVoxelData.Height; y++)
                {
                    for (int x = 0; x < OutputVoxelData.Width; x++)
                    {
                        int worldPositionX = x + WorldPositionOffset.x;
                        int worldPositionY = y + WorldPositionOffset.y;
                        int worldPositionZ = z + WorldPositionOffset.z;

                        bool isInsideOfMap = worldPositionX < HeightmapWidth && worldPositionZ < HeightmapHeight;

                        // Default is 1, because the default voxel data should be air
                        float voxelData = math.select(1, CalculateVoxelData(worldPositionX, worldPositionY, worldPositionZ), isInsideOfMap);

                        _outputVoxelData[x, y, z] = (byte)(math.saturate(voxelData) * byte.MaxValue);
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the voxel data at the world-space position
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateVoxelData(int worldPositionX, int worldPositionY, int worldPositionZ)
        {
            float heightmapValue = _heightmapData[worldPositionX + worldPositionZ * HeightmapWidth];
            float h = Amplitude * heightmapValue;
            return worldPositionY - h - HeightOffset;
        }
    }
}