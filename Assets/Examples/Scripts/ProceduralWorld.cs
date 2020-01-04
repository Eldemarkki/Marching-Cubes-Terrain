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

        private Dictionary<int3, ProceduralChunk> _chunks;
        private Vector3 _startPos;

        public ProceduralTerrainSettings ProceduralTerrainSettings { get => proceduralTerrainSettings; set => proceduralTerrainSettings = value; }

        private void Awake()
        {
            _chunks = new Dictionary<int3, ProceduralChunk>();
        }

        private void Start()
        {
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
            var newTerrainChunks = new Dictionary<int3, ProceduralChunk>((renderDistance * 2 + 1) * (renderDistance * 2 + 1) * (renderDistance * 2 + 1));

            Queue<ProceduralChunk> availableChunks = new Queue<ProceduralChunk>();
            foreach (var chunk in _chunks.Values)
            {
                int xOffset = Mathf.Abs(chunk.Coordinate.x - playerCoordinate.x);
                int yOffset = Mathf.Abs(chunk.Coordinate.y - playerCoordinate.y);
                int zOffset = Mathf.Abs(chunk.Coordinate.z - playerCoordinate.z);

                if (xOffset > renderDistance || yOffset > renderDistance || zOffset > renderDistance)
                {
                    availableChunks.Enqueue(chunk);
                }
            }

            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    for (int z = -renderDistance; z <= renderDistance; z++)
                    {
                        int3 chunkCoordinate = playerCoordinate + new int3(x, y, z);

                        ProceduralChunk chunk;
                        bool chunkExistsAtCoordinate = _chunks.ContainsKey(chunkCoordinate);
                        if (chunkExistsAtCoordinate)
                        {
                            chunk = _chunks[chunkCoordinate];
                        }
                        else
                        {
                            // There is not a chunk at that coordinate, so move (or create) one there
                            if (availableChunks.Count > 0)
                            {
                                chunk = availableChunks.Dequeue();
                                chunk.SetCoordinate(chunkCoordinate);
                            }
                            else
                            {
                                chunk = CreateChunk(chunkCoordinate);
                            }
                        }

                        newTerrainChunks.Add(chunkCoordinate, chunk);
                    }
                }
            }

            _chunks = newTerrainChunks;
            _startPos = playerPos;
        }

        public override Chunk GetChunk(int3 worldPosition)
        {
            int newX = Utils.FloorToNearestX((float)worldPosition.x, ChunkSize) / ChunkSize;
            int newY = Utils.FloorToNearestX((float)worldPosition.y, ChunkSize) / ChunkSize;
            int newZ = Utils.FloorToNearestX((float)worldPosition.z, ChunkSize) / ChunkSize;

            int3 key = new int3(newX, newY, newZ);
            return _chunks.TryGetValue(key, out ProceduralChunk chunk) ? chunk : null;
        }

        private ProceduralChunk CreateChunk(int3 chunkCoordinate)
        {
            ProceduralChunk chunk = Instantiate(ChunkPrefab, (chunkCoordinate * ChunkSize).ToVectorInt(), Quaternion.identity).GetComponent<ProceduralChunk>();
            chunk.World = this;
            chunk.Initialize(ChunkSize, Isolevel, chunkCoordinate);
            _chunks.Add(chunkCoordinate, chunk);

            return chunk;
        }

        public override float GetDensity(int3 worldPosition)
        {
            Chunk chunk = GetChunk(worldPosition);
            if (chunk == null) { return 0; }

            return chunk.GetDensity(worldPosition.Mod(ChunkSize));
        }

        public override void SetDensity(float density, int3 pos)
        {
            for (int i = 0; i < 8; i++)
            {
                int3 chunkPos = (pos - LookupTables.CubeCorners[i]).FloorToNearestX(ChunkSize);
                Chunk chunk = GetChunk(chunkPos);
                if (chunk == null) { continue; }

                int3 localPos = (pos - chunkPos).Mod(ChunkSize + 1);
                chunk.SetDensity(density, localPos.x, localPos.y, localPos.z);
            }
        }
    }
}