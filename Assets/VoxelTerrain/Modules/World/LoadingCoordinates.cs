using System.Collections.Generic;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A helper struct for using the coordinates of "an outer part of the cube of chunks that should be rendered". Think of this as an outline to a cube.
    /// </summary>
    public struct LoadingCoordinates
    {
        /// <summary>
        /// The central coordinate
        /// </summary>
        private int3 centerCoordinate;

        /// <summary>
        /// The size of the inner cube
        /// </summary>
        private int innerSize;

        /// <summary>
        /// The thickness between the outer cube and the inner cube; Think of this as the thickness of the imaginary outline
        /// </summary>
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

        /// <summary>
        /// Checks if these coordinates contain point <paramref name="point"/>
        /// </summary>
        /// <param name="point">The point to check for</param>
        /// <returns>Returns true if <paramref name="point"/> is contained in these coordinates, false otherwise</returns>
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

        /// <summary>
        /// Gets the coordinates that this <see cref="LoadingCoordinates"/> has, but that <paramref name="except"/> does not have
        /// </summary>
        /// <param name="except">The coordinates to exclude from these coordinates</param>
        /// <returns>Returns all these coordinates except for the ones that are contained in <paramref name="except"/></returns>
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