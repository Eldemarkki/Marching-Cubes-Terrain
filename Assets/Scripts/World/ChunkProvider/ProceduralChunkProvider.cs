using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World.Chunks;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// Provider for procedurally generated chunks
    /// </summary>
    public class ProceduralChunkProvider : ChunkProvider<ProceduralChunk>
    {
        /// <summary>
        /// The procedural terrain generation settings
        /// </summary>
        [SerializeField] private ProceduralTerrainSettings _proceduralTerrainSettings = new ProceduralTerrainSettings(1, 9, 120, 0);

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

        protected override void Awake()
        {
            base.Awake();
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
        /// Creates a chunk to a coordinate, adds it to the Chunks dictionary and Initializes the chunk
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <returns>The created chunk</returns>
        public override ProceduralChunk CreateChunkAtCoordinate(int3 chunkCoordinate)
        {
            ProceduralChunk chunk = Instantiate(ChunkGenerationParams.ChunkPrefab, (chunkCoordinate * ChunkGenerationParams.ChunkSize).ToVectorInt(), Quaternion.identity).GetComponent<ProceduralChunk>();
            Chunks.Add(chunkCoordinate, chunk);
            chunk.TerrainGenerationSettings = _proceduralTerrainSettings;
            chunk.Initialize(chunkCoordinate, ChunkGenerationParams);
            return chunk;
        }


        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a chunk is created using <see cref="CreateChunkAtCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public override void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (Chunks.ContainsKey(chunkCoordinate)) { return; }
            if (_generationQueue.Contains(chunkCoordinate)) { return; }

            _generationQueue.Add(chunkCoordinate);
        }

        /// <summary>
        /// Unloads the Chunks whose coordinate is in the coordinatesToUnload parameter
        /// </summary>
        /// <param name="coordinatesToUnload">A list of the coordinates to unload</param>
        public override void UnloadCoordinates(List<int3> coordinatesToUnload)
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