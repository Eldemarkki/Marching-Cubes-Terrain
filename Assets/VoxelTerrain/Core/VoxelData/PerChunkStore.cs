using Eldemarkki.VoxelTerrain.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A dictionary that stores a single variable of type <typeparamref name="T"/> for a chunk
    /// </summary>
    /// <typeparam name="T">The type of data to store for each chunk</typeparam>
    public abstract class PerChunkStore<T> : MonoBehaviour
    {
        /// <summary>
        /// The world that "owns" this store
        /// </summary>
        public VoxelWorld VoxelWorld { get; set; }

        /// <summary>
        /// Every value in currently in the dictionary
        /// </summary>
        public IEnumerable<T> Chunks => _chunks.Values;

        /// <summary>
        /// The dictionary that contains the data, key is the chunk's coordinate and value is the data associated with the chunk
        /// </summary>
        protected Dictionary<int3, T> _chunks;

        protected virtual void Awake()
        {
            _chunks = new Dictionary<int3, T>();
        }

        /// <summary>
        /// Checks whether or not data exists for the chunk at coordinate <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The chunk coordinate to check for</param>
        /// <returns>Returns true if data exists for the chunk, otherwise returns false</returns>
        public virtual bool DoesChunkExistAtCoordinate(int3 chunkCoordinate)
        {
            return _chunks.ContainsKey(chunkCoordinate);
        }

        /// <summary>
        /// Tries to get the data of a chunk. 
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose data should be gotten</param>
        /// <param name="chunkData">The data associated with chunk</param>
        /// <returns>True if a chunk exists at <paramref name="chunkCoordinate"/>, otherwise false.</returns>
        public virtual bool TryGetDataChunk(int3 chunkCoordinate, out T chunkData)
        {
            return _chunks.TryGetValue(chunkCoordinate, out chunkData);
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
                RemoveChunkUnchecked(from);
                GenerateDataForChunkUnchecked(to, existingData);
            }
        }

        /// <summary>
        /// Removes a chunk from <paramref name="chunkCoordinate"/> without checking if a chunk exists there
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk to remove</param>
        public void RemoveChunkUnchecked(int3 chunkCoordinate)
        {
            _chunks.Remove(chunkCoordinate);
        }

        /// <summary>
        /// If data does not already exist at <paramref name="chunkCoordinate"/>, it generates the data for a chunk at coordinate <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate to generate the data for</param>
        public void GenerateDataForChunk(int3 chunkCoordinate)
        {
            if (!DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                GenerateDataForChunkUnchecked(chunkCoordinate);
            }
        }

        /// <summary>
        /// If data does not already exist at <paramref name="chunkCoordinate"/>, it generates the data for a chunk at <paramref name="chunkCoordinate"/> by reusing <paramref name="existingData"/> in order to save memory
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the data for</param>
        /// <param name="existingData">The already existing data that should be reused to generate the new data</param>
        public void GenerateDataForChunk(int3 chunkCoordinate, T existingData)
        {
            if (!DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                GenerateDataForChunkUnchecked(chunkCoordinate, existingData);
            }
        }

        /// <summary>
        /// Generates the data for a chunk at <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the data for</param>
        public abstract void GenerateDataForChunkUnchecked(int3 chunkCoordinate);

        /// <summary>
        /// Generates the data for a chunk at <paramref name="chunkCoordinate"/> by reusing <paramref name="existingData"/> in order to save memory
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the data for</param>
        /// <param name="existingData">The already existing data that should be reused to generate the new data</param>
        public abstract void GenerateDataForChunkUnchecked(int3 chunkCoordinate, T existingData);

        /// <summary>
        /// Adds a chunk to the chunk store, if one does not already exist
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk that the data will be associated with</param>
        /// <param name="data">The data that will be associated with the chunk</param>
        public void AddChunk(int3 chunkCoordinate, T data)
        {
            if (!DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                AddChunkUnchecked(chunkCoordinate, data);
            }
        }

        /// <summary>
        /// Adds a chunk to the chunk store, throws error if one already exists at <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk that the data will be associated with</param>
        /// <param name="data">The data that will be associated with the chunk</param>
        public void AddChunkUnchecked(int3 chunkCoordinate, T data)
        {
            _chunks.Add(chunkCoordinate, data);
        }

        /// <summary>
        /// Gets a collection of chunk coordinates whose Chebyshev distance to <paramref name="coordinate"/> is more than <paramref name="range"/>
        /// </summary>
        /// <param name="coordinate">Central coordinate</param>
        /// <param name="range">The maximum allowed Chebyshev distance</param>
        /// <returns>A collection of chunk coordinates outside of <paramref name="range"/> from <paramref name="coordinate"/></returns>
        public virtual IEnumerable<int3> GetChunkCoordinatesOutsideOfRange(int3 coordinate, int range)
        {
            foreach (int3 chunkCoordinate in _chunks.Keys.ToList())
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
        /// <param name="function">The function that will be performed on every chunk. The arguments are as follows: 1) The chunk's coordinate, 2) The data associated with the chunk</param>
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
                        if (TryGetDataChunk(chunkCoordinate, out T voxelDataChunk))
                        {
                            function(chunkCoordinate, voxelDataChunk);
                        }
                    }
                }
            }
        }
    }
}