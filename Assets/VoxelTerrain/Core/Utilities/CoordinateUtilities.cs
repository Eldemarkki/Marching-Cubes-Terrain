using Eldemarkki.VoxelTerrain.Utilities.Intersection;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public static class CoordinateUtilities
    {
        /// <summary>
        /// Gets a collection of chunks that contain a world position. For a chunk to contain a position, the position has to be inside of the chunk or on the chunk's edge
        /// </summary>
        /// <param name="worldPosition">The world position to check</param>
        /// <param name="chunkSize">The size of the chunks</param>
        /// <returns>A collection of chunk coordinates that contain the world position</returns>
        public static IEnumerable<int3> GetChunkCoordinatesContainingPoint(int3 worldPosition, int3 chunkSize)
        {
            int3 localPosition = VectorUtilities.Mod(worldPosition, chunkSize);

            int chunkCheckCountX = localPosition.x == 0 ? 1 : 0;
            int chunkCheckCountY = localPosition.y == 0 ? 1 : 0;
            int chunkCheckCountZ = localPosition.z == 0 ? 1 : 0;

            int3 origin = VectorUtilities.WorldPositionToCoordinate(worldPosition, chunkSize);

            // The origin (worldPosition as a chunk coordinate) is always included
            yield return origin;

            // The first corner can be skipped, since it's (0, 0, 0) and would just return origin
            for (int i = 1; i < 8; i++)
            {
                var cornerOffset = LookupTables.CubeCorners[i];
                if (cornerOffset.x <= chunkCheckCountX && cornerOffset.y <= chunkCheckCountY && cornerOffset.z <= chunkCheckCountZ)
                {
                    yield return origin - cornerOffset;
                }
            }
        }
        
        /// <summary>
         /// Gets the coordinates of the chunks whose voxel data should be generated. The coordinates are in a cubical shape, with the inside of the cube being empty; generates the coordinates of the outer part of the cube
         /// </summary>
         /// <param name="centerCoordinate">The central coordinate</param>
         /// <param name="innerSize">The size of the inner cube</param>
         /// <param name="outerSize">The thickness between the outer cube and the inner cube; Think of this as the thickness of the imaginary outline</param>
         /// <returns>The coordinates for the outer parts of the cube</returns>
        public static int3[] GetPreloadCoordinates(int3 centerCoordinate, int innerSize, int outerSize)
        {
            int3 min = -new int3(innerSize + outerSize);
            int3 max = new int3(innerSize + outerSize);

            int3 innerMin = -new int3(innerSize);
            int3 innerMax = new int3(innerSize);

            int3 fullSize = max - min + 1;
            int fullVolume = fullSize.x * fullSize.y * fullSize.z;

            int3 innerDimensions = innerMax - innerMin + 1;
            int innerVolume = innerDimensions.x * innerDimensions.y * innerDimensions.z;

            int3[] result = new int3[fullVolume - innerVolume];

            int index = 0;
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

                        result[index] = new int3(x, y, z) + centerCoordinate;
                        index++;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates the chunk coordinates that should be present when the viewer is in <paramref name="centerChunkCoordinate"/>.
        /// </summary>
        /// <param name="centerChunkCoordinate">The chunk coordinate to generate the coordinates around</param>
        /// <param name="renderDistance">The radius the chunks the player sees</param>
        /// <returns>A collection of coordinates that should be generated</returns>
        public static int3[] GetChunkGenerationCoordinates(int3 centerChunkCoordinate, int renderDistance)
        {
            int3[] coordinates = new int3[(int)math.pow(renderDistance * 2 + 1, 3)];
            int i = 0;
            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    for (int z = -renderDistance; z <= renderDistance; z++)
                    {
                        int3 chunkCoordinate = centerChunkCoordinate + new int3(x, y, z);
                        coordinates[i] = chunkCoordinate;
                        i++;
                    }
                }
            }

            return coordinates;
        }

        /// <summary>
        /// Gets the list of new coordinates that should be generated when the player moved from <paramref name="oldChunks"/> to <paramref name="newChunks"/>; Every coordinate in <paramref name="newChunks"/> that is not in <paramref name="oldChunks"/>
        /// </summary>
        /// <param name="oldChunks">The old rendering bounds of the chunks the player saw</param>
        /// <param name="newChunks">The new rendering bounds of the chunks the player sees</param>
        /// <returns>Returns the new coordinates that need chunks</returns>
        public static int3[] GetCoordinatesThatNeedChunks(BoundsInt oldChunks, BoundsInt newChunks)
        {
            // Cache the min/max values because accessing them repeatedly in a loop is surprisingly costly
            int newChunksMinX = newChunks.xMin;
            int newChunksMaxX = newChunks.xMax;
            int newChunksMinY = newChunks.yMin;
            int newChunksMaxY = newChunks.yMax;
            int newChunksMinZ = newChunks.zMin;
            int newChunksMaxZ = newChunks.zMax;

            int3[] coordinates;

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(oldChunks, newChunks);
            if (math.any(intersection.size.ToInt3() < int3.zero))
            {
                coordinates = new int3[newChunks.CalculateVolume()];
                int i = 0;
                for (int x = newChunksMinX; x < newChunksMaxX; x++)
                {
                    for (int y = newChunksMinY; y < newChunksMaxY; y++)
                    {
                        for (int z = newChunksMinZ; z < newChunksMaxZ; z++)
                        {
                            coordinates[i] = new int3(x, y, z);
                            i++;
                        }
                    }
                }
            }
            else
            {
                var intersectionMinX = intersection.xMin;
                var intersectionMinY = intersection.yMin;
                var intersectionMinZ = intersection.zMin;

                var intersectionMaxX = intersection.xMax;
                var intersectionMaxY = intersection.yMax;
                var intersectionMaxZ = intersection.zMax;

                int count = newChunks.CalculateVolume() - intersection.CalculateVolume();
                coordinates = new int3[count];

                int i = 0;
                for (int x = newChunksMinX; x < newChunksMaxX; x++)
                {
                    for (int y = newChunksMinY; y < newChunksMaxY; y++)
                    {
                        for (int z = newChunksMinZ; z < newChunksMaxZ; z++)
                        {
                            if (intersectionMinX <= x && x < intersectionMaxX &&
                                intersectionMinY <= y && y < intersectionMaxY &&
                                intersectionMinZ <= z && z < intersectionMaxZ)
                            {
                                continue;
                            }

                            coordinates[i] = new int3(x, y, z);
                            i++;
                        }
                    }
                }
            }

            return coordinates;
        }
    }
}
