using System;
using System.Collections.Generic;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.Utilities.Intersection;
using Eldemarkki.VoxelTerrain.World;
using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Density
{
    /// <summary>
    /// A store which handles getting and setting the voxel data for the world
    /// </summary>
    public class VoxelDataStore : MonoBehaviour
    {
        /// <summary>
        /// A dictionary containing the chunks. Key is the chunk's coordinate, and the value is the chunk's density volume
        /// </summary>
        private Dictionary<int3, DensityVolume> _chunks;

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
            _chunks = new Dictionary<int3, DensityVolume>();
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
        /// Gets a density from a world position. If the position is not loaded, 0 will be returned (Note that 0 doesn't directly mean that the position is not loaded)
        /// </summary>
        /// <param name="worldPosition">The world position to get the density from</param>
        /// <returns>The density value at the world position</returns>
        public float GetDensity(int3 worldPosition)
        {
            int3 chunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldPosition, VoxelWorld.WorldSettings.ChunkSize);
            ApplyChunkChanges(chunkCoordinate);
            if (_chunks.TryGetValue(chunkCoordinate, out DensityVolume chunk))
            {
                int3 densityLocalPosition = worldPosition.Mod(VoxelWorld.WorldSettings.ChunkSize);
                float density = chunk.GetDensity(densityLocalPosition.x, densityLocalPosition.y, densityLocalPosition.z);
                return density;
            }

            Debug.LogWarning($"The chunk which contains the world position {worldPosition.ToString()} is not loaded.");
            return 0;
        }

        /// <summary>
        /// Gets the density volume for one chunk with a persistent allocator
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose densities should be gotten</param>
        /// <returns>The densities for the chunk</returns>
        public DensityVolume GetDensityChunk(int3 chunkCoordinate)
        {
            ApplyChunkChanges(chunkCoordinate);
            if (_chunks.TryGetValue(chunkCoordinate, out DensityVolume chunk))
            {
                return chunk;
            }

            throw new Exception($"The chunk at coordinate {chunkCoordinate.ToString()} is not loaded.");
        }

        /// <summary>
        /// Gets the densities of a custom volume in the world
        /// </summary>
        /// <param name="bounds">The world-space volume to get the densities for</param>
        /// <param name="allocator">How the new density volume should be allocated</param>
        /// <returns>The density volume inside the bounds</returns>
        public DensityVolume GetDensityCustom(Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            DensityVolume densityVolume = new DensityVolume(bounds.size.ToInt3(), allocator);

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
                        DensityVolume chunkDensities = GetDensityChunk(chunkCoordinate);

                        Vector3 chunkBoundsSize = new Vector3(chunkDensities.Width - 1, chunkDensities.Height - 1, chunkDensities.Depth - 1);
                        int3 chunkWorldSpaceOrigin = chunkCoordinate * chunkSize;

                        Bounds chunkWorldSpaceBounds = new Bounds();
                        chunkWorldSpaceBounds.SetMinMax(chunkWorldSpaceOrigin.ToVectorInt(), chunkWorldSpaceOrigin.ToVectorInt() + chunkBoundsSize);

                        Bounds intersectionVolume = IntersectionUtilities.GetIntersectionVolume(worldSpaceQuery, chunkWorldSpaceBounds);
                        int3 intersectionVolumeMin = intersectionVolume.min.ToInt3();
                        int3 intersectionVolumeMax = intersectionVolume.max.ToInt3();

                        for (int densityWorldPositionX = intersectionVolumeMin.x; densityWorldPositionX < intersectionVolumeMax.x; densityWorldPositionX++)
                        {
                            for (int densityWorldPositionY = intersectionVolumeMin.y; densityWorldPositionY < intersectionVolumeMax.y; densityWorldPositionY++)
                            {
                                for (int densityWorldPositionZ = intersectionVolumeMin.z; densityWorldPositionZ < intersectionVolumeMax.z; densityWorldPositionZ++)
                                {
                                    int3 densityWorldPosition = new int3(densityWorldPositionX, densityWorldPositionY, densityWorldPositionZ);
                                    int3 densityLocalPosition = densityWorldPosition - chunkWorldSpaceOrigin;

                                    float density = chunkDensities.GetDensity(densityLocalPosition);
                                    densityVolume.SetDensity(density, densityWorldPosition - bounds.min.ToInt3());
                                }
                            }
                        }
                    }
                }
            }

            return densityVolume;
        }

        /// <summary>
        /// Sets the density for a world position
        /// </summary>
        /// <param name="density">The new density</param>
        /// <param name="worldPosition">The density's world position</param>
        public void SetDensity(float density, int3 worldPosition)
        {
            List<int3> affectedChunkCoordinates = ChunkProvider.GetChunkCoordinatesContainingPoint(worldPosition, VoxelWorld.WorldSettings.ChunkSize);

            for (int i = 0; i < affectedChunkCoordinates.Count; i++)
            {
                int3 chunkCoordinate = affectedChunkCoordinates[i];

                if (_chunks.ContainsKey(chunkCoordinate))
                {
                    var densityVolume = GetDensityChunk(chunkCoordinate);
                    int3 localPos = (worldPosition - chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize).Mod(VoxelWorld.WorldSettings.ChunkSize + 1);
                    densityVolume.SetDensity(density, localPos.x, localPos.y, localPos.z);

                    if (VoxelWorld.ChunkStore.TryGetChunkAtCoordinate(chunkCoordinate, out Chunk chunk))
                    {
                        chunk.HasChanges = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sets a chunk's densities
        /// </summary>
        /// <param name="chunkDensities">The new densities</param>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose densities should be set</param>
        public void SetDensityChunk(DensityVolume chunkDensities, int3 chunkCoordinate)
        {
            if (_chunks.TryGetValue(chunkCoordinate, out DensityVolume chunkDensityVolume))
            {
                chunkDensityVolume.CopyFrom(chunkDensities);
            }
            else
            {
                _chunks.Add(chunkCoordinate, chunkDensities);
            }

            if (VoxelWorld.ChunkStore.TryGetChunkAtCoordinate(chunkCoordinate, out Chunk chunk))
            {
                chunk.HasChanges = true;
            }
        }

        /// <summary>
        /// Sets the densities for a volume in the world
        /// </summary>
        /// <param name="densities">The new density volume</param>
        /// <param name="originPosition">The world position of the bottom-left-front corner where the densities should be set</param>
        public void SetDensityCustom(DensityVolume densities, int3 originPosition)
        {
            Bounds worldSpaceQuery = new Bounds();

            worldSpaceQuery.SetMinMax(originPosition.ToVectorInt(), (originPosition + densities.Size - new int3(1, 1, 1)).ToVectorInt());

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
                        DensityVolume chunkDensities = GetDensityChunk(chunkCoordinate);

                        Vector3 chunkBoundsSize = new Vector3(chunkDensities.Width - 1, chunkDensities.Height - 1, chunkDensities.Depth - 1);
                        int3 chunkWorldSpaceOrigin = chunkCoordinate * chunkSize;

                        Bounds chunkWorldSpaceBounds = new Bounds();
                        chunkWorldSpaceBounds.SetMinMax(chunkWorldSpaceOrigin.ToVectorInt(), chunkWorldSpaceOrigin.ToVectorInt() + chunkBoundsSize);

                        Bounds intersectionVolume = IntersectionUtilities.GetIntersectionVolume(worldSpaceQuery, chunkWorldSpaceBounds);
                        int3 intersectionVolumeMin = intersectionVolume.min.ToInt3();
                        int3 intersectionVolumeMax = intersectionVolume.max.ToInt3();

                        for (int densityWorldPositionX = intersectionVolumeMin.x; densityWorldPositionX <= intersectionVolumeMax.x; densityWorldPositionX++)
                        {
                            for (int densityWorldPositionY = intersectionVolumeMin.y; densityWorldPositionY <= intersectionVolumeMax.y; densityWorldPositionY++)
                            {
                                for (int densityWorldPositionZ = intersectionVolumeMin.z; densityWorldPositionZ <= intersectionVolumeMax.z; densityWorldPositionZ++)
                                {
                                    int3 densityWorldPosition = new int3(densityWorldPositionX, densityWorldPositionY, densityWorldPositionZ);

                                    float density = densities.GetDensity(densityWorldPosition - worldSpaceQuery.min.ToInt3());
                                    chunkDensities.SetDensity(density, densityWorldPosition - chunkWorldSpaceOrigin);
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
        public void SetDensityChunkJobHandle(JobHandleWithData<IVoxelDataGenerationJob> generationJobHandle, int3 chunkCoordinate)
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
                SetDensityChunk(jobHandle.JobData.OutputVoxelData, chunkCoordinate);
                _generationJobHandles.Remove(chunkCoordinate);
            }
        }

        /// <summary>
        /// Unloads the densities of chunks from the coordinates
        /// </summary>
        /// <param name="coordinatesToUnload">The list of chunk coordinates to unload</param>
        public void UnloadCoordinates(List<int3> coordinatesToUnload)
        {
            foreach (int3 coordinate in coordinatesToUnload)
            {
                if (_chunks.TryGetValue(coordinate, out DensityVolume densityVolume))
                {
                    densityVolume.Dispose();
                    _chunks.Remove(coordinate);
                }
            }
        }
    }
}
