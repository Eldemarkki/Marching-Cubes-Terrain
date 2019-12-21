using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public class World : MonoBehaviour
    {
        [Header("Chunk settings")]
        [SerializeField] private int chunkSize = 16;
        [SerializeField] private GameObject chunkPrefab;

        [Header("Marching Cubes settings")]
        [SerializeField] private float isolevel = 0.5F;

        [Header("Terrain Settings")]
        [SerializeField] private TerrainSettings terrainSettings = new TerrainSettings(0.001f, 16, 10, 5);

        [Header("Player settings")]
        [SerializeField] private int renderDistance = 4;
        [SerializeField] private Transform player;

        private Dictionary<int3, Chunk> _chunks;
        private Vector3 _startPos;

        public TerrainSettings TerrainSettings { get => terrainSettings; private set => terrainSettings = value; }

        private void Awake()
        {
            _chunks = new Dictionary<int3, Chunk>();
        }

        private void Start()
        {
            GenerateNewTerrain(player.position);
        }

        private void Update()
        {
            if (Mathf.Abs(player.position.x - _startPos.x) >= chunkSize ||
                Mathf.Abs(player.position.y - _startPos.y) >= chunkSize ||
                Mathf.Abs(player.position.z - _startPos.z) >= chunkSize)
            {
                GenerateNewTerrain(player.position);
            }
        }

        private void GenerateNewTerrain(Vector3 playerPos)
        {
            int3 playerChunkPosition = playerPos.ToMathematicsFloat().FloorToNearestX(chunkSize);
            int3 playerCoordinate = ((float3)playerChunkPosition / chunkSize).Floor();

            // TODO: Initialize this only once
            var newTerrainChunks = new Dictionary<int3, Chunk>((renderDistance * 2 + 1) * (renderDistance * 2 + 1) * (renderDistance * 2 + 1));

            Queue<Chunk> availableChunks = new Queue<Chunk>();
            foreach (var chunk in _chunks.Values)
            {
                int xOffset = Mathf.Abs(chunk.Coordinate.x - playerCoordinate.x);
                int yOffset = Mathf.Abs(chunk.Coordinate.y - playerCoordinate.y);
                int zOffset = Mathf.Abs(chunk.Coordinate.z - playerCoordinate.z);

                if(xOffset > renderDistance || yOffset > renderDistance || zOffset > renderDistance)
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

                        Chunk chunk;
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

        private Chunk GetChunk(int x, int y, int z)
        {
            int newX = Utils.FloorToNearestX((float)x, chunkSize) / chunkSize;
            int newY = Utils.FloorToNearestX((float)y, chunkSize) / chunkSize;
            int newZ = Utils.FloorToNearestX((float)z, chunkSize) / chunkSize;

            int3 key = new int3(newX, newY, newZ);
            if (_chunks.TryGetValue(key, out Chunk chunk))
            {
                return chunk;
            }

            return null;
        }

        public float GetDensity(int x, int y, int z)
        {
            Chunk chunk = GetChunk(x, y, z);
            if (chunk == null) { return 0; }

            float density = chunk.GetDensity(x.Mod(chunkSize),
                                             y.Mod(chunkSize),
                                             z.Mod(chunkSize));
            return density;
        }

        public void SetDensity(float density, int3 pos)
        {
            for (int i = 0; i < 8; i++)
            {
                int3 chunkPos = (pos - LookupTables.CubeCorners[i]).FloorToNearestX(chunkSize);
                Chunk chunk = GetChunk(chunkPos.x, chunkPos.y, chunkPos.z);
                if (chunk != null)
                {
                    int3 localPos = (pos - chunkPos).Mod(chunkSize + 1);
                    chunk.SetDensity(density, localPos.x, localPos.y, localPos.z);
                }
            }
        }

        private Chunk CreateChunk(int3 chunkCoordinate)
        {
            var chunk = Instantiate(chunkPrefab, (chunkCoordinate * chunkSize).ToVectorInt(), Quaternion.identity).GetComponent<Chunk>();
            chunk.Initialize(this, chunkSize, isolevel, chunkCoordinate);
            _chunks.Add(chunkCoordinate, chunk);

            return chunk;
        }
    }
}