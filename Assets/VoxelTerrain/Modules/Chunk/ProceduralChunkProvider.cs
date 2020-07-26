using Eldemarkki.VoxelTerrain.World.Chunks;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Chunks
{
    /// <summary>
    /// Provider for procedurally generated chunks
    /// </summary>
    public class ProceduralChunkProvider : ChunkProvider
    {
        /// <summary>
        /// The maximum amount of chunks that can be generated in one frame
        /// </summary>
        [SerializeField] private int chunkGenerationRate = 10;

        /// <summary>
        /// A list that contains all the coordinates where a chunk will eventually have to be generated
        /// </summary>
        private List<int3> _generationQueue;

        private void Awake()
        {
            _generationQueue = new List<int3>();
        }

        private void Update()
        {
            int chunksGenerated = 0;
            while (_generationQueue.Count > 0 && chunksGenerated < chunkGenerationRate)
            {
                int3 chunkCoordinate = _generationQueue[0];
                _generationQueue.RemoveAt(0);

                LoadChunkToCoordinate(chunkCoordinate);

                chunksGenerated++;
            }
        }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a new chunk is created later
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public override void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (VoxelWorld.ChunkStore.DoesChunkExistAtCoordinate(chunkCoordinate)) { return; }
            if (_generationQueue.Contains(chunkCoordinate)) { return; }

            _generationQueue.Add(chunkCoordinate);
        }

        /// <summary>
        /// Unloads every coordinate in the list
        /// </summary>
        /// <param name="coordinatesToUnload">A list of the coordinates to unload</param>
        public void UnloadCoordinates(IEnumerable<int3> coordinatesToUnload)
        {
            // Remove the coordinates from the generation queue
            foreach(int3 coordinateToUnload in coordinatesToUnload)
            {
                if (_generationQueue.Contains(coordinateToUnload))
                {
                    _generationQueue.Remove(coordinateToUnload);
                }
            }

            VoxelWorld.VoxelDataStore.UnloadCoordinates(coordinatesToUnload);

            foreach(int3 chunkCoordinate in coordinatesToUnload)
            {
                VoxelWorld.ChunkStore.DestroyChunk(chunkCoordinate);
            }
        }
    }
}