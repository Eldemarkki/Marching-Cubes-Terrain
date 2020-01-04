using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    [BurstCompile]
    struct HeightmapTerrainDensityCalculationJob : IDensityCalculationJob
    {
        [WriteOnly] private NativeArray<float> densities;

        [ReadOnly] public NativeArray<float> heightmapData;
        [ReadOnly] public int3 offset;
        [ReadOnly] public int chunkSize;
        [ReadOnly] public int heightmapWidth;
        [ReadOnly] public int heightmapHeight;
        [ReadOnly] public float amplitude;
        [ReadOnly] public float heightOffset;

        public NativeArray<float> Densities { get => densities; set => densities = value; }

        public void Execute(int index)
        {
            int worldPositionX = index / (chunkSize * chunkSize) + offset.x;
            int worldPositionY = index / chunkSize % chunkSize + offset.y;
            int worldPositionZ = index % chunkSize + offset.z;

            float density = 0;
            if(worldPositionX < heightmapWidth && worldPositionZ < heightmapHeight)
                density = CalculateDensity(worldPositionX, worldPositionY, worldPositionZ);

            densities[index] = math.clamp(density, -1, 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float CalculateDensity(int worldPositionX, int worldPositionY, int worldPositionZ)
        {
            float heightmapValue = heightmapData[worldPositionX + worldPositionZ * heightmapWidth];
            float h = amplitude * heightmapValue;
            return worldPositionY - h - heightOffset;
        }
    }
}