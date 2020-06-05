using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// A chunk provider for heightmap chunks
    /// </summary>
    public class HeightmapChunkProvider : ChunkProvider<HeightmapChunk>
    {
        /// <summary>
        /// Information about how the heightmap world should be generated
        /// </summary>
        [SerializeField] private HeightmapTerrainSettings _heightmapTerrainSettings = null;

        /// <summary>
        /// Creates a chunk to a coordinate, adds it to the Chunks dictionary and Initializes the chunk
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <returns>The created chunk</returns>
        public override HeightmapChunk CreateChunkAtCoordinate(int3 chunkCoordinate)
        {
            HeightmapChunk chunk = Instantiate(ChunkGenerationParams.ChunkPrefab, (chunkCoordinate * ChunkGenerationParams.ChunkSize).ToVectorInt(), Quaternion.identity).GetComponent<HeightmapChunk>();
            Chunks.Add(chunkCoordinate, chunk);
            chunk.HeightmapTerrainSettings = _heightmapTerrainSettings;
            chunk.Initialize(chunkCoordinate, ChunkGenerationParams);
            return chunk;
        }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a chunk is created there via <see cref="CreateChunkAtCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public override void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
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
        public override void UnloadCoordinates(List<int3> coordinatesToUnload)
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