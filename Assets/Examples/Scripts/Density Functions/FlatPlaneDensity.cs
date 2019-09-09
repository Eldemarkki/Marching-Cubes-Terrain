using UnityEngine;

namespace MarchingCubes.Examples.DensityFunctions
{   
    [CreateAssetMenu(fileName = "New Flat Plane Density Function", menuName = "Density Functions/Flat Plane Density Function")]
    public class FlatPlaneDensity : DensityFunction
    {
        [SerializeField] private float groundLevel;
        
        public override float CalculateDensity(float x, float y, float z)
        {
            return y - groundLevel + 0.5f;
        }
    }
}