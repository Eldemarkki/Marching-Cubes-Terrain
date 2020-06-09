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
    /// A store which handles getting and setting densities for the world
    /// </summary>
    public class VoxelDensityStore : MonoBehaviour
    {
        /// <summary>
        /// The chunk provider that will be notified when a density is changed
        /// </summary>
        [SerializeField] private ChunkProvider chunkProvider;

        /// <summary>
        /// A dictionary containing the chunks. Key is the chunk's coordinate, and the value is the chunk's density volume
        /// </summary>
        private Dictionary<int3, DensityVolume> chunks;

        void Awake()
        {
            chunks = new Dictionary<int3, DensityVolume>();
        }

        void OnApplicationQuit()
        {
            foreach (var chunk in chunks)
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
            int3 chunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldPosition, chunkProvider.ChunkGenerationParams.ChunkSize);
            if (chunks.TryGetValue(chunkCoordinate, out DensityVolume chunk))
            {
                int3 densityLocalPosition = worldPosition.Mod(chunkProvider.ChunkGenerationParams.ChunkSize);
                float density = chunk.GetDensity(densityLocalPosition.x, densityLocalPosition.y, densityLocalPosition.z);
                return density;
            }

            // TODO: Load the chunk from disk and get the density from that.
            // TODO: If the chunk doesn't exist, call the density function to calculate
            Debug.LogWarning($"The chunk which contains the world position {worldPosition} is not loaded.");
            return 0;
        }

        /// <summary>
        /// Gets the density volume for one chunk with a persistent allocator
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose densities should be gotten</param>
        /// <returns>The densities for the chunk</returns>
        public DensityVolume GetDensityChunk(int3 chunkCoordinate)
        {
            if (chunks.TryGetValue(chunkCoordinate, out DensityVolume chunk))
            {
                return chunk;
            }

            // TODO: Load the chunk from disk and get the densities from it
            throw new Exception($"The chunk at coordinate {chunkCoordinate} is not loaded.");
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

            int chunkSize = chunkProvider.ChunkGenerationParams.ChunkSize;

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

                        Bounds intersectionVolume = IntersectionUtilities.GetIntersectionArea(worldSpaceQuery, chunkWorldSpaceBounds);
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
            List<Chunk> affectedChunks = chunkProvider.GetChunksContainingPoint(worldPosition);

            for (int i = 0; i < affectedChunks.Count; i++)
            {
                int3 chunkCoordinate = affectedChunks[i].Coordinate;
                var densityVolume = GetDensityChunk(chunkCoordinate);
                int3 localPos = (worldPosition - chunkCoordinate * chunkProvider.ChunkGenerationParams.ChunkSize).Mod(chunkProvider.ChunkGenerationParams.ChunkSize + 1);
                densityVolume.SetDensity(density, localPos.x, localPos.y, localPos.z);
                SetDensityChunk(densityVolume, chunkCoordinate);
            }

            chunkProvider.SetChunksHaveChanges(affectedChunks);
        }

        /// <summary>
        /// Sets a chunk's densities
        /// </summary>
        /// <param name="chunkDensities">The new densities</param>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose densities should be set</param>
        public void SetDensityChunk(DensityVolume chunkDensities, int3 chunkCoordinate)
        {
            if (chunks.TryGetValue(chunkCoordinate, out DensityVolume chunk))
            {
                chunk.CopyFrom(chunkDensities);
            }
            else
            {
                chunks.Add(chunkCoordinate, chunkDensities);
            }

            chunkProvider.SetChunkHasChanges(chunkCoordinate);
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

            int chunkSize = chunkProvider.ChunkGenerationParams.ChunkSize;

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

                        Bounds intersectionVolume = IntersectionUtilities.GetIntersectionArea(worldSpaceQuery, chunkWorldSpaceBounds);
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

                        chunkProvider.SetChunkHasChanges(chunkCoordinate);
                    }
                }
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
                if (chunks.TryGetValue(coordinate, out DensityVolume densityVolume))
                {
                    densityVolume.Dispose();
                    chunks.Remove(coordinate);
                }
            }
        }
    }
}