using System.Collections.Generic;
using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A base class for providing chunks to the world
    /// </summary>
    public abstract class ChunkProvider : MonoBehaviour
    {
        /// <summary>
        /// Parameters that specify how a chunk will be generated
        /// </summary>
        [SerializeField] private ChunkGenerationParams chunkGenerationParams;

        /// <summary>
        /// The store which contains all the densities for the world
        /// </summary>
        [SerializeField] private VoxelDensityStore voxelDensityStore;

        /// <summary>
        /// A dictionary of all the chunks currently in the world. The key is the chunk's coordinate, and the value is the chunk
        /// </summary>
        protected Dictionary<int3, Chunk> _chunks;

        /// <summary>
        /// A dictionary of all the chunks currently in the world. The key is the chunk's coordinate, and the value is the chunk
        /// </summary>
        public Dictionary<int3, Chunk> Chunks => _chunks;

        /// <summary>
        /// Parameters that specify how a chunk will be generated
        /// </summary>
        public ChunkGenerationParams ChunkGenerationParams => chunkGenerationParams;

        /// <summary>
        /// The store which contains all the densities for the world
        /// </summary>
        protected VoxelDensityStore VoxelDensityStore => voxelDensityStore;

        protected virtual void Awake()
        {
            _chunks = new Dictionary<int3, Chunk>();
        }

        /// <summary>
        /// Creates a chunk to a coordinate, adds it to the Chunks dictionary and Initializes the chunk
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <returns>The created chunk</returns>
        public Chunk CreateChunkAtCoordinate(int3 chunkCoordinate)
        {
            Chunk chunk = Instantiate(ChunkGenerationParams.ChunkPrefab,
                    (chunkCoordinate * ChunkGenerationParams.ChunkSize).ToVectorInt(), Quaternion.identity)
                .GetComponent<Chunk>();
            _chunks.Add(chunkCoordinate, chunk);

            DensityVolume chunkDensities = CalculateChunkDensities(chunkCoordinate);
            voxelDensityStore.SetDensityChunk(chunkDensities, chunkCoordinate);

            chunk.Initialize(chunkCoordinate, ChunkGenerationParams, VoxelDensityStore);
            return chunk;
        }

        /// <summary>
        /// Tries to get a chunk from a coordinate, if it finds one it returns true and sets chunk to the found chunk, otherwise it returns false and sets chunk to null
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <param name="chunk">The chunk that was found. If none was found, it is null</param>
        /// <returns>Is there a chunk at the coordinate</returns>
        public bool TryGetChunkAtCoordinate(int3 chunkCoordinate, out Chunk chunk)
        {
            return _chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Calculates the densities for a chunk at a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose densities will be calculated</param>
        /// <returns>A density volume containing the densities. The volume's size is (chunkSize+1)^3</returns>
        protected abstract DensityVolume CalculateChunkDensities(int3 chunkCoordinate);

        /// <summary>
        /// Mark a chunk as having changes
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk to mark</param>
        public void SetChunkHasChanges(int3 chunkCoordinate)
        {
            if (TryGetChunkAtCoordinate(chunkCoordinate, out Chunk chunk))
            {
                chunk.HasChanges = true;
            }
        }

        /// <summary>
        /// Marks multiple chunks as having changes
        /// </summary>
        /// <param name="chunks">The list of chunks that should be marked</param>
        public void SetChunksHaveChanges(List<Chunk> chunks)
        {
            for (int i = 0; i < chunks.Count; i++)
            {
                chunks[i].HasChanges = true;
            }
        }

        /// <summary>
        /// Gets a list of chunks that contain a world position. For a chunk to contain a position, the position has to be inside of the chunk or on the chunk's edge
        /// </summary>
        /// <param name="worldPosition">The world position to check for</param>
        /// <returns>A list of chunks that contain the world position</returns>
        public List<Chunk> GetChunksContainingPoint(int3 worldPosition)
        {
            List<int3> chunkCoordinates = new List<int3>();
            for (int i = 0; i < 8; i++)
            {
                int3 chunkCoordinate =
                    VectorUtilities.WorldPositionToCoordinate(worldPosition - LookupTables.CubeCorners[i],
                        chunkGenerationParams.ChunkSize);
                if (chunkCoordinates.Contains(chunkCoordinate))
                {
                    continue;
                }

                chunkCoordinates.Add(chunkCoordinate);
            }

            List<Chunk> chunks = new List<Chunk>(chunkCoordinates.Count);
            for (int i = 0; i < chunkCoordinates.Count; i++)
            {
                if (TryGetChunkAtCoordinate(chunkCoordinates[i], out Chunk chunk))
                {
                    chunks.Add(chunk);
                }
            }

            return chunks;
        }
    }
}