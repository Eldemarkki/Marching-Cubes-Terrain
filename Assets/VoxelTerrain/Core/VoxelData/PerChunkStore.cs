﻿using Eldemarkki.VoxelTerrain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    public abstract class PerChunkStore<T> : MonoBehaviour
    {
        /// <summary>
        /// The world that "owns" this store
        /// </summary>
        public VoxelWorld VoxelWorld { get; set; }

        /// <summary>
        /// Every chunk that is currently loaded
        /// </summary>
        public IEnumerable<T> Data => _data.Values;

        protected Dictionary<int3, T> _data;

        protected virtual void Awake()
        {
            _data = new Dictionary<int3, T>();
        }

        public virtual bool DoesChunkExistAtCoordinate(int3 chunkCoordinate)
        {
            return _data.ContainsKey(chunkCoordinate);
        }

        /// <summary>
        /// Tries to get the colors of a chunk
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose colors should be gotten</param>
        /// <param name="chunk">The colors of the chunk</param>
        /// <returns>True if a chunk exists at <paramref name="chunkCoordinate"/>, otherwise false.</returns>
        public virtual bool TryGetDataChunk(int3 chunkCoordinate, out T chunk)
        {
            return _data.TryGetValue(chunkCoordinate, out chunk);
        }

        /// <summary>
        /// Moves a chunk from coordinate <paramref name="from"/> to the coordinate <paramref name="to"/>
        /// </summary>
        /// <param name="from">The coordinate to move the chunk from</param>
        /// <param name="to">The new coordinate of the chunk</param>
        public virtual void MoveChunk(int3 from, int3 to)
        {
            // Check that 'from' and 'to' are not equal
            if (from.Equals(to)) { return; }

            // Check that a chunk does NOT already exist at 'to'
            if (DoesChunkExistAtCoordinate(to)) { return; }

            // Check that a chunk exists at 'from'
            if (TryGetDataChunk(from, out T existingData))
            {
                RemoveChunk(from);
                GenerateDataForChunk(to, existingData);
            }
        }

        /// <summary>
        /// Removes a chunk from a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk to remove</param>
        public void RemoveChunk(int3 chunkCoordinate)
        {
            _data.Remove(chunkCoordinate);
        }

        /// <summary>
        /// Generates the colors for a chunk at <paramref name="chunkCoordinate"/>; fills the color array with the default color
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the colors for</param>
        public abstract void GenerateDataForChunk(int3 chunkCoordinate);

        /// <summary>
        /// Generates the colors for a chunk at <paramref name="chunkCoordinate"/>; fills the color array with the default color
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the colors for</param>
        public abstract void GenerateDataForChunk(int3 chunkCoordinate, T existingData);

        /// <summary>
        /// Adds a chunk to the chunk store
        /// </summary>
        /// <param name="chunkProperties">The chunk to add</param>
        public void AddChunk(int3 chunkCoordinate, T data)
        {
            if (!_data.ContainsKey(chunkCoordinate))
            {
                _data.Add(chunkCoordinate, data);
            }
        }
        /// <summary>
        /// Gets a collection of chunk coordinates whose Manhattan Distance to the coordinate parameter is more than <paramref name="range"/>
        /// </summary>
        /// <param name="coordinate">Central coordinate</param>
        /// <param name="range">The radius of the chunks the player can see</param>
        /// <returns>A collection of chunk coordinates outside of the viewing range from the coordinate parameter</returns>
        public virtual IEnumerable<int3> GetChunkCoordinatesOutsideOfRange(int3 coordinate, int range)
        {
            foreach (int3 chunkCoordinate in _data.Keys.ToList())
            {
                if(DistanceUtilities.ChebyshevDistanceGreaterThan(coordinate, chunkCoordinate, range))
                {
                    yield return chunkCoordinate;
                }
            }
        }

        /// <summary>
        /// Loops through each voxel data array that intersects with <paramref name="worldSpaceQuery"/> and performs <paramref name="function"/> on them.
        /// </summary>
        /// <param name="worldSpaceQuery">The query which will be used to determine all the chunks that should be looped through</param>
        /// <param name="function">The function that will be performed on every chunk. The arguments are as follows: 1) The chunk's coordinate, 2) The chunk's voxel data</param>
        public void ForEachVoxelDataArrayInQuery(BoundsInt worldSpaceQuery, Action<int3, T> function)
        {
            int3 chunkSize = VoxelWorld.WorldSettings.ChunkSize;

            int3 minChunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldSpaceQuery.min - Vector3Int.one, chunkSize);
            int3 maxChunkCoordinate = VectorUtilities.WorldPositionToCoordinate(worldSpaceQuery.max, chunkSize);

            for (int chunkCoordinateX = minChunkCoordinate.x; chunkCoordinateX <= maxChunkCoordinate.x; chunkCoordinateX++)
            {
                for (int chunkCoordinateY = minChunkCoordinate.y; chunkCoordinateY <= maxChunkCoordinate.y; chunkCoordinateY++)
                {
                    for (int chunkCoordinateZ = minChunkCoordinate.z; chunkCoordinateZ <= maxChunkCoordinate.z; chunkCoordinateZ++)
                    {
                        int3 chunkCoordinate = new int3(chunkCoordinateX, chunkCoordinateY, chunkCoordinateZ);
                        if (!TryGetDataChunk(chunkCoordinate, out T voxelDataChunk))
                        {
                            continue;
                        }

                        function(chunkCoordinate, voxelDataChunk);
                    }
                }
            }
        }
    }
}