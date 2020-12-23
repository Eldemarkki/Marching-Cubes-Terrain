using System.Collections.Generic;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.World
{
    public struct LoadingCoordinates
    {
        private int3 centerCoordinate;
        private int innerSize;
        private int outerSize;

        /// <summary>
        /// Gets the coordinates of the chunks whose voxel data should be generated. The coordinates are in a cubical shape, with the inside of the cube being empty; generates the coordinates of the outer part of the cube
        /// </summary>
        /// <param name="centerCoordinate">The point which the coordinates should be generated around</param>
        /// <param name="innerSize">The radius of the chunks the player sees; the inner radius</param>
        /// <param name="outerSize">The size of the outer layer</param>
        public LoadingCoordinates(int3 centerCoordinate, int innerSize, int outerSize)
        {
            this.centerCoordinate = centerCoordinate;
            this.innerSize = innerSize;
            this.outerSize = outerSize;
        }

        /// <summary>
        /// Gets the coordinates of the chunks whose voxel data should be generated. The coordinates are in a cubical shape, with the inside of the cube being empty; generates the coordinates of the outer part of the cube
        /// </summary>
        /// <returns>The coordinates for the outer parts of the cube</returns>
        public IEnumerable<int3> GetCoordinates()
        {
            int3 min = -new int3(innerSize + outerSize);
            int3 max = new int3(innerSize + outerSize);

            int3 innerMin = -new int3(innerSize);
            int3 innerMax = new int3(innerSize);

            for (int x = min.x; x <= max.x; x++)
            {
                for (int y = min.y; y <= max.y; y++)
                {
                    for (int z = min.z; z <= max.z; z++)
                    {
                        if (innerMin.x <= x && x <= innerMax.x &&
                            innerMin.y <= y && y <= innerMax.y &&
                            innerMin.z <= z && z <= innerMax.z)
                        {
                            continue;
                        }

                        yield return new int3(x, y, z) + centerCoordinate;
                    }
                }
            }
        }

        public bool Contains(int3 point)
        {
            // Bigger cube should contain, and the smaller one should not contain
            int3 smallCubeMin = centerCoordinate - new int3(innerSize);
            int3 smallCubeMax = centerCoordinate + new int3(innerSize);

            int3 bigCubeMin = centerCoordinate - new int3(innerSize + outerSize);
            int3 bigCubeMax = centerCoordinate + new int3(innerSize + outerSize);

            bool smallCubeContains = smallCubeMin.x <= point.x && point.x <= smallCubeMax.x &&
                                     smallCubeMin.y <= point.y && point.y <= smallCubeMax.y &&
                                     smallCubeMin.z <= point.z && point.z <= smallCubeMax.z;

            bool bigCubeContains = bigCubeMin.x <= point.x && point.x <= bigCubeMax.x &&
                                   bigCubeMin.y <= point.y && point.y <= bigCubeMax.y &&
                                   bigCubeMin.z <= point.z && point.z <= bigCubeMax.z;

            return !smallCubeContains && bigCubeContains;
        }

        public IEnumerable<int3> GetCoordinatesExcept(LoadingCoordinates except)
        {
            foreach(int3 point in GetCoordinates())
            {
                if (!except.Contains(point))
                {
                    yield return point;
                }
            }
        }
    }
}
