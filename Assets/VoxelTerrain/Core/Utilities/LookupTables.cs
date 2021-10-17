using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public static class LookupTables
    {
        public static readonly int3[] CubeCorners =
        {
            new int3(0, 0, 0),
            new int3(1, 0, 0),
            new int3(1, 0, 1),
            new int3(0, 0, 1),
            new int3(0, 1, 0),
            new int3(1, 1, 0),
            new int3(1, 1, 1),
            new int3(0, 1, 1)
        };
    }
}