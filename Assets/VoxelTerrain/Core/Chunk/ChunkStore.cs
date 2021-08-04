﻿using System;
using Unity.Jobs;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    /// <summary>
    /// A container for all of the chunks in the world
    /// </summary>
    public class ChunkStore : PerChunkStore<ChunkProperties>, IDisposable
    {
        public void Dispose()
        {
            foreach (var chunk in Chunks)
            {
                chunk.Dispose();
            }
        }

        /// <summary>
        /// Generates the data for a chunk at coordinate <paramref name="chunkCoordinate"/>, and adds it to the chunks dictionary in <see cref="PerChunkStore{T}"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate to generate the data for</param>
        public override JobHandle GenerateDataForChunkUnchecked(int3 chunkCoordinate)
        {
            ChunkProperties chunkProperties = new ChunkProperties(VoxelWorld.WorldSettings.ChunkSize);
            chunkProperties.Move(chunkCoordinate);
            AddChunk(chunkCoordinate, chunkProperties);

            return default;
        }

        /// <summary>
        /// Generates the data for a chunk at coordinate <paramref name="chunkCoordinate"/>, and adds it to the chunks dictionary in <see cref="PerChunkStore{T}"/>. The new data is generated to <paramref name="existingData"/>.
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate to generate the data for</param>
        /// <param name="existingData">The already existing data that will be used to generate the data into</param>
        public override JobHandle GenerateDataForChunkUnchecked(int3 chunkCoordinate, ChunkProperties existingData, JobHandle dependency = default)
        {
            existingData.MeshCollider.enabled = false;
            existingData.MeshRenderer.enabled = false;
            existingData.Move(chunkCoordinate);
            AddChunk(chunkCoordinate, existingData);

            return dependency;
        }
    }
}