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
        [SerializeField] private Transform player = null;

        /// <summary>
        /// The coordinate of the chunk where terrain was last generated around
        /// </summary>
        private int3 _lastGenerationCoordinate;

        private void Start()
        {
            int3 playerCoordinate = VectorUtilities.WorldPositionToCoordinate(player.position, chunkProvider.ChunkGenerationParams.ChunkSize);
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
            int3 playerCoordinate = VectorUtilities.WorldPositionToCoordinate(player.position, chunkProvider.ChunkGenerationParams.ChunkSize);
            if (!playerCoordinate.Equals(_lastGenerationCoordinate))
            {
                List<int3> coordinatesToUnload = GetChunkCoordinatesOutsideOfRenderDistance(playerCoordinate);

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

        /// <summary>
        /// Gets a list of chunk coordinates whose Manhattan Distance to the coordinate parameter is more than <see cref="renderDistance"/>
        /// </summary>
        /// <param name="coordinate">Central coordinate</param>
        /// <returns>A list of chunk coordinates outside of the viewing range from the coordinate parameter</returns>
        private List<int3> GetChunkCoordinatesOutsideOfRenderDistance(int3 coordinate)
        {
            List<int3> chunkCoordinates = new List<int3>();
            foreach(int3 chunkCoordinate in chunkProvider.Chunks.Keys)
            {
                int dX = math.abs(coordinate.x - chunkCoordinate.x);
                int dY = math.abs(coordinate.y - chunkCoordinate.y); 
                int dZ = math.abs(coordinate.z - chunkCoordinate.z);

                if(dX > renderDistance || dY > renderDistance || dZ > renderDistance)
                {
                    chunkCoordinates.Add(chunkCoordinate);
                }
            }

            return chunkCoordinates;
        }
    }
}