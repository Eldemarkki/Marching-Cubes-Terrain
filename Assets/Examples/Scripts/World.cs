using System.Collections.Generic;
using System.Linq;
using MarchingCubes.Examples.DensityFunctions;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public class World : MonoBehaviour
    {
        [Header("Chunk settings")]
        [SerializeField] private int chunkSize = 8;
        [SerializeField] private GameObject chunkPrefab;

        [Header("Marching Cubes settings")]
        [SerializeField] private float isolevel = 0.5F;
        [SerializeField] private DensityFunction densityFunction;

        [Header("Player settings")]
        [SerializeField] private int renderDistance = 40;
        [SerializeField] private Transform player;

        [Header("Other settings")]
        [SerializeField] private bool useThreading;

        private Dictionary<Vector3Int, Chunk> _chunks;
        private Vector3 _startPos;
        private Queue<Chunk> _availableChunks;
        private int _renderDistanceChunkCount;

        public DensityFunction DensityFunction { get; private set; }
        public bool UseThreading { get => useThreading; private set => useThreading = value; }

        private void Awake()
        {
            _renderDistanceChunkCount = Mathf.CeilToInt(renderDistance / (float)chunkSize);

            DensityFunction = densityFunction;
            _availableChunks = new Queue<Chunk>();
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
            
            // TODO: Initialize this only once
            var newTerrain = new Dictionary<Vector3Int, Chunk>((int)Mathf.Pow(_renderDistanceChunkCount * 2 + 1, 3));

            for (int x = -_renderDistanceChunkCount; x <= _renderDistanceChunkCount; x++)
            {
                for (int y = -_renderDistanceChunkCount; y < _renderDistanceChunkCount; y++)
                {
                    for (int z = -_renderDistanceChunkCount; z < _renderDistanceChunkCount; z++)
                    {
                        var chunkPosition = playerChunkPosition + new Vector3Int(x * chunkSize, y * chunkSize, z * chunkSize);
                        if (!_chunks.TryGetValue(chunkPosition, out Chunk chunk))
                        {
                            if (_availableChunks.Count > 0)
                            {
                                chunk = _availableChunks.Dequeue();
                                chunk.gameObject.SetActive(true);
                                chunk.transform.position = chunkPosition;
                                chunk.SetPosition(chunkPosition);
                            }
                            else
                            {
                                chunk = CreateChunk(chunkPosition);
                            }
                        }

                        newTerrain.Add(chunkPosition, chunk);
                    }
                }
            }

            var oldKeys = _chunks.Keys.ToList();
            foreach (var key in oldKeys)
            {
                if (newTerrain.ContainsKey(key)) { continue; }
                
                Chunk chunk = _chunks[key];
                if (_availableChunks.Contains(chunk)) { continue; }
                
                _availableChunks.Enqueue(chunk);
                chunk.gameObject.SetActive(false);
            }

            _chunks = newTerrain;

            _startPos = playerPos;
        }

        private Chunk GetChunk(Vector3Int pos)
        {
            return GetChunk(pos.x, pos.y, pos.z);
        }

        private Chunk GetChunk(int x, int y, int z)
        {
            int newX = Utils.FloorToNearestX(x, chunkSize);
            int newY = Utils.FloorToNearestX(y, chunkSize);
            int newZ = Utils.FloorToNearestX(z, chunkSize);

            Vector3Int key = new Vector3Int(newX, newY, newZ);
            return _chunks.TryGetValue(key, out Chunk chunk) ? chunk : null;
        }

        public float GetDensity(int x, int y, int z)
        {
            Chunk chunk = GetChunk(x, y, z);
            if (chunk == null)
            {
                return 0;
            }

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
                Chunk chunk = GetChunk(chunkPos);
                Vector3Int localPos = (pos - chunkPos).Mod(chunkSize + 1);

                chunk.SetDensity(density, localPos);
            }
        }

        private Chunk CreateChunk(Vector3Int chunkPosition)
        {
            var chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity).GetComponent<Chunk>();
            chunk.Initialize(this, chunkSize, isolevel, chunkPosition);
            _chunks.Add(chunkPosition, chunk);
            chunk.Generate();

            return chunk;
        }
    }
}