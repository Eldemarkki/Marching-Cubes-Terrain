using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// A procedurally generated world
    /// </summary>
    [RequireComponent(typeof(ProceduralChunkProvider))]
    public class ProceduralWorld : VoxelWorld<ProceduralChunk>
    {
        /// <summary>
        /// The "radius" of the chunks the player sees
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

        protected override void Awake()
        {
            base.Awake();
            _lastGenerationCoordinate = WorldPositionToCoordinate(player.position);
        }

        private void Start()
        {
            int3 playerCoordinate = WorldPositionToCoordinate(player.position);
            _lastGenerationCoordinate = playerCoordinate;
            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    for (int z = -renderDistance; z <= renderDistance; z++)
                    {
                        int3 chunkCoordinate = playerCoordinate + new int3(x, y, z);
                        ChunkProvider.EnsureChunkExistsAtCoordinate(chunkCoordinate);
                    }
                }
            }
        }

        private void Update()
        {
            int3 playerCoordinate = WorldPositionToCoordinate(player.position);
            if (!playerCoordinate.Equals(_lastGenerationCoordinate))
            {
                List<int3> coordinatesToUnload = GetChunkCoordinatesOutsideOfRenderDistance(playerCoordinate);
                ChunkProvider.UnloadCoordinates(coordinatesToUnload);

                for (int x = -renderDistance; x <= renderDistance; x++)
                {
                    for (int y = -renderDistance; y <= renderDistance; y++)
                    {
                        for (int z = -renderDistance; z <= renderDistance; z++)
                        {
                            int3 chunkCoordinate = playerCoordinate + new int3(x, y, z);
                            ChunkProvider.EnsureChunkExistsAtCoordinate(chunkCoordinate);
                        }
                    }
                }

                _lastGenerationCoordinate = playerCoordinate;
            }
        }

        /// <summary>
        /// Gets a list of chunk coordinates (from <see cref="ChunkProvider{T}.Chunks"/>) whose Manhattan Distance to the coordinate parameter is more than <see cref="renderDistance"/>
        /// </summary>
        /// <param name="coordinate">Central coordinate</param>
        /// <returns>A list of chunk coordinates outside of the viewing range from the coordinate parameter</returns>
        private List<int3> GetChunkCoordinatesOutsideOfRenderDistance(int3 coordinate)
        {
            List<int3> chunkCoordinates = new List<int3>();
            foreach(int3 chunkCoordinate in ChunkProvider.Chunks.Keys)
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