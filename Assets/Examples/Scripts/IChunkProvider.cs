using System.Collections.Generic;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// Interface for providing chunks to worlds
    /// </summary>
    /// <typeparam name="T">Type of chunk to provide</typeparam>
    public interface IChunkProvider<T> where T : Chunk
    {
        /// <summary>
        /// Parameters that specify how a chunk will be generated
        /// </summary>
        ChunkGenerationParams ChunkGenerationParams { get; }

        /// <summary>
        /// A dictionary of all the chunks currently in the world. The key is the chunk's coordinate, and the value is the chunk
        /// </summary>
        Dictionary<int3, T> Chunks { get; set; }

        /// <summary>
        /// Creates a chunk to a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <returns>The created chunk</returns>
        T CreateChunkAtCoordinate(int3 chunkCoordinate);

        /// <summary>
        /// Tries to get a chunk from a coordinate, if it finds one it returns true and sets chunk to the found chunk, otherwise it returns false and sets chunk to null
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <param name="chunk">The found chunk, if none was found it is null</param>
        /// <returns>Is there a chunk at the coordinate</returns>
        bool TryGetChunkAtCoordinate(int3 chunkCoordinate, out T chunk);

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a chunk is created
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate);

        /// <summary>
        /// Unloads the Chunks whose coordinate is in the coordinatesToUnload parameter
        /// </summary>
        /// <param name="coordinatesToUnload">A queue of the coordinates to unload</param>
        void UnloadCoordinates(Queue<int3> coordinatesToUnload);
    }
}