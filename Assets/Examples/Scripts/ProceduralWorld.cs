using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public class ProceduralWorld : World
    {
        [SerializeField] private ProceduralTerrainSettings proceduralTerrainSettings = new ProceduralTerrainSettings(1, 16, 50, -40);
        [SerializeField] private int renderDistance = 3;
        [SerializeField] private Transform player;

        private Vector3 _startPos;

        public ProceduralTerrainSettings ProceduralTerrainSettings { get => proceduralTerrainSettings; set => proceduralTerrainSettings = value; }

        protected override void Start()
        {
            base.Start();
            GenerateNewTerrain(player.position);
        }

        private void Update()
        {
            if (Mathf.Abs(player.position.x - _startPos.x) >= ChunkSize ||
                Mathf.Abs(player.position.y - _startPos.y) >= ChunkSize ||
                Mathf.Abs(player.position.z - _startPos.z) >= ChunkSize)
            {
                GenerateNewTerrain(player.position);
            }
        }

        private void GenerateNewTerrain(Vector3 playerPos)
        {
            int3 playerChunkPosition = playerPos.ToMathematicsFloat().FloorToNearestX(ChunkSize);
            int3 playerCoordinate = ((float3)playerChunkPosition / ChunkSize).Floor();

            // TODO: Initialize this only once
            var newTerrainChunks = new Dictionary<int3, Chunk>((renderDistance * 2 + 1) * (renderDistance * 2 + 1) * (renderDistance * 2 + 1));

            Queue<ProceduralChunk> availableChunks = new Queue<ProceduralChunk>();
            foreach (var chunk in Chunks.Values)
            {
                int xOffset = Mathf.Abs(chunk.Coordinate.x - playerCoordinate.x);
                int yOffset = Mathf.Abs(chunk.Coordinate.y - playerCoordinate.y);
                int zOffset = Mathf.Abs(chunk.Coordinate.z - playerCoordinate.z);

                if (xOffset > renderDistance || yOffset > renderDistance || zOffset > renderDistance)
                {
                    // This conversion always succeeds because we are always adding only ProceduralChunks to Chunks.
                    availableChunks.Enqueue(chunk as ProceduralChunk);
                }
            }

            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    for (int z = -renderDistance; z <= renderDistance; z++)
                    {
                        int3 chunkCoordinate = playerCoordinate + new int3(x, y, z);

                        ProceduralChunk proceduralChunk;
                        if (Chunks.TryGetValue(chunkCoordinate, out Chunk chunk))
                        {
                            proceduralChunk = chunk as ProceduralChunk;
                        }
                        else
                        {
                            // There is not a chunk at that coordinate, so move (or create) one there
                            if (availableChunks.Count > 0)
                            {
                                proceduralChunk = availableChunks.Dequeue();
                                proceduralChunk.SetCoordinate(chunkCoordinate);
                            }
                            else
                            {
                                proceduralChunk = CreateChunk(chunkCoordinate);
                            }
                        }

                        newTerrainChunks.Add(chunkCoordinate, proceduralChunk);
                    }
                }
            }

            Chunks = newTerrainChunks;
            _startPos = playerPos;
        }

        private ProceduralChunk CreateChunk(int3 chunkCoordinate)
        {
            ProceduralChunk chunk = Instantiate(ChunkPrefab, (chunkCoordinate * ChunkSize).ToVectorInt(), Quaternion.identity).GetComponent<ProceduralChunk>();
            chunk.name = $"Chunk_{chunkCoordinate.x}_{chunkCoordinate.y}_{chunkCoordinate.z}";
            chunk.World = this;
            chunk.Initialize(ChunkSize, Isolevel, chunkCoordinate);
            Chunks.Add(chunkCoordinate, chunk);

            return chunk;
        }
    }
}