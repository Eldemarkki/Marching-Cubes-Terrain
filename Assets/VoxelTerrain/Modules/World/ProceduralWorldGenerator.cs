using Eldemarkki.VoxelTerrain.Chunks;
#if DEBUG
using Eldemarkki.VoxelTerrain.Debugging;
#endif
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
        /// The coordinate of the chunk where terrain was last generated around
        /// </summary>
        private int3 _lastGenerationCoordinate;

        private void Start()
        {
#if DEBUG
            DebugView.AddDebugProperty("Render distance", () => renderDistance);
#endif

            int3 playerCoordinate = GetPlayerCoordinate();
            GenerateTerrainAroundCoordinate(playerCoordinate);
        }

        private void Update()
        {
            int3 newPlayerCoordinate = GetPlayerCoordinate();
            if (!newPlayerCoordinate.Equals(_lastGenerationCoordinate))
            {
                void addChunkToGenerationQueue(int3 coordinate) => chunkProvider.AddChunkToGenerationQueue(coordinate);
                voxelWorld.VoxelDataStore.MoveChunks(_lastGenerationCoordinate, newPlayerCoordinate, renderDistance, addChunkToGenerationQueue);
                voxelWorld.VoxelColorStore.MoveChunks(_lastGenerationCoordinate, newPlayerCoordinate, renderDistance, addChunkToGenerationQueue);
                voxelWorld.ChunkStore.MoveChunks(_lastGenerationCoordinate, newPlayerCoordinate, renderDistance, addChunkToGenerationQueue);
                _lastGenerationCoordinate = newPlayerCoordinate;
            }
        }


        /// <summary>
        /// Get's the current coordinate of <see cref="player"/>
        /// </summary>
        /// <returns>The coordinate of <see cref="player"/></returns>
        private int3 GetPlayerCoordinate()
        {
            return VectorUtilities.WorldPositionToCoordinate(voxelWorld.Player.position, voxelWorld.WorldSettings.ChunkSize);
        }

        /// <summary>
        /// Generates terrain around <paramref name="coordinate"/> with a radius of <see cref="renderDistance"/>
        /// </summary>
        /// <param name="coordinate">The coordinate to generate the terrain around</param>
        private void GenerateTerrainAroundCoordinate(int3 coordinate)
        {
            // Generate chunks with radius 'renderDistance'
            int3[] chunkGenerationCoordinates = CoordinateUtilities.GetChunkGenerationCoordinates(coordinate, renderDistance);
            for (int i = 0; i < chunkGenerationCoordinates.Length; i++)
            {
                int3 generationCoordinate = chunkGenerationCoordinates[i];
                chunkProvider.EnsureChunkExistsAtCoordinate(generationCoordinate);
            }

            _lastGenerationCoordinate = coordinate;
        }
    }
}