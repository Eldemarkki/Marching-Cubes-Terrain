using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// A chunk provider for heightmap chunks
    /// </summary>
    public class HeightmapChunkProvider : MonoBehaviour, IChunkProvider<HeightmapChunk>
    {
        /// <summary>
        /// Parameters that specify how a chunk should be generated
        /// </summary>
        [SerializeField] private ChunkGenerationParams _chunkGenerationParams;

        /// <summary>
        /// Information about how the heightmap world should be generated
        /// </summary>
        [SerializeField] private HeightmapTerrainSettings _heightmapTerrainSettings;

        /// <summary>
        /// A dictonary of all the chunks currently in the world. The key is the chunk's coordinate, and the value is the chunk
        /// </summary>
        public Dictionary<int3, HeightmapChunk> Chunks { get; set; }

        /// <summary>
        /// Parameters that specify how a chunk should be generated
        /// </summary>
        public ChunkGenerationParams ChunkGenerationParams => _chunkGenerationParams;

        private void Awake()
        {
            Chunks = new Dictionary<int3, HeightmapChunk>();
        }

        /// <summary>
        /// Instantiates and initializes a chunk at a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate where the chunk should be created</param>
        /// <returns>The created chunk</returns>
        public HeightmapChunk CreateChunkAtCoordinate(int3 chunkCoordinate)
        {
            HeightmapChunk chunk = Instantiate(_chunkGenerationParams.ChunkPrefab, (chunkCoordinate * _chunkGenerationParams.ChunkSize).ToVectorInt(), Quaternion.identity).GetComponent<HeightmapChunk>();
            Chunks.Add(chunkCoordinate, chunk);
            chunk.HeightmapTerrainSettings = _heightmapTerrainSettings;
            chunk.Initialize(chunkCoordinate, ChunkGenerationParams);
            return chunk;
        }

        /// <summary>
        /// Tries to get a chunk from a coordinate, if it finds one it returns true and sets chunk to the found chunk, otherwise it returns false and sets chunk to null
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <param name="chunk">The found chunk, if none was found it is null</param>
        /// <returns>Is there a chunk at the coordinate</returns>
        public bool TryGetChunkAtCoordinate(int3 chunkCoordinate, out HeightmapChunk chunk)
        {
            return Chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a chunk is created there via <see cref="CreateChunkAtCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (!Chunks.ContainsKey(chunkCoordinate))
            {
                CreateChunkAtCoordinate(chunkCoordinate);
            }
        }

        /// <summary>
        /// Destroy's and removes (from <see cref="Chunks"/>) all chunks whose coordinate is in <see cref="coordinatesToUnload"/>
        /// </summary>
        /// <param name="coordinatesToUnload">A list of all the coordinates that should be unloaded</param>
        public void UnloadCoordinates(List<int3> coordinatesToUnload)
        {
            for (var i = 0; i < coordinatesToUnload.Count; i++)
            {
                int3 coordinate = coordinatesToUnload[i];
                if (TryGetChunkAtCoordinate(coordinate, out HeightmapChunk chunk))
                {
                    Destroy(chunk.gameObject);
                    Chunks.Remove(coordinate);
                }
            }
        }
    }
}