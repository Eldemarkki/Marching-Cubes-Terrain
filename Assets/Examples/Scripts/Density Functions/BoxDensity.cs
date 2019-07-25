using UnityEngine;

namespace MarchingCubes.Examples.DensityFunctions
{
    // Taken from http://iquilezles.org/www/articles/distfunctions/distfunctions.htm

    [CreateAssetMenu(fileName = "New Box Density Function", menuName = "Density Functions/Box Density Function")]
    public class BoxDensity : DensityFunction
    {
        public Vector3 center;
        public Vector3 size;

        public override float CalculateDensity(int x, int y, int z)
        {
            Vector3 p = new Vector3(x, y, z) - center;
            Vector3 d = p.Abs() - size;
            
            return Vector3.Max(d, Vector3.zero).magnitude + 
                   Utils.Min(Utils.Max(d.x, Utils.Max(d.y, d.z)), 0);
        }
    }
}