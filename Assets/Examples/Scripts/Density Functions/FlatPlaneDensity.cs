using UnityEngine;

namespace MarchingCubes.Examples.DensityFunctions
{   
    [CreateAssetMenu(fileName = "New Flat Plane Density Function", menuName = "Density Functions/Flat Plane Density Function")]
    public class FlatPlaneDensity : DensityFunction
    {
        [SerializeField] private float groundLevel = 0;
        
        public override float CalculateDensity(int x, int y, int z)
        {
            return y - groundLevel + 0.5f;
        }
    }
}