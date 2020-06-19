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
    public class ProceduralChunkProvider : ChunkProvider
    {
        /// <summary>
        /// The maximum amount of chunks that can be generated in one frame
        /// </summary>
        [SerializeField] private int chunkGenerationRate = 10;

        /// <summary>
        /// A queue that contains the chunks' coordinates that are out of view and thus ready to be moved anywhere
        /// </summary>
        private Queue<int3> _availableChunkCoordinates;

        /// <summary>
        /// A list that contains all the coordinates where a chunk will eventually have to be generated
        /// </summary>
        private List<int3> _generationQueue;

        private void Awake()
        {
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
                while (_generationQueue.Count > 0 && chunksGenerated < chunkGenerationRate)
                {
                    int3 chunkCoordinate = _generationQueue[0];
                    _generationQueue.RemoveAt(0);

                    if (_availableChunkCoordinates.Count == 0)
                    {
                        VoxelWorld.ChunkLoader.LoadChunkToCoordinate(chunkCoordinate);
                    }
                    else
                    {
                        int3 availableChunkCoordinate = _availableChunkCoordinates.Dequeue();
                        MoveChunk(availableChunkCoordinate, chunkCoordinate);
                    }

                    chunksGenerated++;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a new chunk is created in the next frame
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public override void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (VoxelWorld.ChunkStore.DoesChunkExistAtCoordinate(chunkCoordinate)) { return; }
            if (_generationQueue.Contains(chunkCoordinate)) { return; }

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

            VoxelWorld.VoxelDataStore.UnloadCoordinates(coordinatesToUnload);
        }

        /// <summary>
        /// Moves a chunk to a different coordinate if a chunk doesn't already exist there
        /// </summary>
        /// <param name="fromCoordinate">The coordinate of the chunk to move</param>
        /// <param name="toCoordinate">The coordinate where the chunk should be moved</param>
        private void MoveChunk(int3 fromCoordinate, int3 toCoordinate)
        {
            if (!VoxelWorld.ChunkStore.TryGetChunkAtCoordinate(fromCoordinate, out Chunk chunk))
            {
                Debug.LogWarning($"No chunk at {fromCoordinate}, exiting the function");
                return;
            }

            if (VoxelWorld.ChunkStore.DoesChunkExistAtCoordinate(toCoordinate))
            {
                Debug.LogWarning($"A chunk already exists at {toCoordinate}, exiting the function");
                return;
            }

            var chunkDensities = VoxelWorld.VoxelDataGenerator.GenerateVoxelData(toCoordinate);
            VoxelWorld.VoxelDataStore.SetDensityChunk(chunkDensities, toCoordinate);

            VoxelWorld.ChunkStore.RemoveChunk(fromCoordinate);
            VoxelWorld.ChunkStore.AddChunk(toCoordinate, chunk);

            chunk.Initialize(toCoordinate, VoxelWorld);
        }
    }
}