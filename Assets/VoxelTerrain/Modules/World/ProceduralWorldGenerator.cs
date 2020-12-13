using System.Collections.Generic;
using Eldemarkki.VoxelTerrain.Chunks;
using Eldemarkki.VoxelTerrain.Utilities;
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
            int3 playerCoordinate = GetPlayerCoordinate();
            if (!playerCoordinate.Equals(_lastGenerationCoordinate))
            {
                // Hide the chunks that are out of render distance, but preserve their data
                IEnumerable<int3> coordinatesToHide = voxelWorld.ChunkStore.GetChunkCoordinatesOutsideOfRenderDistance(playerCoordinate, renderDistance);
                chunkProvider.HideChunks(coordinatesToHide);

                // Unload data that is outside of 'renderDistance + loadBuffer'
                var coordinatesToUnload = voxelWorld.VoxelDataStore.GetChunkCoordinatesOutsideOfRenderDistance(playerCoordinate, renderDistance + loadingBufferSize);
                
                voxelWorld.VoxelDataStore.UnloadCoordinates(coordinatesToUnload);
                voxelWorld.VoxelColorStore.UnloadCoordinates(coordinatesToUnload);

                // Generate new terrain
                GenerateTerrainAroundCoordinate(playerCoordinate);
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
            foreach (int3 chunkCoordinate in GetChunkGenerationCoordinates(coordinate, renderDistance))
            {
                chunkProvider.EnsureChunkExistsAtCoordinate(chunkCoordinate);
            }

            _lastGenerationCoordinate = coordinate;
        }

        /// <summary>
        /// Calculates the chunk coordinates that should be present when the viewer is in <paramref name="centerChunkCoordinate"/>.
        /// </summary>
        /// <param name="centerChunkCoordinate">The chunk coordinate to generate the coordinates around</param>
        /// <param name="renderDistance">The radius the chunks the player sees</param>
        /// <returns>A collection of coordinates that should be generated</returns>
        private static IEnumerable<int3> GetChunkGenerationCoordinates(int3 centerChunkCoordinate, int renderDistance)
        {
            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    for (int z = -renderDistance; z <= renderDistance; z++)
                    {
                        int3 chunkCoordinate = centerChunkCoordinate + new int3(x, y, z);
                        yield return chunkCoordinate;
                    }
                }
            }
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