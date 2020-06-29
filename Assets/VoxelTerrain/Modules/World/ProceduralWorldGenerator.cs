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
        [SerializeField] private int renderDistance = 4;

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
                List<int3> coordinatesToUnload = voxelWorld.ChunkStore.GetChunkCoordinatesOutsideOfRenderDistance(playerCoordinate, renderDistance);
                chunkProvider.UnloadCoordinates(coordinatesToUnload);

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
        /// Generates terrain around <paramref name="coordinate"/> with a radius of <paramref name="coordinate"/>
        /// </summary>
        /// <param name="coordinate">The coordinate to generate the terrain around</param>
        private void GenerateTerrainAroundCoordinate(int3 coordinate)
        {
            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    for (int z = -renderDistance; z <= renderDistance; z++)
                    {
                        int3 chunkCoordinate = coordinate + new int3(x, y, z);
                        chunkProvider.EnsureChunkExistsAtCoordinate(chunkCoordinate);
                    }
                }
            }

            _lastGenerationCoordinate = coordinate;
        }
    }
}