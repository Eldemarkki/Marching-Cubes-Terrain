using Eldemarkki.VoxelTerrain.World.Chunks;
using System.Collections.Generic;
using Unity.Jobs;
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
        /// A queue that contains all the coordinates where a chunk will eventually have to be generated
        /// </summary>
        private Queue<int3> _generationQueue;

        private List<ChunkProperties> currentlyRunningJobs;

        private void Awake()
        {
            _generationQueue = new Queue<int3>();
            currentlyRunningJobs = new List<ChunkProperties>();
        }

        private void Update()
        {
            var chunksGenerated = 0;
            while (_generationQueue.Count > 0 && chunksGenerated < chunkGenerationRate)
            {
                var chunkCoordinate = _generationQueue.Dequeue();

                if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
                {
                    if (!chunkProperties.IsMeshGenerated)
                    {
                        VoxelWorld.ChunkUpdater.StartGeneratingChunk(chunkProperties);
                        currentlyRunningJobs.Add(chunkProperties);

                        chunksGenerated++;
                    }
                }
            }

            if (chunksGenerated > 0)
            {
                JobHandle.ScheduleBatchedJobs();
            }

            for (int i = currentlyRunningJobs.Count - 1; i >= 0; i--)
            {
                if(currentlyRunningJobs[i] == null)
                {
                    currentlyRunningJobs.RemoveAt(i);
                    continue;
                }

                var job = currentlyRunningJobs[i].MeshingJobHandle;
                if (job != null)
                {
                    if (job.JobHandle.IsCompleted)
                    {
                        VoxelWorld.ChunkUpdater.FinalizeChunkJob(currentlyRunningJobs[i]);
                        currentlyRunningJobs.RemoveAt(i);
                    }
                }
            }
        }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a new chunk is instantiated there, and its will eventually be generated
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (!VoxelWorld.ChunkStore.DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                CreateUnloadedChunkToCoordinate(chunkCoordinate);
                AddChunkToGenerationQueue(chunkCoordinate);
            }
        }

        /// <summary>
        /// Adds the coordinate <paramref name="chunkCoordinate"/> to the list of chunks that will eventually have to be generated
        /// </summary>
        /// <param name="chunkCoordinate"></param>
        public void AddChunkToGenerationQueue(int3 chunkCoordinate)
        {
            _generationQueue.Enqueue(chunkCoordinate);
        }
    }
}