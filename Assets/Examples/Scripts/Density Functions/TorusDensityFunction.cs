using UnityEngine;

namespace MarchingCubes.Examples.DensityFunctions
{
    // Taken from http://iquilezles.org/www/articles/distfunctions/distfunctions.htm

    [CreateAssetMenu(fileName = "New Torus Density Function", menuName = "Density Functions/Torus Density Function")]
    public class TorusDensityFunction : DensityFunction
    {
        public Vector3 center;
        public Vector2 size;

        public override float CalculateDensity(int x, int y, int z)
        {
            Vector3 p = new Vector3(x, y, z) - center;
            Vector2 q = new Vector2(new Vector2(p.x, p.z).magnitude - size.x, p.y);
            return q.magnitude - size.y;
        }
    }
}