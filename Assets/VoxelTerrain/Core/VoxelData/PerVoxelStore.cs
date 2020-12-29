using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.Utilities.Intersection;
using Eldemarkki.VoxelTerrain.VoxelData;
using Eldemarkki.VoxelTerrain.World.Chunks;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A store that contains a single variable of type <typeparamref name="T"/> for each voxel data point in a chunk
    /// </summary>
    /// <typeparam name="T">The type of variable to associate with each voxel data point</typeparam>
    public abstract class PerVoxelStore<T> : PerChunkStore<VoxelDataVolume<T>> where T : struct
    {
        protected virtual void OnApplicationQuit()
        {
            foreach (VoxelDataVolume<T> dataArray in _chunks.Values)
            {
                if (dataArray.IsCreated)
                {
                    dataArray.Dispose();
                }
            }
        }

        /// <summary>
        /// Set's the data value of the voxel data point at <paramref name="dataWorldPosition"/> to <paramref name="dataValue"/>
        /// </summary>
        /// <param name="dataWorldPosition">The world position of the data point</param>
        /// <param name="dataValue">The new data value of the data point</param>
        public void SetData(int3 dataWorldPosition, T dataValue)
        {
            int3[] affectedChunkCoordinates = CoordinateUtilities.GetChunkCoordinatesContainingPoint(dataWorldPosition, VoxelWorld.WorldSettings.ChunkSize);

            for (int i = 0; i < affectedChunkCoordinates.Length; i++)
            {
                int3 chunkCoordinate = affectedChunkCoordinates[i];
                if (TryGetDataChunk(chunkCoordinate, out VoxelDataVolume<T> chunkData))
                {
                    int3 localPos = (dataWorldPosition - chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize).Mod(VoxelWorld.WorldSettings.ChunkSize + 1);

                    chunkData.SetVoxelData(dataValue, localPos);

                    if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
                    {
                        chunkProperties.HasChanges = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the chunk data of the chunk at <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk</param>
        /// <param name="newData">The data to set the chunk's data to</param>
        public virtual void SetDataChunk(int3 chunkCoordinate, VoxelDataVolume<T> newData)
        {
            bool dataExistsAtCoordinate = DoesChunkExistAtCoordinate(chunkCoordinate);
            SetDataChunkUnchecked(chunkCoordinate, newData, dataExistsAtCoordinate);
        }

        /// <summary>
        /// Sets the chunk data of the chunk at <paramref name="chunkCoordinate"/> without checking if data already exists at <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk</param>
        /// <param name="newData">The data to set the chunk's data to</param>
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
                _chunks.Add(chunkCoordinate, newData);
            }

            if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
            {
                chunkProperties.HasChanges = true;
            }
        }

        /// <inheritdoc/>
        public override void GenerateDataForChunkUnchecked(int3 chunkCoordinate)
        {
            VoxelDataVolume<T> data = new VoxelDataVolume<T>(VoxelWorld.WorldSettings.ChunkSize + 1, Allocator.Persistent);
            GenerateDataForChunkUnchecked(chunkCoordinate, data);
        }

        /// <summary>
        /// Loops through each voxel data point that is contained in <paramref name="dataChunk"/> AND in <paramref name="worldSpaceQuery"/>, and performs <paramref name="function"/> on it
        /// </summary>
        /// <param name="worldSpaceQuery">The query that determines whether or not a voxel data point is contained.</param>
        /// <param name="chunkCoordinate">The coordinate of <paramref name="dataChunk"/></param>
        /// <param name="dataChunk">The voxel datas of the chunk</param>
        /// <param name="function">The function that will be performed on each voxel data point. The arguments are as follows: 1) The world space position of the voxel data point, 2) The chunk space position of the voxel data point, 3) The index of the voxel data point inside of <paramref name="dataChunk"/>, 4) The value of the voxel data</param>
        public void ForEachVoxelDataInQueryInChunk(BoundsInt worldSpaceQuery, int3 chunkCoordinate, VoxelDataVolume<T> dataChunk, Action<int3, int3, int, T> function)
        {
            int3 chunkBoundsSize = VoxelWorld.WorldSettings.ChunkSize;
            int3 chunkWorldSpaceOrigin = chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize;

            BoundsInt chunkWorldSpaceBounds = new BoundsInt(chunkWorldSpaceOrigin.ToVectorInt(), chunkBoundsSize.ToVectorInt());

            BoundsInt intersectionVolume = IntersectionUtilities.GetIntersectionVolume(worldSpaceQuery, chunkWorldSpaceBounds);
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
                        if (dataChunk.TryGetVoxelData(voxelDataIndex, out T voxelData))
                        {
                            function(voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to get the voxel data from <paramref name="worldPosition"/>. If the position is not loaded, false will be returned and <paramref name="voxelData"/> will be set to default(T) (Note that default(T) doesn't directly mean that the position is not loaded). If it is loaded, true will be returned and <paramref name="voxelData"/> will be set to the value at <paramref name="worldPosition"/>.
        /// </summary>
        /// <param name="worldPosition">The world position to get the voxel data from</param>
        /// <param name="voxelData">The voxel data value at the world position</param>
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
            VoxelDataVolume<T> voxelDataArray = new VoxelDataVolume<T>(worldSpaceQuery.CalculateVolume(), allocator);

            ForEachVoxelDataArrayInQuery(worldSpaceQuery, (chunkCoordinate, voxelDataChunk) =>
            {
                ForEachVoxelDataInQueryInChunk(worldSpaceQuery, chunkCoordinate, voxelDataChunk, (voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData) =>
                {
                    voxelDataArray.SetVoxelData(voxelData, voxelDataWorldPosition - worldSpaceQuery.min.ToInt3());
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
            BoundsInt worldSpaceQuery = new BoundsInt(originPosition.ToVectorInt(), (voxelDataArray.Size - new int3(1, 1, 1)).ToVectorInt());

            ForEachVoxelDataArrayInQuery(worldSpaceQuery, (chunkCoordinate, voxelDataChunk) =>
            {
                ForEachVoxelDataInQueryInChunk(worldSpaceQuery, chunkCoordinate, voxelDataChunk, (voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData) =>
                {
                    voxelDataChunk.SetVoxelData(voxelData, voxelDataWorldPosition - chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize);
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
        public void SetVoxelDataCustom(BoundsInt worldSpaceQuery, Func<int3, T, T> setVoxelDataFunction)
        {
            ForEachVoxelDataArrayInQuery(worldSpaceQuery, (chunkCoordinate, voxelDataChunk) =>
            {
                bool anyChanged = false;
                ForEachVoxelDataInQueryInChunk(worldSpaceQuery, chunkCoordinate, voxelDataChunk, (voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData) =>
                {
                    T newVoxelData = setVoxelDataFunction(voxelDataWorldPosition, voxelData);
                    if (!newVoxelData.Equals(voxelData))
                    {
                        voxelDataChunk.SetVoxelData(newVoxelData, voxelDataIndex);
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
    }
}