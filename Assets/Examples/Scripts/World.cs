using System.Collections.Generic;
using MarchingCubes.Examples.DensityFunctions;
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
        [SerializeField] private DensityFunction densityFunction;

        [Header("Terrain Settings")]
        [SerializeField] private TerrainSettings terrainSettings = new TerrainSettings(0.001f, 16, 10, 5);

        [Header("Player settings")]
        [SerializeField] private int renderDistance = 4;
        [SerializeField] private Transform player;

        [Header("Other settings")]
        [SerializeField] private bool useJobSystem;

        private Dictionary<Vector3Int, Chunk> _chunks;
        private Vector3 _startPos;

        public World(DensityFunction densityFunction) 
        {
            this.DensityFunction = densityFunction;
               
        }
        public DensityFunction DensityFunction { get => densityFunction; private set => densityFunction = value; }
        public TerrainSettings TerrainSettings { get => terrainSettings; private set => terrainSettings = value; }
        public bool UseJobSystem { get => useJobSystem; private set => useJobSystem = value; }

        private void Awake()
        {
            DensityFunction = densityFunction;
            _chunks = new Dictionary<Vector3Int, Chunk>();
            if(densityFunction is InitializedDensityFunction initializable)
            {
                initializable.Initialize();
            }
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
            Vector3Int playerChunkPosition = playerPos.FloorToNearestX(chunkSize);
            Vector3Int playerCoordinate = ((Vector3)playerChunkPosition / chunkSize).Floor();

            // TODO: Initialize this only once
            var newTerrainChunks = new Dictionary<Vector3Int, Chunk>((renderDistance * 2 + 1) * (renderDistance * 2 + 1) * (renderDistance * 2 + 1));

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
                        Vector3Int chunkCoordinate = playerCoordinate + new Vector3Int(x, y, z);

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
            int newX = Utils.FloorToNearestX(x, chunkSize) / chunkSize;
            int newY = Utils.FloorToNearestX(y, chunkSize) / chunkSize;
            int newZ = Utils.FloorToNearestX(z, chunkSize) / chunkSize;

            Vector3Int key = new Vector3Int(newX, newY, newZ);
            return _chunks[key];  
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

        public void SetDensity(float density, Vector3Int pos)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3Int chunkPos = (pos - LookupTables.CubeCorners[i]).FloorToNearestX(chunkSize);
                Chunk chunk = GetChunk(chunkPos.x, chunkPos.y, chunkPos.z);
                Vector3Int localPos = (pos - chunkPos).Mod(chunkSize + 1);

                chunk.SetDensity(density, localPos.x, localPos.y, localPos.z);
            }
        }

        private Chunk CreateChunk(Vector3Int chunkCoordinate)
        {
            var chunk = Instantiate(chunkPrefab, chunkCoordinate * chunkSize, Quaternion.identity).GetComponent<Chunk>();
            chunk.Initialize(this, chunkSize, isolevel, chunkCoordinate);
            _chunks.Add(chunkCoordinate, chunk);

            return chunk;
        }
    }
}