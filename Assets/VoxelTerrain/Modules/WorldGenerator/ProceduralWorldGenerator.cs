using System.Collections.Generic;
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
        [SerializeField] private VoxelWorld voxelWorld = null;

        /// <summary>
        /// A chunk provider which provides chunks with procedurally generated data
        /// </summary>
        [SerializeField] private ProceduralChunkProvider chunkProvider = null;

        /// <summary>
        /// The radius of the chunks the player sees
        /// </summary>
        [SerializeField] private int renderDistance = 4;

        /// <summary>
        /// The viewer which the terrain is generated around
        /// </summary>
        [SerializeField] private Transform player = null;

        /// <summary>
        /// The coordinate of the chunk where terrain was last generated around
        /// </summary>
        private int3 _lastGenerationCoordinate;

        private void Start()
        {
            int3 playerCoordinate = VectorUtilities.WorldPositionToCoordinate(player.position, voxelWorld.WorldSettings.ChunkSize);
            _lastGenerationCoordinate = playerCoordinate;
            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    for (int z = -renderDistance; z <= renderDistance; z++)
                    {
                        int3 chunkCoordinate = playerCoordinate + new int3(x, y, z);
                        chunkProvider.EnsureChunkExistsAtCoordinate(chunkCoordinate);
                    }
                }
            }
        }

        private void Update()
        {
            int3 playerCoordinate = VectorUtilities.WorldPositionToCoordinate(player.position, voxelWorld.WorldSettings.ChunkSize);
            if (!playerCoordinate.Equals(_lastGenerationCoordinate))
            {
                List<int3> coordinatesToUnload = voxelWorld.ChunkStore.GetChunkCoordinatesOutsideOfRenderDistance(playerCoordinate, renderDistance);

                chunkProvider.UnloadCoordinates(coordinatesToUnload);

                for (int x = -renderDistance; x <= renderDistance; x++)
                {
                    for (int y = -renderDistance; y <= renderDistance; y++)
                    {
                        for (int z = -renderDistance; z <= renderDistance; z++)
                        {
                            int3 chunkCoordinate = playerCoordinate + new int3(x, y, z);
                            chunkProvider.EnsureChunkExistsAtCoordinate(chunkCoordinate);
                        }
                    }
                }

                _lastGenerationCoordinate = playerCoordinate;
            }
        }
    }
}