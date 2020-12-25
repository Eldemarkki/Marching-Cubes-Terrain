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
                MoveVoxelData(_lastGenerationCoordinate, newPlayerCoordinate);

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

                    // Move colors and generate new
                    voxelWorld.VoxelColorStore.MoveChunk(source, target);

                    chunkProvider.AddChunkToGenerationQueue(target);

                    i++;
                }

                _lastGenerationCoordinate = newPlayerCoordinate;
            }
        }

        /// <summary>
        /// Moves all of the voxel data that existed when the player was at the coordinate <paramref name="playerFromCoordinate"/> to the coordinates that don't yet have data but should have when the player is at coordinate <paramref name="playerToCoordinate"/>
        /// </summary>
        /// <param name="playerFromCoordinate">The old coordinate of the player</param>
        /// <param name="playerToCoordinate">The new coordinate of the player</param>
        private void MoveVoxelData(int3 playerFromCoordinate, int3 playerToCoordinate)
        {
            int range = renderDistance + loadingBufferSize;
            int3 renderSize = new int3(range * 2 + 1);

            int3 oldPos = playerFromCoordinate - new int3(range);
            BoundsInt oldCoords = new BoundsInt(oldPos.ToVectorInt(), renderSize.ToVectorInt());

            int3 newPos = playerToCoordinate - new int3(range);
            BoundsInt newCoords = new BoundsInt(newPos.ToVectorInt(), renderSize.ToVectorInt());

            int3[] coordinatesThatNeedData = GetCoordinatesThatNeedChunks(oldCoords, newCoords);

            var newlyFreedCoordinates = voxelWorld.VoxelDataStore.GetChunkCoordinatesOutsideOfRange(playerToCoordinate, range);

            int i = 0;
            foreach (int3 freeCoordinate in newlyFreedCoordinates)
            {
                var targetCoordinate = coordinatesThatNeedData[i];
                voxelWorld.VoxelDataStore.MoveChunk(freeCoordinate, targetCoordinate);
                i++;
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
            LoadingCoordinates loadingCoordinates = new LoadingCoordinates(coordinate, renderDistance, loadingBufferSize);
            foreach (int3 loadingCoordinate in loadingCoordinates.GetCoordinates())
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
        /// <summary>
        /// Gets the list of new coordinates that should be generated when the player moved from <paramref name="oldChunks"/> to <paramref name="newChunks"/>; Every coordinate in <paramref name="newChunks"/> that is not in <paramref name="oldChunks"/>
        /// </summary>
        /// <param name="oldChunks">The old rendering bounds of the chunks the player saw</param>
        /// <param name="newChunks">The new rendering bounds of the chunks the player sees</param>
        /// <returns>Returns the new coordinates that need chunks</returns>
        private static int3[] GetCoordinatesThatNeedChunks(BoundsInt oldChunks, BoundsInt newChunks)
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