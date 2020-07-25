using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    /// <summary>
    /// A container for all of the chunks in the world
    /// </summary>
    public class ChunkStore : MonoBehaviour
    {
        /// A dictionary of all the chunks currently in the world. The key is the chunk's coordinate, and the value is the chunk
        private Dictionary<int3, Chunk> _chunks;

        private void Awake()
        {
            _chunks = new Dictionary<int3, Chunk>();
        }

        /// <summary>
        /// Gets whether or not a chunk exists at a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk to check</param>
        /// <returns>Is there a chunk at the coordinate</returns>
        public bool DoesChunkExistAtCoordinate(int3 chunkCoordinate)
        {
            return _chunks.ContainsKey(chunkCoordinate);
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
        /// Gets a collection of chunk coordinates whose Manhattan Distance to the coordinate parameter is more than <see cref="renderDistance"/>
        /// </summary>
        /// <param name="coordinate">Central coordinate</param>
        /// <param name="renderDistance">The radius of the chunks the player can see</param>
        /// <returns>A collection of chunk coordinates outside of the viewing range from the coordinate parameter</returns>
        public IEnumerable<int3> GetChunkCoordinatesOutsideOfRenderDistance(int3 coordinate, int renderDistance)
        {
            foreach (int3 chunkCoordinate in _chunks.Keys)
            {
                int dX = math.abs(coordinate.x - chunkCoordinate.x);
                int dY = math.abs(coordinate.y - chunkCoordinate.y);
                int dZ = math.abs(coordinate.z - chunkCoordinate.z);

                if (dX > renderDistance || dY > renderDistance || dZ > renderDistance)
                {
                    yield return chunkCoordinate;
                }
            }
        }

        /// <summary>
        /// Adds a chunk to the chunk store
        /// </summary>
        /// <param name="chunk">The chunk to add</param>
        public void AddChunk(Chunk chunk)
        {
            if (!_chunks.ContainsKey(chunk.ChunkCoordinate))
            {
                _chunks.Add(chunk.ChunkCoordinate, chunk);
            }
        }

        /// <summary>
        /// Removes a chunk from a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk to remove</param>
        public void RemoveChunk(int3 chunkCoordinate)
        {
            _chunks.Remove(chunkCoordinate);
        }
    }
}