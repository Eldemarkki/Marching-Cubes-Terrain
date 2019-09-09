using UnityEngine;

namespace MarchingCubes.Examples.DensityFunctions
{
    public abstract class DensityFunction : ScriptableObject
    {
        public abstract float CalculateDensity(float x, float y, float z);
    }
}