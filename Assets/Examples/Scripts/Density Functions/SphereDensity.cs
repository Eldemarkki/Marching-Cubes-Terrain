using UnityEngine;

namespace MarchingCubes.Examples.DensityFunctions
{
    [CreateAssetMenu(fileName = "New Sphere Density Function", menuName = "Density Functions/Sphere Density Function")]
    public class SphereDensity : DensityFunction
    {
        [SerializeField] private Vector3 center;
        [SerializeField] private float radius;

        public override float CalculateDensity(float x, float y, float z)
        {
            float newX = x - center.x;
            float newY = y - center.y;
            float newZ = z - center.z;

            return newX * newX + newY * newY + newZ * newZ - radius * radius;
        }
    }
}