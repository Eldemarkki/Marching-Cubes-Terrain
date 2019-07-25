using UnityEngine;

namespace MarchingCubes.Examples.DensityFunctions
{
    public abstract class DensityFunction : ScriptableObject
    {
        public abstract float CalculateDensity(int x, int y, int z);
    }
}