using System.Collections.Generic;
using System.Linq;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// A store which handles getting and setting the voxel data for the world
    /// </summary>
    public class VoxelDataStore : PerVoxelStore<byte>
    {
        /// <summary>
        /// A dictionary of all the ongoing voxel data generation jobs. Key is the chunk's coordinate, and the value is the ongoing job for that chunk
        /// </summary>
        private Dictionary<int3, JobHandleWithData<IVoxelDataGenerationJob>> _generationJobHandles;

        protected override void Awake()
        {
            base.Awake();
            _generationJobHandles = new Dictionary<int3, JobHandleWithData<IVoxelDataGenerationJob>>();
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();

            if (_generationJobHandles != null)
            {
                foreach (var jobHandle in _generationJobHandles.Values)
                {
                    jobHandle.JobHandle.Complete();
                    jobHandle.JobData.OutputVoxelData.Dispose();
                }

                _generationJobHandles.Clear();
            }
        }

        /// <summary>
        /// Checks if a chunk exists or is currently being generated at the coordinate <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate to check for</param>
        /// <returns>Returns true if a chunk exists or is currently being generated at <paramref name="chunkCoordinate"/>, otherwise returns false</returns>
        public override bool DoesChunkExistAtCoordinate(int3 chunkCoordinate)
        {
            return base.DoesChunkExistAtCoordinate(chunkCoordinate) || _generationJobHandles.ContainsKey(chunkCoordinate);
        }

        /// <summary>
        /// Gets all the coordinates of the chunks that already exist or are currently being generated, where the Chebyshev distance from <paramref name="coordinate"/> to the chunk's coordinate is more than <paramref name="range"/>
        /// </summary>
        /// <param name="coordinate">The central coordinate where the distances should be measured from</param>
        /// <param name="range">The maximum allowed manhattan distance</param>
        /// <returns></returns>
        public override IEnumerable<int3> GetChunkCoordinatesOutsideOfRange(int3 coordinate, int range)
        {
            foreach(var baseCoordinate in base.GetChunkCoordinatesOutsideOfRange(coordinate, range))
            {
                yield return baseCoordinate;
            }

            int3[] generationJobHandleArray = _generationJobHandles.Keys.ToArray();
            for (int i = 0; i < generationJobHandleArray.Length; i++)
            {
                int3 generationCoordinate = generationJobHandleArray[i];
                if (DistanceUtilities.ChebyshevDistanceGreaterThan(coordinate, generationCoordinate, range))
                {
                    yield return generationCoordinate;
                }
            }
        }

        /// <summary>
        /// Tries to get the voxel data array for one chunk with a persistent allocator. If a chunk doesn't exist there, false will be returned and <paramref name="chunk"/> will be set to null. If a chunk exists there, true will be returned and <paramref name="chunk"/> will be set to the chunk. If the data for that chunk is currently being calculated, the job will complete and the new data will be set to <paramref name="chunk"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose voxel data should be gotten</param>
        /// <param name="chunk">The voxel data of a chunk at the coordinate</param>
        /// <returns>Does a chunk exists at that coordinate</returns>
        public override bool TryGetDataChunk(int3 chunkCoordinate, out VoxelDataVolume<byte> chunk)
        {
            ApplyChunkChanges(chunkCoordinate);
            return TryGetDataChunkWithoutApplying(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Tries to get the voxel data array for one chunk with a persistent allocator. If a chunk doesn't exist there, false will be returned and <paramref name="chunk"/> will be set to null. If a chunk exists there, true will be returned and <paramref name="chunk"/> will be set to the chunk. If the data for that chunk is currently being calculated, the job will NOT be completed.
        /// <param name="chunkCoordinate">The coordinate of the chunk whose voxel data should be gotten</param>
        /// <param name="chunk">The voxel data of a chunk at the coordinate</param>
        /// <returns>Does a chunk exists at that coordinate</returns>
        private bool TryGetDataChunkWithoutApplying(int3 chunkCoordinate, out VoxelDataVolume<byte> chunk)
        {
            return _chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// If the chunk coordinate has an ongoing voxel data generation job, it will get completed and it's result will be applied to the chunk
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk to apply changes for</param>
        private void ApplyChunkChanges(int3 chunkCoordinate)
        {
            if (_generationJobHandles.TryGetValue(chunkCoordinate, out JobHandleWithData<IVoxelDataGenerationJob> jobHandle))
            {
                jobHandle.JobHandle.Complete();
                SetDataChunk(chunkCoordinate, jobHandle.JobData.OutputVoxelData);
                _generationJobHandles.Remove(chunkCoordinate);
            }
        }

        public override void SetDataChunk(int3 chunkCoordinate, VoxelDataVolume<byte> newData)
        {
            if (TryGetDataChunkWithoutApplying(chunkCoordinate, out VoxelDataVolume<byte> oldData))
            {
                oldData.CopyFrom(newData);
                newData.Dispose();
            }
            else
            {
                AddChunkUnchecked(chunkCoordinate, newData);
            }

            if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
            {
                chunkProperties.HasChanges = true;
            }
        }

        public override void GenerateDataForChunkUnchecked(int3 chunkCoordinate, VoxelDataVolume<byte> existingData)
        {
            int3 chunkWorldOrigin = chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize;
            JobHandleWithData<IVoxelDataGenerationJob> jobHandleWithData = VoxelWorld.VoxelDataGenerator.GenerateVoxelData(chunkWorldOrigin, existingData);
            _generationJobHandles.Add(chunkCoordinate, jobHandleWithData);
        }
    }
}
