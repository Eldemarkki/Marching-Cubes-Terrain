using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    /// <summary>
    /// A container for all of the chunks in the world
    /// </summary>
    public class ChunkStore : MonoBehaviour
    {
        /// <summary>
        /// A dictionary of all the chunks currently in the world. The key is the chunk's coordinate, and the value is the chunk
        /// </summary>
        private Dictionary<int3, ChunkProperties> _chunks;

        /// <summary>
        /// Every chunk that is currently loaded
        /// </summary>
        public IEnumerable<ChunkProperties> Chunks => _chunks.Values;

        public VoxelWorld VoxelWorld { get; set; }

        private void Awake()
        {
            _chunks = new Dictionary<int3, ChunkProperties>();
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
        public bool TryGetChunkAtCoordinate(int3 chunkCoordinate, out ChunkProperties chunk)
        {
            return _chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Gets a collection of chunk coordinates whose Manhattan Distance to the coordinate parameter is more than <paramref name="renderDistance"/>
        /// </summary>
        /// <param name="coordinate">Central coordinate</param>
        /// <param name="renderDistance">The radius of the chunks the player can see</param>
        /// <returns>A collection of chunk coordinates outside of the viewing range from the coordinate parameter</returns>
        public IEnumerable<int3> GetChunkCoordinatesOutsideOfRenderDistance(int3 coordinate, int renderDistance)
        {
            foreach (int3 chunkCoordinate in _chunks.Keys.ToList())
            {
                int3 difference = math.abs(coordinate - chunkCoordinate);

                if(math.any(difference > renderDistance))
                {
                    yield return chunkCoordinate;
                }
            }
        }

        /// <summary>
        /// Adds a chunk to the chunk store
        /// </summary>
        /// <param name="chunkProperties">The chunk to add</param>
        public void AddChunk(ChunkProperties chunkProperties)
        {
            if (!_chunks.ContainsKey(chunkProperties.ChunkCoordinate))
            {
                _chunks.Add(chunkProperties.ChunkCoordinate, chunkProperties);
            }
        }

        public void AddChunkUnchecked(ChunkProperties chunkProperties)
        {
            _chunks.Add(chunkProperties.ChunkCoordinate, chunkProperties);
        }

        /// <summary>
        /// Removes a chunk from a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk to remove</param>
        public void RemoveChunk(int3 chunkCoordinate)
        {
            _chunks.Remove(chunkCoordinate);
        }

        public void MoveChunk(int3 from, int3 to)
        {
            // Check that 'from' and 'to' are not equal
            if (from.Equals(to)) { return; }

            // Check that a chunk does NOT already exist at 'to'
            if (DoesChunkExistAtCoordinate(to)) { return; }

            // Check that a chunk exists at 'from'
            if (!TryGetChunkAtCoordinate(from, out ChunkProperties chunk)) { return; }

            RemoveChunk(from);
            chunk.MeshCollider.enabled = false;
            chunk.MeshRenderer.enabled = false;
            chunk.Initialize(to, VoxelWorld.WorldSettings.ChunkSize);

            AddChunk(chunk);
        }
    }
}