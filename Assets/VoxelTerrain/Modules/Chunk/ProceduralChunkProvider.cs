using Eldemarkki.VoxelTerrain.VoxelData;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World.Chunks;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Chunks
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
        /// A queue that contains the chunks' coordinates that are out of view and thus ready to be moved elsewhere
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

        private void Update()
        {
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
        }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a new chunk is created later
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public override void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (VoxelWorld.ChunkStore.DoesChunkExistAtCoordinate(chunkCoordinate)) { return; }
            if (_generationQueue.Contains(chunkCoordinate)) { return; }

            _generationQueue.Add(chunkCoordinate);
        }

        /// <summary>
        /// Unloads every coordinate in <paramref name="coordinatesToUnload"/>
        /// </summary>
        /// <param name="coordinatesToUnload">A collection of the chunk coordinates to unload</param>
        public void UnloadCoordinates(IEnumerable<int3> coordinatesToUnload)
        {
            // Mark coordinates as available
            foreach(int3 chunkCoordinate in coordinatesToUnload)
            {
                if (!_availableChunkCoordinates.Contains(chunkCoordinate))
                {
                    _availableChunkCoordinates.Enqueue(chunkCoordinate);
                }

                if (_generationQueue.Contains(chunkCoordinate))
                {
                    _generationQueue.Remove(chunkCoordinate);
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
                Debug.LogWarning($"No chunk at {fromCoordinate.ToString()}, exiting the function");
                return;
            }

            if (VoxelWorld.ChunkStore.DoesChunkExistAtCoordinate(toCoordinate))
            {
                Debug.LogWarning($"A chunk already exists at {toCoordinate.ToString()}, exiting the function");
                return;
            }

            Bounds generationBounds = BoundsUtilities.GetChunkBounds(toCoordinate, VoxelWorld.WorldSettings.ChunkSize);
            JobHandleWithData<IVoxelDataGenerationJob> jobHandleWithData = VoxelWorld.VoxelDataGenerator.GenerateVoxelData(generationBounds);
            jobHandleWithData.JobHandle.Complete();
            VoxelWorld.VoxelDataStore.SetVoxelDataChunk(jobHandleWithData.JobData.OutputVoxelData, toCoordinate);

            VoxelWorld.ChunkStore.RemoveChunk(fromCoordinate);

            chunk.Initialize(toCoordinate, VoxelWorld);
            VoxelWorld.ChunkStore.AddChunk(chunk);
        }
    }
}