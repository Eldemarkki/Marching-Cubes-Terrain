using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.Utilities.Intersection;
using Eldemarkki.VoxelTerrain.World.Chunks;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    public abstract class PerVoxelStore<T> : PerChunkStore<NativeArray<T>> where T : struct
    {
        protected virtual void OnApplicationQuit()
        {
            foreach (NativeArray<T> dataArray in _data.Values)
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
        /// <param name="dataWorldPosition">The world position of the corner</param>
        /// <param name="dataValue">The new data value of the corner</param>
        public void SetData(int3 dataWorldPosition, T dataValue)
        {
            IEnumerable<int3> affectedChunkCoordinates = ChunkUtilities.GetChunkCoordinatesContainingPoint(dataWorldPosition, VoxelWorld.WorldSettings.ChunkSize);

            foreach (int3 chunkCoordinate in affectedChunkCoordinates)
            {
                if (TryGetDataChunk(chunkCoordinate, out NativeArray<T> chunkData))
                {
                    int3 localPos = (dataWorldPosition - chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize).Mod(VoxelWorld.WorldSettings.ChunkSize + 1);

                    int index = IndexUtilities.XyzToIndex(localPos, VoxelWorld.WorldSettings.ChunkSize.x + 1, VoxelWorld.WorldSettings.ChunkSize.y + 1);

                    chunkData[index] = dataValue;

                    if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
                    {
                        chunkProperties.HasChanges = true;
                    }
                }
            }
        }

        public override void GenerateDataForChunk(int3 chunkCoordinate)
        {
            if (!DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                NativeArray<T> data = new NativeArray<T>((VoxelWorld.WorldSettings.ChunkSize.x + 1) * (VoxelWorld.WorldSettings.ChunkSize.y + 1) * (VoxelWorld.WorldSettings.ChunkSize.z + 1), Allocator.Persistent);

                GenerateDataForChunkUnchecked(chunkCoordinate, data);
            }
        }

        public override void GenerateDataForChunk(int3 chunkCoordinate, NativeArray<T> existingData)
        {
            if (!DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                GenerateDataForChunkUnchecked(chunkCoordinate, existingData);
            }
        }

        /// <summary>
        /// Generates the colors for a chunk at <paramref name="chunkCoordinate"/>; fills the color array with the default color
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the colors for</param>
        protected abstract void GenerateDataForChunkUnchecked(int3 chunkCoordinate, NativeArray<T> existingData);

        /// <summary>
        /// Sets the voxel colors of a chunk at <paramref name="chunkCoordinate"/> without checking if colors already exist for that chunk
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk</param>
        /// <param name="newData">The colors to set the chunk's colors to</param>
        public virtual void SetDataChunk(int3 chunkCoordinate, NativeArray<T> newData)
        {
            if (TryGetDataChunk(chunkCoordinate, out NativeArray<T> oldData))
            {
                oldData.CopyFrom(newData);
                newData.Dispose();
            }
            else
            {
                _data.Add(chunkCoordinate, newData);
            }

            if (VoxelWorld.ChunkStore.TryGetDataChunk(chunkCoordinate, out ChunkProperties chunkProperties))
            {
                chunkProperties.HasChanges = true;
            }
        }

        /// <summary>
        /// Loops through each voxel data point that is contained in <paramref name="dataChunk"/> AND in <paramref name="worldSpaceQuery"/>, and performs <paramref name="function"/> on it
        /// </summary>
        /// <param name="worldSpaceQuery">The query that determines whether or not a voxel data point is contained.</param>
        /// <param name="chunkCoordinate">The coordinate of <paramref name="dataChunk"/></param>
        /// <param name="dataChunk">The voxel datas of the chunk</param>
        /// <param name="function">The function that will be performed on each voxel data point. The arguments are as follows: 1) The world space position of the voxel data point, 2) The chunk space position of the voxel data point, 3) The index of the voxel data point inside of <paramref name="dataChunk"/>, 4) The value of the voxel data</param>
        public void ForEachVoxelDataInQueryInChunk(BoundsInt worldSpaceQuery, int3 chunkCoordinate, NativeArray<T> dataChunk, Action<int3, int3, int, T> function)
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
                        if (dataChunk.TryGetElement(voxelDataIndex, out T voxelData))
                        {
                            function(voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Tries to get the voxel data from <paramref name="worldPosition"/>. If the position is not loaded, false will be returned and <paramref name="voxelData"/> will be set to 0 (Note that 0 doesn't directly mean that the position is not loaded). If it is loaded, true will be returned and <paramref name="voxelData"/> will be set to the value.
        /// </summary>
        /// <param name="worldPosition">The world position to get the voxel data from</param>
        /// <param name="voxelData">The voxel data value at the world position</param>
        /// <returns>Does a voxel data point exist at that position</returns>
        public bool TryGetVoxelData(int3 worldPosition, out T voxelData)
        {
            int3 chunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldPosition, VoxelWorld.WorldSettings.ChunkSize);
            if (TryGetDataChunk(chunkCoordinate, out NativeArray<T> chunk))
            {
                int3 voxelDataLocalPosition = worldPosition.Mod(VoxelWorld.WorldSettings.ChunkSize);
                return chunk.TryGetElement(VoxelWorld.WorldSettings.ChunkSize + 1, voxelDataLocalPosition, out voxelData);
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
        public NativeArray<T> GetVoxelDataCustom(BoundsInt worldSpaceQuery, Allocator allocator)
        {
            NativeArray<T> voxelDataArray = new NativeArray<T>(worldSpaceQuery.CalculateVolume(), allocator);

            ForEachVoxelDataArrayInQuery(worldSpaceQuery, (chunkCoordinate, voxelDataChunk) =>
            {
                ForEachVoxelDataInQueryInChunk(worldSpaceQuery, chunkCoordinate, voxelDataChunk, (voxelDataWorldPosition, voxelDataLocalPosition, voxelDataIndex, voxelData) =>
                {
                    voxelDataArray.SetElement(voxelData, worldSpaceQuery.size.ToInt3(), voxelDataWorldPosition - worldSpaceQuery.min.ToInt3());
                });
            });

            return voxelDataArray;
        }

        /// <summary>
        /// Sets the voxel data for a volume in the world
        /// </summary>
        /// <param name="voxelDataArray">The new voxel data array</param>
        /// <param name="originPosition">The world position of the origin where the voxel data should be set</param>
        public void SetVoxelDataCustom(NativeArray<T> voxelDataArray, int3 voxelDataArrayDimensions, int3 originPosition)
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
    }
}