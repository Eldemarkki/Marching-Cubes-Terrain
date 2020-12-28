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

                var newlyFreedCoordinates = voxelWorld.ChunkStore.GetChunkCoordinatesOutsideOfRange(newPlayerCoordinate, renderDistance);

                int3 renderSize = new int3(renderDistance * 2 + 1);

                int3 oldPos = _lastGenerationCoordinate - new int3(renderDistance);
                BoundsInt oldCoords = new BoundsInt(oldPos.ToVectorInt(), renderSize.ToVectorInt());

                int3 newPos = newPlayerCoordinate - new int3(renderDistance);
                BoundsInt newCoords = new BoundsInt(newPos.ToVectorInt(), renderSize.ToVectorInt());

                int3[] coordinatesThatNeedChunks = CoordinateUtilities.GetCoordinatesThatNeedChunks(oldCoords, newCoords);

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

            int3[] coordinatesThatNeedData = CoordinateUtilities.GetCoordinatesThatNeedChunks(oldCoords, newCoords);

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
            // Start generating voxel data for chunks with radius 'renderDistance + loadingBufferSize'
            int3[] preloadCoordinates = CoordinateUtilities.GetPreloadCoordinates(coordinate, renderDistance, loadingBufferSize);
            for (int i = 0; i < preloadCoordinates.Length; i++)
            {
                int3 loadingCoordinate = preloadCoordinates[i];
                voxelWorld.VoxelDataStore.GenerateDataForChunk(loadingCoordinate);
            }

            // Generate chunks with radius 'renderDistance'
            int3[] chunkGenerationCoordinates = CoordinateUtilities.GetChunkGenerationCoordinates(coordinate, renderDistance);
            for (int i = 0; i < chunkGenerationCoordinates.Length; i++)
            {
                int3 generationCoordinate = chunkGenerationCoordinates[i];
                voxelWorld.VoxelColorStore.GenerateDataForChunk(generationCoordinate);
                chunkProvider.EnsureChunkExistsAtCoordinate(generationCoordinate);
            }

            _lastGenerationCoordinate = coordinate;
        }
    }
}