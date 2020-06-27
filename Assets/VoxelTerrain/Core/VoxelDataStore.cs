using System;
using System.Collections.Generic;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.Utilities.Intersection;
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
    public class VoxelDataStore : MonoBehaviour
    {
        /// <summary>
        /// A dictionary containing the chunks. Key is the chunk's coordinate, and the value is the chunk's voxel data volume
        /// </summary>
        private Dictionary<int3, VoxelDataVolume> _chunks;

        /// <summary>
        /// A dictionary of all the ongoing voxel data generation jobs. Key is the chunk's coordinate, and the value is the ongoing job for that chunk
        /// </summary>
        private Dictionary<int3, JobHandleWithData<IVoxelDataGenerationJob>> _generationJobHandles;

        /// <summary>
        /// The world that "owns" this voxel data store
        /// </summary>
        public VoxelWorld VoxelWorld { get; set; }

        void Awake()
        {
            _chunks = new Dictionary<int3, VoxelDataVolume>();
            _generationJobHandles = new Dictionary<int3, JobHandleWithData<IVoxelDataGenerationJob>>();
        }

        void OnApplicationQuit()
        {
            foreach (var chunk in _chunks)
            {
                if (chunk.Value.IsCreated)
                {
                    chunk.Value.Dispose();
                }
            }
        }

        /// <summary>
        /// Gets a voxel data from a world position. If the position is not loaded, 0 will be returned (Note that 0 doesn't directly mean that the position is not loaded)
        /// </summary>
        /// <param name="worldPosition">The world position to get the voxel data from</param>
        /// <returns>The voxel data at the world position</returns>
        public float GetVoxelData(int3 worldPosition)
        {
            int3 chunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldPosition, VoxelWorld.WorldSettings.ChunkSize);
            ApplyChunkChanges(chunkCoordinate);
            if (_chunks.TryGetValue(chunkCoordinate, out VoxelDataVolume chunk))
            {
                int3 voxelDataLocalPosition = worldPosition.Mod(VoxelWorld.WorldSettings.ChunkSize);
                float voxelData = chunk.GetVoxelData(voxelDataLocalPosition.x, voxelDataLocalPosition.y, voxelDataLocalPosition.z);
                return voxelData;
            }

            Debug.LogWarning($"The chunk which contains the world position {worldPosition.ToString()} is not loaded.");
            return 0;
        }

        /// <summary>
        /// Gets the voxel data volume for one chunk with a persistent allocator
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose voxel data should be gotten</param>
        /// <returns>The voxel data for the chunk</returns>
        public VoxelDataVolume GetVoxelDataChunk(int3 chunkCoordinate)
        {
            ApplyChunkChanges(chunkCoordinate);
            if (_chunks.TryGetValue(chunkCoordinate, out VoxelDataVolume chunk))
            {
                return chunk;
            }

            throw new Exception($"The chunk at coordinate {chunkCoordinate.ToString()} is not loaded.");
        }

        /// <summary>
        /// Gets the voxel data of a custom volume in the world
        /// </summary>
        /// <param name="bounds">The world-space volume to get the voxel data for</param>
        /// <param name="allocator">How the new voxel data volume should be allocated</param>
        /// <returns>The voxel data volume inside the bounds</returns>
        public VoxelDataVolume GetVoxelDataCustom(Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            VoxelDataVolume voxelDataVolume = new VoxelDataVolume(bounds.size.ToInt3(), allocator);

            Bounds worldSpaceQuery = bounds;

            int chunkSize = VoxelWorld.WorldSettings.ChunkSize;

            int3 minChunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldSpaceQuery.min - Vector3Int.one, chunkSize);
            int3 maxChunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldSpaceQuery.max + Vector3Int.one, chunkSize);

            for (int chunkCoordinateX = minChunkCoordinate.x; chunkCoordinateX <= maxChunkCoordinate.x; chunkCoordinateX++)
            {
                for (int chunkCoordinateY = minChunkCoordinate.y; chunkCoordinateY <= maxChunkCoordinate.y; chunkCoordinateY++)
                {
                    for (int chunkCoordinateZ = minChunkCoordinate.z; chunkCoordinateZ <= maxChunkCoordinate.z; chunkCoordinateZ++)
                    {
                        int3 chunkCoordinate = new int3(chunkCoordinateX, chunkCoordinateY, chunkCoordinateZ);
                        VoxelDataVolume voxelDataChunk = GetVoxelDataChunk(chunkCoordinate);

                        Vector3 chunkBoundsSize = new Vector3(voxelDataChunk.Width - 1, voxelDataChunk.Height - 1, voxelDataChunk.Depth - 1);
                        int3 chunkWorldSpaceOrigin = chunkCoordinate * chunkSize;

                        Bounds chunkWorldSpaceBounds = new Bounds();
                        chunkWorldSpaceBounds.SetMinMax(chunkWorldSpaceOrigin.ToVectorInt(), chunkWorldSpaceOrigin.ToVectorInt() + chunkBoundsSize);

                        Bounds intersectionVolume = IntersectionUtilities.GetIntersectionVolume(worldSpaceQuery, chunkWorldSpaceBounds);
                        int3 intersectionVolumeMin = intersectionVolume.min.ToInt3();
                        int3 intersectionVolumeMax = intersectionVolume.max.ToInt3();

                        for (int voxelDataWorldPositionX = intersectionVolumeMin.x; voxelDataWorldPositionX < intersectionVolumeMax.x; voxelDataWorldPositionX++)
                        {
                            for (int voxelDataWorldPositionY = intersectionVolumeMin.y; voxelDataWorldPositionY < intersectionVolumeMax.y; voxelDataWorldPositionY++)
                            {
                                for (int oxelDataWorldPositionZ = intersectionVolumeMin.z; oxelDataWorldPositionZ < intersectionVolumeMax.z; oxelDataWorldPositionZ++)
                                {
                                    int3 voxelDataWorldPosition = new int3(voxelDataWorldPositionX, voxelDataWorldPositionY, oxelDataWorldPositionZ);
                                    int3 voxelDataLocalPosition = voxelDataWorldPosition - chunkWorldSpaceOrigin;

                                    float voxelData = voxelDataChunk.GetVoxelData(voxelDataLocalPosition);
                                    voxelDataVolume.SetVoxelData(voxelData, voxelDataWorldPosition - bounds.min.ToInt3());
                                }
                            }
                        }
                    }
                }
            }

            return voxelDataVolume;
        }

        /// <summary>
        /// Sets the voxel data for a world position
        /// </summary>
        /// <param name="voxelData">The new voxel data</param>
        /// <param name="worldPosition">The voxel data's world position</param>
        public void SetVoxelData(float voxelData, int3 worldPosition)
        {
            List<int3> affectedChunkCoordinates = ChunkProvider.GetChunkCoordinatesContainingPoint(worldPosition, VoxelWorld.WorldSettings.ChunkSize);

            for (int i = 0; i < affectedChunkCoordinates.Count; i++)
            {
                int3 chunkCoordinate = affectedChunkCoordinates[i];

                if (_chunks.ContainsKey(chunkCoordinate))
                {
                    var voxelDataVolume = GetVoxelDataChunk(chunkCoordinate);
                    int3 localPos = (worldPosition - chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize).Mod(VoxelWorld.WorldSettings.ChunkSize + 1);
                    voxelDataVolume.SetVoxelData(voxelData, localPos.x, localPos.y, localPos.z);

                    if (VoxelWorld.ChunkStore.TryGetChunkAtCoordinate(chunkCoordinate, out Chunk chunk))
                    {
                        chunk.HasChanges = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sets a chunk's voxel data
        /// </summary>
        /// <param name="chunkVoxelData">The new voxel data</param>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose voxel data should be set</param>
        public void SetVoxelDataChunk(VoxelDataVolume chunkVoxelData, int3 chunkCoordinate)
        {
            if (_chunks.TryGetValue(chunkCoordinate, out VoxelDataVolume voxelDataVolume))
            {
                voxelDataVolume.CopyFrom(chunkVoxelData);
            }
            else
            {
                _chunks.Add(chunkCoordinate, chunkVoxelData);
            }

            if (VoxelWorld.ChunkStore.TryGetChunkAtCoordinate(chunkCoordinate, out Chunk chunk))
            {
                chunk.HasChanges = true;
            }
        }

        /// <summary>
        /// Sets the voxel data for a volume in the world
        /// </summary>
        /// <param name="voxelData">The new voxel data volume</param>
        /// <param name="originPosition">The world position of the origin where the voxel data should be set</param>
        public void SetVoxelDataCustom(VoxelDataVolume voxelDataVolume, int3 originPosition)
        {
            Bounds worldSpaceQuery = new Bounds();

            worldSpaceQuery.SetMinMax(originPosition.ToVectorInt(), (originPosition + voxelDataVolume.Size - new int3(1, 1, 1)).ToVectorInt());

            int chunkSize = VoxelWorld.WorldSettings.ChunkSize;

            int3 minChunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldSpaceQuery.min - Vector3Int.one, chunkSize);
            int3 maxChunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldSpaceQuery.max + Vector3Int.one, chunkSize);

            for (int chunkCoordinateX = minChunkCoordinate.x; chunkCoordinateX <= maxChunkCoordinate.x; chunkCoordinateX++)
            {
                for (int chunkCoordinateY = minChunkCoordinate.y; chunkCoordinateY <= maxChunkCoordinate.y; chunkCoordinateY++)
                {
                    for (int chunkCoordinateZ = minChunkCoordinate.z; chunkCoordinateZ <= maxChunkCoordinate.z; chunkCoordinateZ++)
                    {
                        int3 chunkCoordinate = new int3(chunkCoordinateX, chunkCoordinateY, chunkCoordinateZ);
                        VoxelDataVolume voxelDataChunk = GetVoxelDataChunk(chunkCoordinate);

                        Vector3 chunkBoundsSize = new Vector3(voxelDataChunk.Width - 1, voxelDataChunk.Height - 1, voxelDataChunk.Depth - 1);
                        int3 chunkWorldSpaceOrigin = chunkCoordinate * chunkSize;

                        Bounds chunkWorldSpaceBounds = new Bounds();
                        chunkWorldSpaceBounds.SetMinMax(chunkWorldSpaceOrigin.ToVectorInt(), chunkWorldSpaceOrigin.ToVectorInt() + chunkBoundsSize);

                        Bounds intersectionVolume = IntersectionUtilities.GetIntersectionVolume(worldSpaceQuery, chunkWorldSpaceBounds);
                        int3 intersectionVolumeMin = intersectionVolume.min.ToInt3();
                        int3 intersectionVolumeMax = intersectionVolume.max.ToInt3();

                        for (int voxelDataWorldPositionX = intersectionVolumeMin.x; voxelDataWorldPositionX <= intersectionVolumeMax.x; voxelDataWorldPositionX++)
                        {
                            for (int voxelDataWorldPositionY = intersectionVolumeMin.y; voxelDataWorldPositionY <= intersectionVolumeMax.y; voxelDataWorldPositionY++)
                            {
                                for (int voxelDataWorldPositionZ = intersectionVolumeMin.z; voxelDataWorldPositionZ <= intersectionVolumeMax.z; voxelDataWorldPositionZ++)
                                {
                                    int3 voxelDataWorldPosition = new int3(voxelDataWorldPositionX, voxelDataWorldPositionY, voxelDataWorldPositionZ);

                                    float voxelData = voxelDataChunk.GetVoxelData(voxelDataWorldPosition - worldSpaceQuery.min.ToInt3());
                                    voxelDataChunk.SetVoxelData(voxelData, voxelDataWorldPosition - chunkWorldSpaceOrigin);
                                }
                            }
                        }

                        if (VoxelWorld.ChunkStore.TryGetChunkAtCoordinate(chunkCoordinate, out Chunk chunk))
                        {
                            chunk.HasChanges = true;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sets the job handle for a chunk coordinate
        /// </summary>
        /// <param name="generationJobHandle">The job handle with data</param>
        /// <param name="chunkCoordinate">The coordinate of the chunk to set the job handle for</param>
        public void SetVoxelDataJobHandle(JobHandleWithData<IVoxelDataGenerationJob> generationJobHandle, int3 chunkCoordinate)
        {
            if (!_generationJobHandles.ContainsKey(chunkCoordinate))
            {
                _generationJobHandles.Add(chunkCoordinate, generationJobHandle);
            }
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
                SetVoxelDataChunk(jobHandle.JobData.OutputVoxelData, chunkCoordinate);
                _generationJobHandles.Remove(chunkCoordinate);
            }
        }

        /// <summary>
        /// Unloads the voxel data of chunks from the coordinates
        /// </summary>
        /// <param name="coordinatesToUnload">The list of chunk coordinates to unload</param>
        public void UnloadCoordinates(List<int3> coordinatesToUnload)
        {
            foreach (int3 coordinate in coordinatesToUnload)
            {
                if (_chunks.TryGetValue(coordinate, out VoxelDataVolume voxelDataVolume))
                {
                    voxelDataVolume.Dispose();
                    _chunks.Remove(coordinate);
                }
            }
        }
    }
}
