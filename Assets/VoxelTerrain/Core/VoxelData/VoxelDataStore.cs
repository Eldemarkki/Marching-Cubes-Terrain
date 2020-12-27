using System;
using System.Collections.Generic;
using System.Linq;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

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
        /// Tries to get the voxel data from <paramref name="worldPosition"/>. If the position is not loaded, false will be returned and <paramref name="voxelData"/> will be set to 0 (Note that 0 doesn't directly mean that the position is not loaded). If it is loaded, true will be returned and <paramref name="voxelData"/> will be set to the value.
        /// </summary>
        /// <param name="worldPosition">The world position to get the voxel data from</param>
        /// <param name="voxelData">The voxel data value at the world position</param>
        /// <returns>Does a voxel data point exist at that position</returns>
        public bool TryGetVoxelData(int3 worldPosition, out byte voxelData)
        {
            int3 chunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldPosition, VoxelWorld.WorldSettings.ChunkSize);
            ApplyChunkChanges(chunkCoordinate);
            if (_data.TryGetValue(chunkCoordinate, out NativeArray<byte> chunk))
            {
                int3 voxelDataLocalPosition = worldPosition.Mod(VoxelWorld.WorldSettings.ChunkSize);
                return chunk.TryGetElement(VoxelWorld.WorldSettings.ChunkSize + 1, voxelDataLocalPosition, out voxelData);
            }
            else
            {
                voxelData = 0;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the voxel data array for one chunk with a persistent allocator. If a chunk doesn't exist there, false will be returned and <paramref name="chunk"/> will be set to null. If a chunk exists there, true will be returned and <paramref name="chunk"/> will be set to the chunk.
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose voxel data should be gotten</param>
        /// <param name="chunk">The voxel data of a chunk at the coordinate</param>
        /// <returns>Does a chunk exists at that coordinate</returns>
        public override bool TryGetDataChunk(int3 chunkCoordinate, out NativeArray<byte> chunk)
        {
            ApplyChunkChanges(chunkCoordinate);
            return TryGetDataChunkWithoutApplying(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Gets the voxel data of a custom volume in the world
        /// </summary>
        /// <param name="worldSpaceQuery">The world-space volume to get the voxel data for</param>
        /// <param name="allocator">How the new voxel data array should be allocated</param>
        /// <returns>The voxel data array inside the bounds</returns>
        public NativeArray<byte> GetVoxelDataCustom(BoundsInt worldSpaceQuery, Allocator allocator)
        {
            NativeArray<byte> voxelDataArray = new NativeArray<byte>(worldSpaceQuery.CalculateVolume(), allocator);

            ForEachVoxelDataArrayInQuery(worldSpaceQuery, (chunkCoordinate, voxelDataChunk) =>
            {
                ForEachVoxelDataInQueryInChunk(worldSpaceQuery, chunkCoordinate, voxelDataChunk, (voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData) =>
                {
                    voxelDataArray.SetElement(voxelData, worldSpaceQuery.size.ToInt3(), voxelDataWorldPosition - worldSpaceQuery.min.ToInt3());
                });
            });

            return voxelDataArray;
        }

        private bool TryGetDataChunkWithoutApplying(int3 chunkCoordinate, out NativeArray<byte> chunk)
        {
            return _data.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Sets the voxel data for a volume in the world
        /// </summary>
        /// <param name="voxelDataArray">The new voxel data array</param>
        /// <param name="originPosition">The world position of the origin where the voxel data should be set</param>
        public void SetVoxelDataCustom(NativeArray<byte> voxelDataArray, int3 voxelDataArrayDimensions, int3 originPosition)
        {
            BoundsInt worldSpaceQuery = new BoundsInt(originPosition.ToVectorInt(), (voxelDataArrayDimensions - new int3(1, 1, 1)).ToVectorInt());

            ForEachVoxelDataArrayInQuery(worldSpaceQuery, (chunkCoordinate, voxelDataChunk) =>
            {
                ForEachVoxelDataInQueryInChunk(worldSpaceQuery, chunkCoordinate, voxelDataChunk, (voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData) =>
                {
                    voxelDataChunk.SetElement(voxelData, voxelDataArrayDimensions, voxelDataWorldPosition - chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize);
                });

                if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
                {
                    chunkProperties.HasChanges = true;
                }
            });
        }

        /// <summary>
        /// Sets the voxel data for a volume in the world
        /// </summary>
        /// <param name="worldSpaceQuery">The volume where the voxel datas should be set to</param>
        /// <param name="setVoxelDataFunction">The function that calculates what the voxel data should be set to at the specific location. The first argument is the world space position of the voxel data, and the second argument is the current voxel data. The return value is what the new voxel data should be set to.</param>

        public void SetVoxelDataCustom(BoundsInt worldSpaceQuery, Func<int3, byte, byte> setVoxelDataFunction)
        {
            ForEachVoxelDataArrayInQuery(worldSpaceQuery, (chunkCoordinate, voxelDataChunk) =>
            {
                bool anyChanged = false;
                ForEachVoxelDataInQueryInChunk(worldSpaceQuery, chunkCoordinate, voxelDataChunk, (voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData) =>
                {
                    byte newVoxelData = setVoxelDataFunction(voxelDataWorldPosition, voxelData);
                    if (newVoxelData != voxelData)
                    {
                        voxelDataChunk.SetElement(newVoxelData, voxelDataIndex);
                        anyChanged = true;
                    }
                });

                if (anyChanged)
                {
                    if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
                    {
                        chunkProperties.HasChanges = true;
                    }
                }
            });
        }

        /// <summary>
        /// Sets the job handle for a chunk coordinate
        /// </summary>
        /// <param name="generationJobHandle">The job handle with data</param>
        /// <param name="chunkCoordinate">The coordinate of the chunk to set the job handle for</param>
        private void SetVoxelDataJobHandle(JobHandleWithData<IVoxelDataGenerationJob> generationJobHandle, int3 chunkCoordinate)
        {
            _generationJobHandles.Add(chunkCoordinate, generationJobHandle);
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

        protected override void GenerateDataForChunkUnchecked(int3 chunkCoordinate, NativeArray<byte> existingData)
        {
            int3 chunkWorldOrigin = chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize;
            JobHandleWithData<IVoxelDataGenerationJob> jobHandleWithData = VoxelWorld.VoxelDataGenerator.GenerateVoxelData(chunkWorldOrigin, VoxelWorld.WorldSettings.ChunkSize + 1, existingData);
            SetVoxelDataJobHandle(jobHandleWithData, chunkCoordinate);
        }
    }
}
