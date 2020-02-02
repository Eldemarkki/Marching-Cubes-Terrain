using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public abstract class World : MonoBehaviour
    {
        /// <summary>
        /// The chunk's size. This represents the width, height and depth in Unity units.
        /// </summary>
        [SerializeField] private int chunkSize = 16;

        /// <summary>
        /// The chunk's prefab that will be instantiated
        /// </summary>
        [SerializeField] private GameObject chunkPrefab;

        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        [SerializeField] private float isolevel;

        /// <summary>
        /// The chunk's size. This represents the width, height and depth in Unity units.
        /// </summary>
        public int ChunkSize => chunkSize;

        /// <summary>
        /// The chunk's prefab that will be instantiated
        /// </summary>
        public GameObject ChunkPrefab => chunkPrefab;

        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        public float Isolevel => isolevel;

        /// <summary>
        /// All the chunks of this world
        /// </summary>
        public Dictionary<int3, Chunk> Chunks { get; set; }

        protected virtual void Start()
        {
            Chunks = new Dictionary<int3, Chunk>();
        }
        
        /// <summary>
        /// Tries to get a chunk at a world position
        /// </summary>
        /// <param name="worldPosition">World position of the chunk (can be inside the chunk, doesn't have to be the chunk's origin)</param>
        /// <param name="chunk">The chunk at that position (if any)</param>
        /// <returns>Does a chunk exist at that world position</returns>
        public bool TryGetChunk(int3 worldPosition, out Chunk chunk)
        {
            int3 chunkCoordinate = WorldPositionToCoordinate(worldPosition);
            return Chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Gets the density at a world position
        /// </summary>
        /// <param name="worldPosition">The world position of the density to get</param>
        /// <returns>The density at that world position (0 if it doesn't exist)</returns>
        public float GetDensity(int3 worldPosition)
        {
            if (TryGetChunk(worldPosition, out Chunk chunk))
            {
                return chunk.GetDensity(worldPosition.Mod(ChunkSize));
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
                int3 chunkPos = chunkSize * WorldPositionToCoordinate(worldPosition - LookupTables.CubeCorners[i]);
                if (modifiedChunkPositions.Contains(chunkPos)) { continue; }

                if (TryGetChunk(chunkPos, out Chunk chunk))
                {
                    int3 localPos = (worldPosition - chunkPos).Mod(ChunkSize + 1);
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
            return worldPosition.FloorToMultipleOfX(ChunkSize) / ChunkSize;
        }
    }
}