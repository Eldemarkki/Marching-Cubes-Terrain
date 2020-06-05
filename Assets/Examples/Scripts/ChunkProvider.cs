using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// Interface for providing chunks to worlds
    /// </summary>
    /// <typeparam name="T">Type of chunk to provide</typeparam>
    [System.Serializable]
    public abstract class ChunkProvider<T> : MonoBehaviour where T : Chunk
    {
        [SerializeField] private ChunkGenerationParams chunkGenerationParams;

        /// <summary>
        /// Parameters that specify how a chunk will be generated
        /// </summary>
        public ChunkGenerationParams ChunkGenerationParams => chunkGenerationParams;

        /// <summary>
        /// A dictionary of all the chunks currently in the world. The key is the chunk's coordinate, and the value is the chunk
        /// </summary>
        public Dictionary<int3, T> Chunks { get; set; }

        protected virtual void Awake()
        {
            Chunks = new Dictionary<int3, T>();
        }

        /// <summary>
        /// Creates a chunk to a coordinate, adds it to <see cref="Chunks"/> and Initializes the chunk
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <returns>The created chunk</returns>
        public abstract T CreateChunkAtCoordinate(int3 chunkCoordinate);

        /// <summary>
        /// Tries to get a chunk from a coordinate, if it finds one it returns true and sets chunk to the found chunk, otherwise it returns false and sets chunk to null
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <param name="chunk">The found chunk, if none was found it is null</param>
        /// <returns>Is there a chunk at the coordinate</returns>
        public virtual bool TryGetChunkAtCoordinate(int3 chunkCoordinate, out T chunk)
        {
            return Chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a chunk is created
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public abstract void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate);

        /// <summary>
        /// Unloads the Chunks whose coordinate is in the coordinatesToUnload parameter
        /// </summary>
        /// <param name="coordinatesToUnload">A list of the coordinates to unload</param>
        public abstract void UnloadCoordinates(List<int3> coordinatesToUnload);
    }
}