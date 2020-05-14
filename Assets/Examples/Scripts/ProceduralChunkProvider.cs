using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// Provider for procedurally generated chunks
    /// </summary>
    public class ProceduralChunkProvider : MonoBehaviour, IChunkProvider<ProceduralChunk>
    {
        /// <summary>
        /// Parameters that specify how a chunk will be generated
        /// </summary>
        [SerializeField] private ChunkGenerationParams _chunkGenerationParams;

        /// <summary>
        /// The procedural terrain generation settings
        /// </summary>
        [SerializeField] private ProceduralTerrainSettings _proceduralTerrainSettings;

        /// <summary>
        /// How many chunks maximum can be generated in one frame
        /// </summary>
        [SerializeField] private int chunksToGeneratePerFrame = 10;

        /// <summary>
        /// A queue that contains the chunks' coordinates that are out of view and thus ready to be moved anywhere
        /// </summary>
        private Queue<int3> _availableChunkCoordinates;

        /// <summary>
        /// A list that contains all the coordinates where a chunk will eventually have to be generated
        /// </summary>
        private List<int3> _generationQueue;

        /// <summary>
        /// A dictionary of all the chunks currently in the world. The key is the chunk's coordinate, and the value is the chunk
        /// </summary>
        public Dictionary<int3, ProceduralChunk> Chunks { get; set; }

        /// <summary>
        /// Parameters that specify how a chunk will be generated
        /// </summary>
        public ChunkGenerationParams ChunkGenerationParams => _chunkGenerationParams;

        private void Awake()
        {
            Chunks = new Dictionary<int3, ProceduralChunk>();
            _availableChunkCoordinates = new Queue<int3>();
            _generationQueue = new List<int3>();
        }

        private void Start()
        {
            StartCoroutine(GenerateChunks());
        }

        /// <summary>
        /// Kind of like Unity's Update() function, generates chunks but not all at once
        /// </summary>
        /// <returns></returns>
        private IEnumerator GenerateChunks()
        {
            while (true)
            {
                // This loop represents Unity's update function

                int chunksGenerated = 0;
                while (_generationQueue.Count > 0 && chunksGenerated < chunksToGeneratePerFrame)
                {
                    int3 chunkCoordinate = _generationQueue[0];
                    _generationQueue.RemoveAt(0);

                    if (_availableChunkCoordinates.Count == 0)
                    {
                        CreateChunkAtCoordinate(chunkCoordinate);
                    }
                    else
                    {
                        int3 availableChunkCoordinate = _availableChunkCoordinates.Dequeue();
                        ProceduralChunk chunk = Chunks[availableChunkCoordinate];
                        Chunks.Remove(availableChunkCoordinate);
                        Chunks.Add(chunkCoordinate, chunk);
                        chunk.SetCoordinate(chunkCoordinate);
                    }

                    chunksGenerated++;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Creates a chunk to a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <returns>The created chunk</returns>
        public ProceduralChunk CreateChunkAtCoordinate(int3 chunkCoordinate)
        {
            ProceduralChunk chunk = Instantiate(_chunkGenerationParams.ChunkPrefab, (chunkCoordinate * _chunkGenerationParams.ChunkSize).ToVectorInt(), Quaternion.identity).GetComponent<ProceduralChunk>();
            Chunks.Add(chunkCoordinate, chunk);
            chunk.TerrainGenerationSettings = _proceduralTerrainSettings;
            chunk.Initialize(chunkCoordinate, ChunkGenerationParams);
            return chunk;
        }

        /// <summary>
        /// Tries to get a chunk from a coordinate, if it finds one it returns true and sets chunk to the found chunk, otherwise it returns false and sets chunk to null
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <param name="chunk">The found chunk, if none was found it is null</param>
        /// <returns>Is there a chunk at the coordinate</returns>
        public bool TryGetChunkAtCoordinate(int3 chunkCoordinate, out ProceduralChunk chunk)
        {
            return Chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a chunk is created using <see cref="CreateChunkAtCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (Chunks.ContainsKey(chunkCoordinate)) return;

            if (_generationQueue.Contains(chunkCoordinate)) return;

            _generationQueue.Add(chunkCoordinate);
        }

        /// <summary>
        /// Unloads the Chunks whose coordinate is in the coordinatesToUnload parameter
        /// </summary>
        /// <param name="coordinatesToUnload">A list of the coordinates to unload</param>
        public void UnloadCoordinates(List<int3> coordinatesToUnload)
        {
            // Mark coordinates as available
            for (var i = 0; i < coordinatesToUnload.Count; i++)
            {
                int3 coordinateToUnload = coordinatesToUnload[i];
                if (!_availableChunkCoordinates.Contains(coordinateToUnload))
                {
                    _availableChunkCoordinates.Enqueue(coordinateToUnload);
                }

                if (_generationQueue.Contains(coordinateToUnload))
                {
                    _generationQueue.Remove(coordinateToUnload);
                }
            }
        }
    }
}