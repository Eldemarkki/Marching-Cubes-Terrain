using Eldemarkki.VoxelTerrain.Utilities.Intersection;
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
        public static int3[] GetChunkCoordinatesContainingPoint(int3 worldPosition, int3 chunkSize)
        {
            int3 localPosition = VectorUtilities.Mod(worldPosition, chunkSize);

            int chunkCheckCountX = localPosition.x == 0 ? 1 : 0;
            int chunkCheckCountY = localPosition.y == 0 ? 1 : 0;
            int chunkCheckCountZ = localPosition.z == 0 ? 1 : 0;

            // Each "1" in chunkCheckCount(x/y/z) will double the chunk count (= move the bit one place to the left), so calculate how many doublings have to be done and then do that many
            int chunkCount = 1 << (chunkCheckCountX + chunkCheckCountY + chunkCheckCountZ);
            int3[] chunkCoordinates = new int3[chunkCount];

            // This is world position converted to a chunk coordinate
            int3 origin = VectorUtilities.WorldPositionToCoordinate(worldPosition, chunkSize);

            int addedIndex = 0;
            for (int i = 0; i < 8; i++)
            {
                var cornerOffset = LookupTables.CubeCorners[i];
                if (cornerOffset.x <= chunkCheckCountX && cornerOffset.y <= chunkCheckCountY && cornerOffset.z <= chunkCheckCountZ)
                {
                    chunkCoordinates[addedIndex] = origin - cornerOffset;
                    addedIndex++;
                }
            }

            return chunkCoordinates;
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

            int oldChunksMinX = oldChunks.xMin;
            int oldChunksMaxX = oldChunks.xMax;
            int oldChunksMinY = oldChunks.yMin;
            int oldChunksMaxY = oldChunks.yMax;
            int oldChunksMinZ = oldChunks.zMin;
            int oldChunksMaxZ = oldChunks.zMax;

            int count = newChunks.CalculateVolume();

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(oldChunks, newChunks);
            if(math.all(intersection.size.ToInt3() > 0))
            {
                count -= intersection.CalculateVolume();
            }

            int3[] coordinates = new int3[count];

            int i = 0;
            for (int x = newChunksMinX; x < newChunksMaxX; x++)
            {
                for (int y = newChunksMinY; y < newChunksMaxY; y++)
                {
                    for (int z = newChunksMinZ; z < newChunksMaxZ; z++)
                    {
                        if (oldChunksMinX <= x && x < oldChunksMaxX &&
                            oldChunksMinY <= y && y < oldChunksMaxY &&
                            oldChunksMinZ <= z && z < oldChunksMaxZ)
                        {
                            continue;
                        }

                        coordinates[i] = new int3(x, y, z);
                        i++;
                    }
                }
            }

            return coordinates;
        }
    }
}