using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    /// <summary>
    /// A class to hold miscellaneous lookup tables
    /// </summary>
    public static class LookupTables
    {
        /// <summary>
        /// The corners of a voxel
        /// </summary>
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