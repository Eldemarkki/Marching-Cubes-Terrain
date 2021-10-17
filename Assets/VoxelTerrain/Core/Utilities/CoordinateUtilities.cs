using Eldemarkki.VoxelTerrain.Utilities.Intersection;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public static class CoordinateUtilities
    {
        /// <summary>
        /// Calculates a list of chunk coordinates that contain <paramref name="worldPosition"/>. For a chunk to contain <paramref name="worldPosition"/>, the position has to be inside of the chunk or on the chunk's edge
        /// </summary>
        /// <returns>An array of chunk coordinates that contain <paramref name="worldPosition"/></returns>
        public static int3[] CalculateChunkCoordinatesContainingPoint(int3 worldPosition, int3 chunkSize)
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
        /// Calculates the chunk coordinates that should be visible when the viewer is in <paramref name="centerChunkCoordinate"/>.
        /// </summary>
        /// <param name="centerChunkCoordinate">The chunk coordinate to generate the coordinates around</param>
        /// <param name="renderDistance">The radius the chunks the player sees</param>
        public static int3[] CalculateChunkGenerationCoordinates(int3 centerChunkCoordinate, int renderDistance)
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
        /// Calculates the list of new coordinates that should be generated when the player moved from <paramref name="oldChunks"/> to <paramref name="newChunks"/>, i.e. every coordinate in <paramref name="newChunks"/> that is not in <paramref name="oldChunks"/>
        /// </summary>
        /// <param name="oldChunks">The old rendering bounds of the chunks the player saw</param>
        /// <param name="newChunks">The new rendering bounds of the chunks the player sees</param>
        /// <returns>Returns the new coordinates that need chunks</returns>
        public static int3[] CalculateCoordinatesThatNeedChunks(BoundsInt oldChunks, BoundsInt newChunks)
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

            int count = newChunks.CalculateBoundsVolume();

            BoundsInt intersection = IntersectionUtilities.CalculateIntersectionVolume(oldChunks, newChunks);
            if (math.all(intersection.size.ToInt3() > 0))
            {
                count -= intersection.CalculateBoundsVolume();
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

        /// <summary>
        /// Calculates the coordinates of all chunks which touch <paramref name="worldSpaceBounds"/>
        /// </summary>
        public static int3[] CalculateChunkCoordinatesInsideWorldSpaceBounds(BoundsInt worldSpaceBounds, int3 chunkSize)
        {
            int3 minCoordinate = VectorUtilities.WorldPositionToCoordinate(worldSpaceBounds.min - Vector3Int.one, chunkSize);
            int3 maxCoordinate = VectorUtilities.WorldPositionToCoordinate(worldSpaceBounds.max, chunkSize);

            int chunkCount = CalculateChunkCountInsideCoordinates(minCoordinate, maxCoordinate);
            int3[] chunkCoordinates = new int3[chunkCount];

            int i = 0;
            for (int x = minCoordinate.x; x <= maxCoordinate.x; x++)
            {
                for (int y = minCoordinate.y; y <= maxCoordinate.y; y++)
                {
                    for (int z = minCoordinate.z; z <= maxCoordinate.z; z++)
                    {
                        int3 chunkCoordinate = new int3(x, y, z);
                        chunkCoordinates[i++] = chunkCoordinate;
                    }
                }
            }

            return chunkCoordinates;
        }

        /// <summary>
        /// Calculates how many points there would be in the bounds between a and b, where a and b are the min/max positions of the imaginary bounds
        /// </summary>
        public static int CalculateChunkCountInsideCoordinates(int3 a, int3 b)
        {
            int3 diff = b - a + 1;
            return diff.x * diff.y * diff.z;
        }
    }
}