using UnityEngine;

namespace MarchingCubes.Examples.DensityFunctions
{
    [CreateAssetMenu(fileName = "New Sphere Density Function", menuName = "Density Functions/Sphere Density Function")]
    public class SphereDensity : DensityFunction
    {
        public Vector3 center;
        public float radius;

        public override float CalculateDensity(int x, int y, int z)
        {
            float newX = x - center.x;
            float newY = y - center.y;
            float newZ = z - center.z;

            return newX * newX + newY * newY + newZ * newZ - radius * radius;
        }
    }
}