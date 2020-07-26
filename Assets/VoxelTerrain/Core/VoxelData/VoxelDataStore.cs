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

        private void Awake()
        {
            _chunks = new Dictionary<int3, VoxelDataVolume>();
            _generationJobHandles = new Dictionary<int3, JobHandleWithData<IVoxelDataGenerationJob>>();
        }

        private void OnApplicationQuit()
        {
            if(_chunks == null) { return; }

            foreach (VoxelDataVolume chunk in _chunks.Values)
            {
                if (chunk.IsCreated)
                {
                    chunk.Dispose();
                }
            }
        }

        /// <summary>
        /// Tries to get the voxel data from <paramref name="worldPosition"/>. If the position is not loaded, false will be returned and <paramref name="voxelData"/> will be set to 0 (Note that 0 doesn't directly mean that the position is not loaded). If it is loaded, true will be returned and <paramref name="voxelData"/> will be set to the value.
        /// </summary>
        /// <param name="worldPosition">The world position to get the voxel data from</param>
        /// <param name="voxelData">The voxel data value at the world position</param>
        /// <returns>Does a voxel data point exist at that position</returns>
        public bool TryGetVoxelData(int3 worldPosition, out float voxelData)
        {
            int3 chunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldPosition, VoxelWorld.WorldSettings.ChunkSize);
            ApplyChunkChanges(chunkCoordinate);
            if (_chunks.TryGetValue(chunkCoordinate, out VoxelDataVolume chunk))
            {
                int3 voxelDataLocalPosition = worldPosition.Mod(VoxelWorld.WorldSettings.ChunkSize);
                return chunk.TryGetVoxelData(voxelDataLocalPosition.x, voxelDataLocalPosition.y, voxelDataLocalPosition.z, out voxelData);
            }
            else
            {
                voxelData = 0;
                return false;
            }
        }

        /// <summary>
        /// Tries to get the <see cref="VoxelDataVolume"/> for one chunk with a persistent allocator. If a chunk doesn't exist there, false will be returned and <paramref name="chunk"/> will be set to null. If a chunk exists there, true will be returned and <paramref name="chunk"/> will be set to the chunk.
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose voxel data should be gotten</param>
        /// <param name="chunk">The voxel data of a chunk at the coordinate</param>
        /// <returns>Does a chunk exists at that coordinate</returns>
        public bool TryGetVoxelDataChunk(int3 chunkCoordinate, out VoxelDataVolume chunk)
        {
            ApplyChunkChanges(chunkCoordinate);
            return _chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Gets the voxel data of a custom volume in the world with a persistent allocator
        /// </summary>
        /// <param name="bounds">The world-space volume to get the voxel data for</param>
        /// <returns>The voxel data volume inside the bounds</returns>
        public VoxelDataVolume GetVoxelDataCustom(Bounds bounds)
        {
            return GetVoxelDataCustom(bounds, Allocator.Persistent);
        }

        /// <summary>
        /// Gets the voxel data of a custom volume in the world
        /// </summary>
        /// <param name="bounds">The world-space volume to get the voxel data for</param>
        /// <param name="allocator">How the new voxel data volume should be allocated</param>
        /// <returns>The voxel data volume inside the bounds</returns>
        public VoxelDataVolume GetVoxelDataCustom(Bounds bounds, Allocator allocator)
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
                        if (!TryGetVoxelDataChunk(chunkCoordinate, out VoxelDataVolume voxelDataChunk))
                        {
                            continue;
                        }

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
                                for (int voxelDataWorldPositionZ = intersectionVolumeMin.z; voxelDataWorldPositionZ < intersectionVolumeMax.z; voxelDataWorldPositionZ++)
                                {
                                    int3 voxelDataWorldPosition = new int3(voxelDataWorldPositionX, voxelDataWorldPositionY, voxelDataWorldPositionZ);
                                    int3 voxelDataLocalPosition = voxelDataWorldPosition - chunkWorldSpaceOrigin;

                                    if (voxelDataChunk.TryGetVoxelData(voxelDataLocalPosition, out float voxelData))
                                    {
                                        voxelDataVolume.SetVoxelData(voxelData, voxelDataWorldPosition - bounds.min.ToInt3());
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return voxelDataVolume;
        }

        /// <summary>
        /// Increases the voxel data at <paramref name="worldPosition"/> by <paramref name="increaseAmount"/>.
        /// </summary>
        /// <param name="worldPosition">The world position of the voxel data that should be increased</param>
        public void IncreaseVoxelData(int3 worldPosition, float increaseAmount)
        {
            IEnumerable<int3> affectedChunkCoordinates = ChunkProvider.GetChunkCoordinatesContainingPoint(worldPosition, VoxelWorld.WorldSettings.ChunkSize);

            foreach(int3 chunkCoordinate in affectedChunkCoordinates)
            {
                if (!_chunks.ContainsKey(chunkCoordinate)) { continue; }

                if (TryGetVoxelDataChunk(chunkCoordinate, out VoxelDataVolume voxelDataVolume))
                {
                    int3 localPos = (worldPosition - chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize).Mod(VoxelWorld.WorldSettings.ChunkSize + 1);
                    voxelDataVolume.IncreaseVoxelData(increaseAmount, localPos);

                    if (VoxelWorld.ChunkStore.TryGetChunkAtCoordinate(chunkCoordinate, out Chunk chunk))
                    {
                        chunk.HasChanges = true;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the voxel data for a world position
        /// </summary>
        /// <param name="voxelData">The new voxel data</param>
        /// <param name="worldPosition">The world position of the voxel data</param>
        public void SetVoxelData(float voxelData, int3 worldPosition)
        {
            IEnumerable<int3> affectedChunkCoordinates = ChunkProvider.GetChunkCoordinatesContainingPoint(worldPosition, VoxelWorld.WorldSettings.ChunkSize);

            foreach (int3 chunkCoordinate in affectedChunkCoordinates)
            {
                if (!_chunks.ContainsKey(chunkCoordinate)) { continue; }

                if (TryGetVoxelDataChunk(chunkCoordinate, out VoxelDataVolume voxelDataVolume))
                {
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
        /// <param name="voxelDataVolume">The new voxel data volume</param>
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
                        if (!TryGetVoxelDataChunk(chunkCoordinate, out VoxelDataVolume voxelDataChunk))
                        {
                            continue;
                        }

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

                                    if (voxelDataChunk.TryGetVoxelData(voxelDataWorldPosition - worldSpaceQuery.min.ToInt3(), out float voxelData))
                                    {
                                        voxelDataChunk.SetVoxelData(voxelData, voxelDataWorldPosition - chunkWorldSpaceOrigin);
                                    }
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
        public void UnloadCoordinates(IEnumerable<int3> coordinatesToUnload)
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
