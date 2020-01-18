using System.Collections.Generic;
using System.Linq;
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
        private Queue<ProceduralChunk> _availableChunks;

        public ProceduralTerrainSettings ProceduralTerrainSettings => proceduralTerrainSettings;

        private void Awake()
        {
            _availableChunks = new Queue<ProceduralChunk>();
        }

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
            int3 playerCoordinate = WorldPositionToCoordinate(playerPos);

            _availableChunks.Clear();

            foreach (var chunk in Chunks.Values.ToList())
            {
                int xOffset = Mathf.Abs(chunk.Coordinate.x - playerCoordinate.x);
                int yOffset = Mathf.Abs(chunk.Coordinate.y - playerCoordinate.y);
                int zOffset = Mathf.Abs(chunk.Coordinate.z - playerCoordinate.z);

                if (xOffset > renderDistance || yOffset > renderDistance || zOffset > renderDistance)
                {
                    // This conversion always succeeds because we are always adding only ProceduralChunks to Chunks.
                    _availableChunks.Enqueue(chunk as ProceduralChunk);
                    Chunks.Remove(chunk.Coordinate);
                }
            }

            for (int x = -renderDistance; x <= renderDistance; x++)
            {
                for (int y = -renderDistance; y <= renderDistance; y++)
                {
                    for (int z = -renderDistance; z <= renderDistance; z++)
                    {
                        int3 chunkCoordinate = playerCoordinate + new int3(x, y, z);
                        if (Chunks.ContainsKey(chunkCoordinate)) continue;

                        // Make sure that there is a chunk at that coordinate
                        Chunk chunk = EnsureChunkAtCoordinate(chunkCoordinate);
                        Chunks.Add(chunkCoordinate, chunk);
                    }
                }
            }

            _startPos = playerPos;
        }

        private Chunk EnsureChunkAtCoordinate(int3 chunkCoordinate)
        {
            if (_availableChunks.Count > 0)
            {
                ProceduralChunk proceduralChunk = _availableChunks.Dequeue();
                proceduralChunk.SetCoordinate(chunkCoordinate);
                return proceduralChunk;
            }

            return CreateChunk(chunkCoordinate);
        }

        private ProceduralChunk CreateChunk(int3 chunkCoordinate)
        {
            ProceduralChunk chunk = Instantiate(ChunkPrefab, (chunkCoordinate * ChunkSize).ToVectorInt(), Quaternion.identity).GetComponent<ProceduralChunk>();
            chunk.World = this;
            chunk.Initialize(ChunkSize, Isolevel, chunkCoordinate);

            return chunk;
        }
    }
}