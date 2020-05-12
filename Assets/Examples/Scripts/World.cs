using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// Base class for all worlds
    /// </summary>
    /// <typeparam name="T">The type of chunks the world will control</typeparam>
    public abstract class World<T> : MonoBehaviour, IDensityProvider where T : Chunk
    {
        /// <summary>
        /// Chunk provider for this world
        /// </summary>
        public IChunkProvider<T> ChunkProvider { get; protected set; }

        protected virtual void Awake()
        {
            ChunkProvider = GetComponent<IChunkProvider<T>>();
        }

        /// <summary>
        /// Tries to get a chunk at a world position
        /// </summary>
        /// <param name="worldPosition">World position of the chunk (can be inside the chunk, doesn't have to be the chunk's origin)</param>
        /// <param name="chunk">The chunk at that position (if any)</param>
        /// <returns>Does a chunk exist at that world position</returns>
        public bool TryGetChunk(int3 worldPosition, out T chunk)
        {
            int3 chunkCoordinate = WorldPositionToCoordinate(worldPosition);
            return ChunkProvider.TryGetChunkAtCoordinate(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Gets the density at a world position
        /// </summary>
        /// <param name="worldPosition">The world position of the density to get</param>
        /// <returns>The density at that world position (0 if it doesn't exist)</returns>
        public float GetDensity(int3 worldPosition)
        {
            if (TryGetChunk(worldPosition, out T chunk))
            {
                return chunk.GetDensity(worldPosition.Mod(ChunkProvider.ChunkGenerationParams.ChunkSize));
            }

            return 0;
        }

        /// <summary>
        /// Sets the density at a world position
        /// </summary>
        /// <param name="density">The new density</param>
        /// <param name="worldPosition">The density's world position</param>
        public void SetDensity(float density, int3 worldPosition)
        {
            List<int3> modifiedChunkPositions = new List<int3>();
            for (int i = 0; i < 8; i++)
            {
                int3 chunkPos = ChunkProvider.ChunkGenerationParams.ChunkSize * WorldPositionToCoordinate(worldPosition - LookupTables.CubeCorners[i]);
                if (modifiedChunkPositions.Contains(chunkPos)) { continue; }

                if (TryGetChunk(chunkPos, out T chunk))
                {
                    int3 localPos = (worldPosition - chunkPos).Mod(ChunkProvider.ChunkGenerationParams.ChunkSize + 1);
                    chunk.SetDensity(density, localPos);
                    modifiedChunkPositions.Add(chunkPos);
                }
            }
        }

        /// <summary>
        /// Converts a world position to a chunk coordinate
        /// </summary>
        /// <param name="worldPosition">The world-position that should be converted</param>
        /// <returns>The world position converted to a chunk coordinate</returns>
        public int3 WorldPositionToCoordinate(float3 worldPosition)
        {
            int chunkSize = ChunkProvider.ChunkGenerationParams.ChunkSize;
            return worldPosition.FloorToMultipleOfX(chunkSize) / chunkSize;
        }
    }
}