using Unity.Collections;
using Unity.Jobs;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// An interface for all density calculation jobs
    /// </summary>
    public interface IDensityCalculationJob : IJobParallelFor
    {
        /// <summary>
        /// The output densities
        /// </summary>
        NativeArray<float> Densities { get; set; }

        /// <summary>
        /// Calculates the density at the world-space position
        /// </summary>
        /// <param name="worldPositionX">Sampling point's world-space x position</param>
        /// <param name="worldPositionY">Sampling point's world-space y position</param>
        /// <param name="worldPositionZ">Sampling point's world-space z position</param>
        /// <returns>The density sampled from the world-space position</returns>
        float CalculateDensity(int worldPositionX, int worldPositionY, int worldPositionZ);
    }
}