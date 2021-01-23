using Eldemarkki.VoxelTerrain.World.Chunks;
using System.Collections.Generic;
using Eldemarkki.VoxelTerrain.Meshing;
using Eldemarkki.VoxelTerrain.Utilities;
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

        private void Awake()
        {
            _generationQueue = new Queue<int3>();
        }

        private void Update()
        {
            if (_generationQueue.Count == 0)
            {
                return;
            }

            var jobs = new JobHandleWithDataAndChunkProperties<IMesherJob>[math.min(_generationQueue.Count, chunkGenerationRate)];

            var chunksGenerated = 0;
            while (_generationQueue.Count > 0 && chunksGenerated < chunkGenerationRate)
            {
                var chunkCoordinate = _generationQueue.Dequeue();

                if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
                {
                    if (!chunkProperties.IsMeshGenerated)
                    {
                        var job = VoxelWorld.ChunkUpdater.StartGeneratingChunk(chunkProperties);
                        jobs[chunksGenerated] = job;

                        chunksGenerated++;
                    }
                }
            }

            JobHandle.ScheduleBatchedJobs();
            while (true)
            {
                bool atleastOneJobRunning = false;
                for (int i = 0; i < chunksGenerated; i++)
                {
                    bool currentJobRunning = !jobs[i].JobHandle.IsCompleted;
                    atleastOneJobRunning |= currentJobRunning;
                }

                if (!atleastOneJobRunning)
                {
                    break;
                }
            }

            VoxelWorld.ChunkUpdater.FinalizeMultipleChunkJobs(jobs, chunksGenerated);
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