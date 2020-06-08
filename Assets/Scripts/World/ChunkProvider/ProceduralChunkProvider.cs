using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World.Chunks;
using System.Collections;
using System.Collections.Generic;
using Eldemarkki.VoxelTerrain.Density;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// Provider for procedurally generated chunks
    /// </summary>
    public class ProceduralChunkProvider : ChunkProvider
    {
        /// <summary>
        /// The procedural terrain generation settings
        /// </summary>
        [SerializeField] private ProceduralTerrainSettings proceduralTerrainSettings = new ProceduralTerrainSettings(1, 9, 120, 0);

        /// <summary>
        /// The maximum amount of chunks that can be generated in one frame
        /// </summary>
        [SerializeField] private int chunkGenerationRate = 10;

        /// <summary>
        /// A queue that contains the chunks' coordinates that are out of view and thus ready to be moved anywhere
        /// </summary>
        private Queue<int3> _availableChunkCoordinates;

        /// <summary>
        /// A list that contains all the coordinates where a chunk will eventually have to be generated
        /// </summary>
        private List<int3> _generationQueue;

        protected override void Awake()
        {
            base.Awake();
            _availableChunkCoordinates = new Queue<int3>();
            _generationQueue = new List<int3>();
        }

        private void Start()
        {
            StartCoroutine(GenerateChunks());
        }

        /// <summary>
        /// Calculates the densities for a chunk at a coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose densities will be calculated</param>
        /// <returns>A density volume containing the densities. The volume's size is (chunkSize+1)^3</returns>
        protected override DensityVolume CalculateChunkDensities(int3 chunkCoordinate)
        {
            DensityVolume chunk = new DensityVolume(ChunkGenerationParams.ChunkSize + 1);
            var job = new ProceduralTerrainDensityCalculationJob
            {
                chunkSize = ChunkGenerationParams.ChunkSize + 1,
                DensityVolume = chunk,
                offset = chunkCoordinate * ChunkGenerationParams.ChunkSize,
                proceduralTerrainSettings = proceduralTerrainSettings
            };

            job.Schedule((ChunkGenerationParams.ChunkSize + 1) * (ChunkGenerationParams.ChunkSize + 1) * (ChunkGenerationParams.ChunkSize + 1), 256).Complete();

            return job.DensityVolume;
        }

        /// <summary>
        /// Kind of like Unity's Update() function, generates chunks but not all at once
        /// </summary>
        /// <returns></returns>
        private IEnumerator GenerateChunks()
        {
            while (true)
            {
                // This loop represents Unity's update function

                int chunksGenerated = 0;
                while (_generationQueue.Count > 0 && chunksGenerated < chunkGenerationRate)
                {
                    int3 chunkCoordinate = _generationQueue[0];
                    _generationQueue.RemoveAt(0);

                    if (_availableChunkCoordinates.Count == 0)
                    {
                        CreateChunkAtCoordinate(chunkCoordinate);
                    }
                    else
                    {
                        var chunkDensities = CalculateChunkDensities(chunkCoordinate);                        
                        VoxelDensityStore.SetDensityChunk(chunkDensities, chunkCoordinate);

                        int3 availableChunkCoordinate = _availableChunkCoordinates.Dequeue();
                        MoveChunk(availableChunkCoordinate, chunkCoordinate);
                    }

                    chunksGenerated++;
                }

                yield return null;
            }
        }

        /// <summary>
        /// Ensures that a chunk exists at a coordinate, if there is not, a new chunk is created in the next frame
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        public void EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            if (_chunks.ContainsKey(chunkCoordinate)) { return; }
            if (_generationQueue.Contains(chunkCoordinate)) { return; }

            _generationQueue.Add(chunkCoordinate);
        }

        /// <summary>
        /// Unloads the Chunks whose coordinate is in the coordinatesToUnload parameter
        /// </summary>
        /// <param name="coordinatesToUnload">A list of the coordinates to unload</param>
        public void UnloadCoordinates(List<int3> coordinatesToUnload)
        {
            // Mark coordinates as available
            for (var i = 0; i < coordinatesToUnload.Count; i++)
            {
                int3 coordinateToUnload = coordinatesToUnload[i];
                if (!_availableChunkCoordinates.Contains(coordinateToUnload))
                {
                    _availableChunkCoordinates.Enqueue(coordinateToUnload);
                }

                if (_generationQueue.Contains(coordinateToUnload))
                {
                    _generationQueue.Remove(coordinateToUnload);
                }
            }

            VoxelDensityStore.UnloadCoordinates(coordinatesToUnload);
        }

        /// <summary>
        /// Moves a chunk to a different coordinate if a chunk doesn't already exist there
        /// </summary>
        /// <param name="fromCoordinate">The coordinate of the chunk to move</param>
        /// <param name="toCoordinate">The coordinate where the chunk should be moved</param>
        private void MoveChunk(int3 fromCoordinate, int3 toCoordinate)
        {
            if (!Chunks.TryGetValue(fromCoordinate, out Chunk chunk))
            {
                Debug.LogWarning($"No chunk at {fromCoordinate}, exiting the function");
                return;
            }

            if (Chunks.ContainsKey(toCoordinate))
            {
                Debug.LogWarning($"A chunk already exists at {toCoordinate}, exiting the function");
                return;
            }

            Chunks.Remove(fromCoordinate);
            Chunks.Add(toCoordinate, chunk);

            chunk.Coordinate = toCoordinate;
            chunk.transform.position = toCoordinate.ToVectorInt() * ChunkGenerationParams.ChunkSize;
            chunk.name = Chunk.GetName(toCoordinate);
            chunk.StartMeshGeneration();
        }
    }
}