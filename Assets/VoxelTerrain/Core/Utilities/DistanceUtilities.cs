using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public static class DistanceUtilities
    {
        /// <summary>
        /// Calculates the difference between <paramref name="pointA"/> and <paramref name="pointB"/>, and returns true if any of the xyz components of the difference is greater than <paramref name="maximumAllowed"/>. If all of the xyz components of the difference is less than or equal to <paramref name="maximumAllowed"/> it returns false. The formal name of this distance calculation is Chebyshev distance. This compares if the Chebyshev distance is greater than <paramref name="maximumAllowed"/>
        /// </summary>
        /// <param name="pointA">Point A</param>
        /// <param name="pointB">Point B</param>
        /// <param name="maximumAllowed">Maximum allowed component-wise difference of the difference between <paramref name="pointA"/> and <paramref name="pointB"/></param>
        /// <returns>True if any of the component-wise differences in the difference between <paramref name="pointA"/> and <paramref name="pointB"/> is greater than <paramref name="maximumAllowed"/>, false otherwise</returns>
        public static bool ChebyshevDistanceGreaterThan(int3 pointA, int3 pointB, int maximumAllowed)
        {
            int abx = pointA.x - pointB.x;
            int aby = pointA.y - pointB.y;
            int abz = pointA.z - pointB.z;

            return abx > maximumAllowed || -abx > maximumAllowed || aby > maximumAllowed || -aby > maximumAllowed || abz > maximumAllowed || -abz > maximumAllowed;
        }
    }
}
