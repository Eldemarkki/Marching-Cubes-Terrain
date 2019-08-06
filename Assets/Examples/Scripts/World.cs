using System.Collections.Generic;
using System.Linq;
using MarchingCubes.Examples.DensityFunctions;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public class World : MonoBehaviour
    {
        [SerializeField] private int chunkSize = 8;

        [SerializeField] private int worldWidth = 5;
        [SerializeField] private int worldHeight = 5;
        [SerializeField] private int worldDepth = 5;

        [SerializeField] public float isolevel = 0.5F;

        [SerializeField] private int renderDistance = 40;
        [SerializeField] private Transform player;

        [SerializeField] private GameObject chunkPrefab = null;

        [SerializeField] public DensityFunction densityFunction;

        private Dictionary<Vector3Int, Chunk> chunks;

        private Vector3 startPos;
        private Queue<Chunk> availableChunks;

        private void Awake()
        {
            startPos = player.position;
            availableChunks = new Queue<Chunk>();
            chunks = new Dictionary<Vector3Int, Chunk>(worldWidth*worldHeight*worldDepth);
            if(densityFunction is InitializedDensityFunction initializable){
                initializable.Initialize();
            }
        }

        private void Update()
        {
            if (Mathf.Abs(player.position.x - startPos.x) >= chunkSize ||
                Mathf.Abs(player.position.y - startPos.y) >= chunkSize ||
                Mathf.Abs(player.position.z - startPos.z) >= chunkSize)
            {
                GenerateNewTerrain(player.position);
            }
        }
        
        private void Start()
        {
            GenerateNewTerrain(player.position);
        }

        public void GenerateNewTerrain(Vector3 playerPos)
        {;
            Vector3Int playerChunkPosition = playerPos.FloorToNearestX(chunkSize);
            int renderDistanceChunkCount = Mathf.CeilToInt(renderDistance / (float)chunkSize);
            var newTerrain = new Dictionary<Vector3Int, Chunk>((int)Mathf.Pow(renderDistanceChunkCount * 2 + 1, 3));

            for (int x = -renderDistanceChunkCount; x <= renderDistanceChunkCount; x++)
            {
                for (int y = -renderDistanceChunkCount; y < renderDistanceChunkCount; y++)
                {
                    for (int z = -renderDistanceChunkCount; z < renderDistanceChunkCount; z++)
                    {
                        Vector3Int chunkPosition = playerChunkPosition + new Vector3Int(x * chunkSize, y * chunkSize, z * chunkSize);
                        if (!chunks.TryGetValue(chunkPosition, out Chunk chunk))
                        {
                            if (availableChunks.Count > 0)
                            {
                                chunk = availableChunks.Dequeue();
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

            var oldKeys = chunks.Keys.ToList();
            foreach (var key in oldKeys)
            {
                if (!newTerrain.ContainsKey(key))
                {
                    Chunk chunk = chunks[key];
                    if (!availableChunks.Contains(chunk))
                    {
                        availableChunks.Enqueue(chunk);
                        chunk.gameObject.SetActive(false);
                    }
                }
            }

            chunks = newTerrain;

            startPos = playerPos;
        }

        public Chunk GetChunk(Vector3Int pos)
        {
            return GetChunk(pos.x, pos.y, pos.z);
        }

        public Chunk GetChunk(int x, int y, int z)
        {
            int newX = Utils.FloorToNearestX(x, chunkSize);
            int newY = Utils.FloorToNearestX(y, chunkSize);
            int newZ = Utils.FloorToNearestX(z, chunkSize);

            Vector3Int key = new Vector3Int(newX, newY, newZ);
            if (chunks.ContainsKey(key))
            {
                return chunks[key];
            }

            return null;
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

        public float GetDensity(Vector3Int pos)
        {
            return GetDensity(pos.x, pos.y, pos.z);
        }

        public void SetDensity(float density, int worldPosX, int worldPosY, int worldPosZ)
        {
            SetDensity(density, new Vector3Int(worldPosX, worldPosY, worldPosZ));
        }

        public void SetDensity(float density, Vector3Int pos)
        {
            for (int i = 0; i < 8; i++)
            {
                Vector3Int chunkPos = (pos - LookupTables.CubeCorners[i]).FloorToNearestX(chunkSize);
                Chunk chunk = GetChunk(chunkPos);
                if (chunk == null)
                {
                    continue;
                }

                Vector3Int localPos = (pos - chunk.position).Mod(chunkSize + 1);

                chunk.SetDensity(density, localPos);
                chunk.isDirty = true;
            }
        }

        private void CreateChunk(int x, int y, int z)
        {
            CreateChunk(new Vector3Int(x, y, z));
        }

        private Chunk CreateChunk(Vector3Int chunkPosition)
        {
            Chunk chunk = Instantiate(chunkPrefab, chunkPosition, Quaternion.identity).GetComponent<Chunk>();
            chunk.Initialize(this, chunkSize, chunkPosition);
            chunk.isDirty = false;
            chunks.Add(chunkPosition, chunk);
            chunk.Generate();

            return chunk;
        }
    }
}