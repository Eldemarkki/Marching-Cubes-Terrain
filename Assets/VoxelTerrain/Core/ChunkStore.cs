using Eldemarkki.VoxelTerrain.World.Chunks;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    public class ChunkStore : MonoBehaviour
    {
        private Dictionary<int3, Chunk> _chunks;

        private void Awake()
        {
            _chunks = new Dictionary<int3, Chunk>();
        }

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

        public void UnloadChunk(int3 chunkCoordinate)
        {
            if(TryGetChunkAtCoordinate(chunkCoordinate, out Chunk chunk))
            {
                Destroy(chunk.gameObject);
                _chunks.Remove(chunkCoordinate);
            }
        }

        /// <summary>
        /// Gets a list of chunk coordinates whose Manhattan Distance to the coordinate parameter is more than <see cref="renderDistance"/>
        /// </summary>
        /// <param name="coordinate">Central coordinate</param>
        /// <returns>A list of chunk coordinates outside of the viewing range from the coordinate parameter</returns>
        public List<int3> GetChunkCoordinatesOutsideOfRenderDistance(int3 coordinate, int renderDistance)
        {
            List<int3> chunkCoordinates = new List<int3>();
            foreach (int3 chunkCoordinate in _chunks.Keys)
            {
                int dX = math.abs(coordinate.x - chunkCoordinate.x);
                int dY = math.abs(coordinate.y - chunkCoordinate.y);
                int dZ = math.abs(coordinate.z - chunkCoordinate.z);

                if (dX > renderDistance || dY > renderDistance || dZ > renderDistance)
                {
                    chunkCoordinates.Add(chunkCoordinate);
                }
            }

            return chunkCoordinates;
        }

        public void AddChunk(int3 chunkCoordinate, Chunk chunk)
        {
            if (!_chunks.ContainsKey(chunkCoordinate))
            {
                _chunks.Add(chunkCoordinate, chunk);
            }
        }

        public void RemoveChunk(int3 fromCoordinate)
        {
            _chunks.Remove(fromCoordinate);
        }
    }
}