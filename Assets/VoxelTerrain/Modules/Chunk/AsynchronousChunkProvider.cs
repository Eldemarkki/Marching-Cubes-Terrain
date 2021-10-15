using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World.Chunks;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Chunks
{
    /// <summary>
    /// Provider for asynchronically generated chunks
    /// </summary>
    public class AsynchronousChunkProvider : ChunkProvider
    {
        /// <summary>
        /// The maximum amount of chunks that can be generated in one frame
        /// </summary>
        [SerializeField] private int chunkGenerationRate = 10;

        /// <summary>
        /// A queue that contains all the coordinates where a chunk will eventually have to be generated
        /// </summary>
        private UniqueQueue<int3> _generationQueue;

        private List<ChunkProperties> currentlyRunningJobs;

        private void Awake()
        {
            _generationQueue = new UniqueQueue<int3>();
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

            if (currentlyRunningJobs.Count > 0)
            {
                List<ChunkProperties> completedJobs = new List<ChunkProperties>();
                for (int i = currentlyRunningJobs.Count - 1; i >= 0; i--)
                {
                    if (currentlyRunningJobs[i] == null)
                    {
                        currentlyRunningJobs.RemoveAt(i);
                        continue;
                    }

                    var job = currentlyRunningJobs[i].MeshingJobHandle;
                    if (job != null)
                    {
                        if (job.JobHandle.IsCompleted)
                        {
                            completedJobs.Add(currentlyRunningJobs[i]);
                            currentlyRunningJobs.RemoveAt(i);
                        }
                    }
                }

                VoxelWorld.ChunkUpdater.FinalizeMultipleChunkJobs(completedJobs);
            }
        }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a new chunk is instantiated there, and its will eventually be generated
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public override ChunkProperties EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunk))
            {
                return chunk;
            }

            chunk = CreateUnloadedChunkToCoordinate(chunkCoordinate);
            AddChunkToGenerationQueue(chunkCoordinate);
            return chunk;
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