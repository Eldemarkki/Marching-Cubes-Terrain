using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.Utilities.Intersection;
using Eldemarkki.VoxelTerrain.VoxelData;
using Eldemarkki.VoxelTerrain.World.Chunks;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A store that contains a single variable of type <typeparamref name="T"/> for each voxel data point in a chunk
    /// </summary>
    public abstract class PerVoxelStore<T> : PerChunkStore<VoxelDataVolume<T>>, IDisposable where T : struct
    {
        /// <summary>
        /// A dictionary of all the ongoing voxel data generation jobs. Key is the chunk's coordinate, and the value is the ongoing job for that chunk
        /// </summary>
        public Dictionary<int3, JobHandleWithData<IVoxelDataGenerationJob<T>>> _generationJobHandles;

        protected override void Awake()
        {
            base.Awake();
            _generationJobHandles = new Dictionary<int3, JobHandleWithData<IVoxelDataGenerationJob<T>>>();
        }

        /// <summary>
        /// Set's the data value of the voxel data point at <paramref name="dataWorldPosition"/> to <paramref name="dataValue"/>
        /// </summary>
        public void SetData(int3 dataWorldPosition, T dataValue)
        {
            int3[] affectedChunkCoordinates = CoordinateUtilities.CalculateChunkCoordinatesContainingPoint(dataWorldPosition, VoxelWorld.WorldSettings.ChunkSize);

            for (int i = 0; i < affectedChunkCoordinates.Length; i++)
            {
                int3 chunkCoordinate = affectedChunkCoordinates[i];
                if (TryGetDataChunk(chunkCoordinate, out VoxelDataVolume<T> chunkData))
                {
                    int3 localPos = (dataWorldPosition - chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize).Mod(VoxelWorld.WorldSettings.ChunkSize + 1);

                    chunkData[localPos] = dataValue;

                    if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
                    {
                        VoxelWorld.ChunkUpdater.SetChunkDirty(chunkProperties);
                    }
                }
            }
        }

        /// <summary>
        /// Tries to get the voxel data array for one chunk. If a chunk doesn't exist there, false will be returned and <paramref name="chunk"/> will be set to null. If a chunk exists there, true will be returned and <paramref name="chunk"/> will be set to the chunk. If the data for that chunk is currently being calculated, the job will complete and the new data will be set to <paramref name="chunk"/>
        /// </summary>
        /// <returns>Does a chunk exist at that coordinate</returns>
        public override bool TryGetDataChunk(int3 chunkCoordinate, out VoxelDataVolume<T> chunk)
        {
            ApplyChunkChanges(chunkCoordinate);
            return TryGetDataChunkWithoutApplyingChanges(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// If the chunk coordinate has an ongoing voxel data generation job, it will get completed and it's result will be applied to the chunk
        /// </summary>
        public void ApplyChunkChanges(int3 chunkCoordinate)
        {
            if (_generationJobHandles.TryGetValue(chunkCoordinate, out JobHandleWithData<IVoxelDataGenerationJob<T>> jobHandle))
            {
                jobHandle.JobHandle.Complete();
                SetDataChunk(chunkCoordinate, jobHandle.JobData.OutputVoxelData);
                _generationJobHandles.Remove(chunkCoordinate);
            }
        }

        /// <summary>
        /// Tries to get the voxel data array for one chunk. If a chunk doesn't exist there, false will be returned and <paramref name="chunk"/> will be set to null. If a chunk exists there, true will be returned and <paramref name="chunk"/> will be set to the chunk. If the data for that chunk is currently being calculated, the job will NOT be completed.
        /// </summary>
        /// <returns>Does a chunk exist at that coordinate</returns>
        public bool TryGetDataChunkWithoutApplyingChanges(int3 chunkCoordinate, out VoxelDataVolume<T> chunk)
        {
            return _chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Tries to get the voxel data array for one chunk. If a chunk doesn't exist there, false will be returned and <paramref name="chunk"/> will be set to null. If a chunk exists there, true will be returned and <paramref name="chunk"/> will be set to the chunk. This also checks the chunk generation queue, so a chunk that is currently being generated may also be returned.
        /// <returns>Does a chunk exist at that coordinate</returns>
        public bool TryGetDataChunkWithoutApplyingChangesIncludeQueue(int3 chunkCoordinate, out VoxelDataVolume<T> chunk)
        {
            if (TryGetDataChunkWithoutApplyingChanges(chunkCoordinate, out chunk))
            {
                return true;
            }

            if (_generationJobHandles.TryGetValue(chunkCoordinate, out var job))
            {
                chunk = job.JobData.OutputVoxelData;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Gets all the coordinates of the chunks that already exist or are currently being generated, where the Chebyshev distance from <paramref name="coordinate"/> to the chunk's coordinate is more than <paramref name="range"/>
        /// </summary>
        /// <param name="coordinate">The central coordinate where the distances should be measured from</param>
        /// <param name="range">The maximum allowed Chebyshev distance</param>
        public override List<int3> GetChunkCoordinatesOutsideOfRange(int3 coordinate, int range)
        {
            List<int3> result = base.GetChunkCoordinatesOutsideOfRange(coordinate, range);

            int3[] generationJobHandleArray = _generationJobHandles.Keys.ToArray();
            for (int i = 0; i < generationJobHandleArray.Length; i++)
            {
                int3 generationCoordinate = generationJobHandleArray[i];
                if (DistanceUtilities.ChebyshevDistanceGreaterThan(coordinate, generationCoordinate, range))
                {
                    result.Add(generationCoordinate);
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if a chunk exists or is currently being generated at <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <returns>Returns true if a chunk exists or is currently being generated at <paramref name="chunkCoordinate"/>, otherwise returns false</returns>
        public override bool DoesChunkExistAtCoordinate(int3 chunkCoordinate)
        {
            return base.DoesChunkExistAtCoordinate(chunkCoordinate) || _generationJobHandles.ContainsKey(chunkCoordinate);
        }

        protected override bool TryGetDefaultOrJobHandle(int3 chunkCoordinate, out JobHandle jobHandle)
        {
            if (base.DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                jobHandle = default;
                return true;
            }
            else
            {
                bool exists = _generationJobHandles.TryGetValue(chunkCoordinate, out var job);
                jobHandle = exists ? job.JobHandle : default;
                return exists;
            }
        }

        public void SetDataChunk(int3 chunkCoordinate, VoxelDataVolume<T> newData)
        {
            if (TryGetDataChunkWithoutApplyingChanges(chunkCoordinate, out VoxelDataVolume<T> oldData))
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
                chunkProperties.IsMeshGenerated = false;
            }
        }

        /// <summary>
        /// Sets the chunk data of the chunk at <paramref name="chunkCoordinate"/> without checking if data already exists at <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="dataExistsAtCoordinate">Does data already exist at <paramref name="chunkCoordinate"/></param>
        public virtual void SetDataChunkUnchecked(int3 chunkCoordinate, VoxelDataVolume<T> newData, bool dataExistsAtCoordinate)
        {
            if (dataExistsAtCoordinate)
            {
                _chunks[chunkCoordinate].CopyFrom(newData);
                newData.Dispose();
            }
            else
            {
                AddChunkUnchecked(chunkCoordinate, newData);
            }

            if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
            {
                chunkProperties.IsMeshGenerated = false;
            }
        }

        /// <inheritdoc/>
        public override JobHandle GenerateDataForChunkUnchecked(int3 chunkCoordinate)
        {
            VoxelDataVolume<T> data = new VoxelDataVolume<T>(VoxelWorld.WorldSettings.ChunkSize + 1, Allocator.Persistent);
            return GenerateDataForChunkUnchecked(chunkCoordinate, data);
        }

        /// <inheritdoc/>
        public override JobHandle GenerateDataForChunkUnchecked(int3 chunkCoordinate, VoxelDataVolume<T> existingData, JobHandle dependency = default)
        {
            var jobHandleWithData = ScheduleGenerationJob(chunkCoordinate, existingData, dependency);
            _generationJobHandles.Add(chunkCoordinate, jobHandleWithData);
            return jobHandleWithData.JobHandle;
        }

        protected abstract JobHandleWithData<IVoxelDataGenerationJob<T>> ScheduleGenerationJob(int3 chunkCoordinate, VoxelDataVolume<T> existingData, JobHandle dependency = default);

        /// <summary>
        /// Loops through each voxel data point that is contained in <paramref name="dataChunk"/> AND in <paramref name="worldSpaceQuery"/>, and performs <paramref name="function"/> on it
        /// </summary>
        /// <param name="worldSpaceQuery">The query that determines whether or not a voxel data point is contained.</param>
        /// <param name="chunkCoordinate">The coordinate of <paramref name="dataChunk"/></param>
        /// <param name="dataChunk">The voxel datas of the chunk</param>
        /// <param name="function">The function that will be performed on each voxel data point. The arguments are as follows: 1) The world space position of the voxel data point, 2) The chunk space (local) position of the voxel data point, 3) The index of the voxel data point inside of <paramref name="dataChunk"/>, 4) The value of the voxel data</param>
        /// <param name="getOriginalVoxelData">Should the original voxel datas be retrieved? i.e. Do you need them inside <paramref name="function"/>? If this is set to false, then the fourth parameter in <paramref name="function"/> will be set to <c>default(T)</c></param>
        public void ForEachVoxelDataInQueryInChunk(BoundsInt worldSpaceQuery, int3 chunkCoordinate, VoxelDataVolume<T> dataChunk, Action<int3, int3, int, T> function, bool getOriginalVoxelData = true)
        {
            if (_generationJobHandles.ContainsKey(chunkCoordinate))
            {
                return;
            }

            int3 chunkBoundsSize = VoxelWorld.WorldSettings.ChunkSize;
            int3 chunkWorldSpaceOrigin = chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize;

            BoundsInt chunkWorldSpaceBounds = new BoundsInt(chunkWorldSpaceOrigin.ToVectorInt(), chunkBoundsSize.ToVectorInt());

            BoundsInt intersectionVolume = IntersectionUtilities.CalculateIntersectionVolume(worldSpaceQuery, chunkWorldSpaceBounds);
            int3 intersectionVolumeMin = intersectionVolume.min.ToInt3();
            int3 intersectionVolumeMax = intersectionVolume.max.ToInt3();

            for (int voxelDataWorldPositionX = intersectionVolumeMin.x; voxelDataWorldPositionX <= intersectionVolumeMax.x; voxelDataWorldPositionX++)
            {
                for (int voxelDataWorldPositionY = intersectionVolumeMin.y; voxelDataWorldPositionY <= intersectionVolumeMax.y; voxelDataWorldPositionY++)
                {
                    for (int voxelDataWorldPositionZ = intersectionVolumeMin.z; voxelDataWorldPositionZ <= intersectionVolumeMax.z; voxelDataWorldPositionZ++)
                    {
                        int3 voxelDataWorldPosition = new int3(voxelDataWorldPositionX, voxelDataWorldPositionY, voxelDataWorldPositionZ);

                        int3 voxelDataLocalPosition = voxelDataWorldPosition - chunkWorldSpaceOrigin;
                        int voxelDataIndex = IndexUtilities.XyzToIndex(voxelDataLocalPosition, chunkBoundsSize.x + 1, chunkBoundsSize.y + 1);
                        T voxelData = getOriginalVoxelData ? dataChunk[voxelDataIndex] : default;

                        function(voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData);
                    }
                }
            }
        }

        public override void MoveChunk(int3 from, int3 to)
        {
            // Check that 'from' and 'to' are not equal
            if (from.Equals(to)) { return; }

            // Check that a chunk does NOT already exist at 'to'
            if (DoesChunkExistAtCoordinate(to)) { return; }

            // Check that a chunk exists at 'from'
            bool chunkExistsAtFrom = _chunks.TryGetValue(from, out var existingData);

            if (_generationJobHandles.TryGetValue(from, out JobHandleWithData<IVoxelDataGenerationJob<T>> jobHandle))
            {
                jobHandle.JobHandle.Complete();
                if (chunkExistsAtFrom)
                {
                    jobHandle.JobData.OutputVoxelData.Dispose();
                }
                else
                {
                    existingData = jobHandle.JobData.OutputVoxelData;
                    chunkExistsAtFrom = true;
                }

                _generationJobHandles.Remove(from);
            }

            if (chunkExistsAtFrom)
            {
                JobHandle meshingJobHandle = default;
                if (VoxelWorld.ChunkStore.TryGetDataChunk(from, out var chunk) && chunk.MeshingJobHandle != null)
                {
                    meshingJobHandle = chunk.MeshingJobHandle.JobHandle;
                }

                RemoveChunkUnchecked(from);
                GenerateDataForChunkUnchecked(to, existingData, meshingJobHandle);
            }
        }

        /// <summary>
        /// Tries to get the voxel data at <paramref name="worldPosition"/>. If the position is not loaded, false will be returned and <paramref name="voxelData"/> will be set to <c>default(T)</c> (Note that <c>default(T)</c> doesn't always mean that the position is not loaded). If it is loaded, true will be returned and <paramref name="voxelData"/> will be set to the value at <paramref name="worldPosition"/>.
        /// </summary>
        /// <returns>Does a voxel data point exist at that position</returns>
        public bool TryGetVoxelData(int3 worldPosition, out T voxelData)
        {
            int3 chunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldPosition, VoxelWorld.WorldSettings.ChunkSize);
            if (TryGetDataChunk(chunkCoordinate, out VoxelDataVolume<T> chunk))
            {
                int3 voxelDataLocalPosition = worldPosition.Mod(VoxelWorld.WorldSettings.ChunkSize);
                return chunk.TryGetVoxelData(voxelDataLocalPosition, out voxelData);
            }
            else
            {
                voxelData = default;
                return false;
            }
        }

        /// <summary>
        /// Gets the voxel data of a custom volume in the world
        /// </summary>
        /// <param name="worldSpaceQuery">The world-space volume to get the voxel data for</param>
        /// <param name="allocator">How the new voxel data array should be allocated</param>
        /// <returns>The voxel data array inside the bounds</returns>
        public VoxelDataVolume<T> GetVoxelDataCustom(BoundsInt worldSpaceQuery, Allocator allocator)
        {
            VoxelDataVolume<T> voxelDataArray = new VoxelDataVolume<T>(worldSpaceQuery.CalculateBoundsVolume(), allocator);

            ForEachVoxelDataArrayInQuery(worldSpaceQuery, (chunkCoordinate, voxelDataChunk) =>
            {
                ForEachVoxelDataInQueryInChunk(worldSpaceQuery, chunkCoordinate, voxelDataChunk, (voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData) =>
                {
                    voxelDataArray[voxelDataWorldPosition - worldSpaceQuery.min.ToInt3()] = voxelData;
                });
            });

            return voxelDataArray;
        }

        /// <summary>
        /// Sets the voxel data for a volume in the world
        /// </summary>
        /// <param name="voxelDataArray">The new voxel data array</param>
        /// <param name="originPosition">The world position of the origin where the voxel data should be set</param>
        public void SetVoxelDataCustom(VoxelDataVolume<T> voxelDataArray, int3 originPosition)
        {
            BoundsInt worldSpaceQuery = new BoundsInt(originPosition.ToVectorInt(), (voxelDataArray.Size - 1).ToVectorInt());
            SetVoxelDataCustom(worldSpaceQuery, (voxelDataWorldPosition, voxelData) => voxelDataArray[voxelDataWorldPosition - originPosition], false);
        }

        /// <summary>
        /// Sets the voxel data for a volume in the world
        /// </summary>
        /// <param name="worldSpaceQuery">The volume where the voxel datas should be set to</param>
        /// <param name="setVoxelDataFunction">The function that calculates what the voxel data should be set to at the specific location. The first argument is the world space position of the voxel data, and the second argument is the current voxel data. The return value is what the new voxel data should be set to.</param>
        /// <param name="getOriginalVoxelData">Should the original voxel datas be retrieved? i.e. Do you need them inside <paramref name="setVoxelDataFunction"/>? If this is set to false, then the second parameter of <paramref name="setVoxelDataFunction"/> will be set to <c>default(T)</c></param>
        public void SetVoxelDataCustom(BoundsInt worldSpaceQuery, Func<int3, T, T> setVoxelDataFunction, bool getOriginalVoxelData = true)
        {
            ForEachVoxelDataArrayInQuery(worldSpaceQuery, (chunkCoordinate, voxelDataChunk) =>
            {
                ForEachVoxelDataInQueryInChunk(worldSpaceQuery, chunkCoordinate, voxelDataChunk, (voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData) =>
                {
                    voxelDataChunk[voxelDataIndex] = setVoxelDataFunction(voxelDataWorldPosition, voxelData);
                }, getOriginalVoxelData);

                if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
                {
                    VoxelWorld.ChunkUpdater.SetChunkDirty(chunkProperties);
                }
            });
        }

        public void Dispose()
        {
            foreach (VoxelDataVolume<T> dataArray in Chunks)
            {
                if (dataArray.IsCreated)
                {
                    dataArray.Dispose();
                }
            }

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
    }
}