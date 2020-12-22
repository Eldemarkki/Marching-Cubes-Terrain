using System.Collections.Generic;
using Eldemarkki.VoxelTerrain.Chunks;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.Utilities.Intersection;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A procedurally generated world
    /// </summary>
    public class ProceduralWorldGenerator : MonoBehaviour
    {
        /// <summary>
        /// The voxel world that "owns" this world generator
        /// </summary>
        [SerializeField] private VoxelWorld voxelWorld;

        /// <summary>
        /// A chunk provider which provides chunks with procedurally generated data
        /// </summary>
        [SerializeField] private ProceduralChunkProvider chunkProvider;

        /// <summary>
        /// The radius of the chunks the player sees
        /// </summary>
        [SerializeField] private int renderDistance = 5;

        /// <summary>
        /// The size of the buffer around the render distance where voxel data will start to generate, but the mesh will not be generated yet
        /// </summary>
        [SerializeField] private int loadingBufferSize = 2;

        /// <summary>
        /// The viewer which the terrain is generated around
        /// </summary>
        [SerializeField] private Transform player;

        /// <summary>
        /// The coordinate of the chunk where terrain was last generated around
        /// </summary>
        private int3 _lastGenerationCoordinate;

        private void Start()
        {
            int3 playerCoordinate = GetPlayerCoordinate();
            GenerateTerrainAroundCoordinate(playerCoordinate);
        }

        private void Update()
        {
            int3 newPlayerCoordinate = GetPlayerCoordinate();
            if (!newPlayerCoordinate.Equals(_lastGenerationCoordinate))
            {
                var newlyFreedCoordinates = voxelWorld.ChunkStore.GetChunkCoordinatesOutsideOfRenderDistance(newPlayerCoordinate, renderDistance);

                int3 renderSize = new int3(renderDistance * 2 + 1);

                int3 oldPos = _lastGenerationCoordinate - new int3(renderDistance);
                BoundsInt oldCoords = new BoundsInt(oldPos.ToVectorInt(), renderSize.ToVectorInt());

                int3 newPos = newPlayerCoordinate - new int3(renderDistance);
                BoundsInt newCoords = new BoundsInt(newPos.ToVectorInt(), renderSize.ToVectorInt());

                int3[] coordinatesThatNeedChunks = GetCoordinatesThatNeedChunks(oldCoords, newCoords);

                int i = 0;
                foreach (int3 source in newlyFreedCoordinates)
                {
                    int3 target = coordinatesThatNeedChunks[i];

                    // Move chunk gameobjects
                    voxelWorld.ChunkStore.MoveChunk(source, target);

                    // Move voxel data and generate new
                    voxelWorld.VoxelDataStore.MoveChunk(source, target);

                    // Move colors and generate new
                    voxelWorld.VoxelColorStore.MoveChunk(source, target);

                    chunkProvider.AddChunkToGenerationQueue(target);

                    i++;
                }

                _lastGenerationCoordinate = newPlayerCoordinate;
            }
        }

        /// <summary>
        /// Get's the current coordinate of <see cref="player"/>
        /// </summary>
        /// <returns>The coordinate of <see cref="player"/></returns>
        private int3 GetPlayerCoordinate()
        {
            return VectorUtilities.WorldPositionToCoordinate(player.position, voxelWorld.WorldSettings.ChunkSize);
        }

        /// <summary>
        /// Generates terrain around <paramref name="coordinate"/> with a radius of <see cref="renderDistance"/>
        /// </summary>
        /// <param name="coordinate">The coordinate to generate the terrain around</param>
        private void GenerateTerrainAroundCoordinate(int3 coordinate)
        {
            // Start generating voxel data for chunks with radius 'renderDistance + additionalLoadSize'
            foreach (int3 loadingCoordinate in GetLoadingCoordinates(coordinate, renderDistance, loadingBufferSize))
            {
                voxelWorld.ChunkUpdater.StartGeneratingData(loadingCoordinate);
            }

            // Generate chunks with radius 'renderDistance'
            int3[] chunkGenerationCoordinates = GetChunkGenerationCoordinates(coordinate, renderDistance);
            for (int i = 0; i < chunkGenerationCoordinates.Length; i++)
            {
                chunkProvider.EnsureChunkExistsAtCoordinate(chunkGenerationCoordinates[i]);
            }

            _lastGenerationCoordinate = coordinate;
        }

        /// <summary>
        /// Calculates the chunk coordinates that should be present when the viewer is in <paramref name="centerChunkCoordinate"/>.
        /// </summary>
        /// <param name="centerChunkCoordinate">The chunk coordinate to generate the coordinates around</param>
        /// <param name="renderDistance">The radius the chunks the player sees</param>
        /// <returns>A collection of coordinates that should be generated</returns>
        private static int3[] GetChunkGenerationCoordinates(int3 centerChunkCoordinate, int renderDistance)
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

        private static int3[] GetCoordinatesThatNeedChunks(BoundsInt oldChunks, BoundsInt newChunks)
        {
            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(oldChunks, newChunks);
            int count = newChunks.CalculateVolume() - intersection.CalculateVolume();
            int3[] coordinates = new int3[count];

            // Cache the min/max values because accessing them repeatedly in a loop is surprisingly costly
            var intersectionMinX = intersection.xMin;
            var intersectionMinY = intersection.yMin;
            var intersectionMinZ = intersection.zMin;

            var intersectionMaxX = intersection.xMax;
            var intersectionMaxY = intersection.yMax;
            var intersectionMaxZ = intersection.zMax;

            int newChunksMinX = newChunks.xMin;
            int newChunksMaxX = newChunks.xMax;
            int newChunksMinY = newChunks.yMin;
            int newChunksMaxY = newChunks.yMax;
            int newChunksMinZ = newChunks.zMin;
            int newChunksMaxZ = newChunks.zMax;

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

            return coordinates;
        }

        /// <summary>
        /// Gets the coordinates of the chunks whose voxel data should be generated. The coordinates are in a cubical shape, with the inside of the cube being empty; generates the coordinates of the outer part of the cube
        /// </summary>
        /// <param name="coordinate">The point which the coordinates should be generated around</param>
        /// <param name="renderDistance">The radius of the chunks the player sees; the inner radius</param>
        /// <param name="loadingBufferSize">The size of the outer layer</param>
        /// <returns>The coordinates for the outer parts of the cube</returns>
        private static IEnumerable<int3> GetLoadingCoordinates(int3 coordinate, int renderDistance, int loadingBufferSize)
        {
            int3 min = -new int3(renderDistance + loadingBufferSize);
            int3 max = new int3(renderDistance + loadingBufferSize);

            int3 innerMin = -new int3(renderDistance);
            int3 innerMax = new int3(renderDistance);

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

                        yield return new int3(x, y, z) + coordinate;
                    }
                }
            }
        }
    }
}