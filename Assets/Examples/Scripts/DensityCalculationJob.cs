using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace MarchingCubes.Examples.DensityFunctions
{
    [BurstCompile]
    struct DensityCalculationJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<float> densities;

        public int offsetX, offsetY, offsetZ;
        public int chunkSize;

        public void Execute(int index)
        {
            int x = (index / (chunkSize * chunkSize)) + offsetX;
            int y = (index / chunkSize % chunkSize) + offsetY;
            int z = (index % chunkSize) + offsetZ;
            
            densities[index] = y - noise.snoise(new float2(x, z)/10f) - 5 + 0.5f;
        }
    }
}