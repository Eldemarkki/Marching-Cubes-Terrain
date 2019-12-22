using Unity.Collections;
using Unity.Jobs;

namespace MarchingCubes.Examples
{
    public interface IDensityCalculationJob : IJobParallelFor
    {
        NativeArray<float> Densities { get; set; }

        float CalculateDensity(int worldPositionX, int worldPositionY, int worldPositionZ);
    }
}