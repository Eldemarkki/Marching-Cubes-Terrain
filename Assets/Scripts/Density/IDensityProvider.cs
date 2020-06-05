using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Density
{
    /// <summary>
    /// An interface for setting and getting densities
    /// </summary>
    public interface IDensityProvider
    {
        /// <summary>
        /// Gets the density at a world position
        /// </summary>
        /// <param name="worldPosition">The world position of the density to get</param>
        /// <returns>The density at that world position</returns>
        float GetDensity(int3 worldPosition);

        /// <summary>
        /// Sets the density at a world position
        /// </summary>
        /// <param name="density">The new density</param>
        /// <param name="worldPosition">The density's world position</param>
        void SetDensity(float density, int3 worldPosition);
    }
}