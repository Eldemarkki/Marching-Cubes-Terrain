using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public class HeightmapWorld : World
    {
        [SerializeField] private HeightmapTerrainSettings heightmapTerrainSettings;

        private Dictionary<int3, HeightmapChunk> _chunks;

        public HeightmapTerrainSettings HeightmapTerrainSettings { get => heightmapTerrainSettings; set => heightmapTerrainSettings = value; }

        private void Awake()
        {
            _chunks = new Dictionary<int3, HeightmapChunk>();
            heightmapTerrainSettings = new HeightmapTerrainSettings(heightmapTerrainSettings.Heightmap, heightmapTerrainSettings.Amplitude, heightmapTerrainSettings.HeightOffset);
        }

        private void Start()
        {
            CreateHeightmapTerrain();
        }

        private void OnDestroy()
        {
            HeightmapTerrainSettings.Dispose();
        }

        private void CreateHeightmapTerrain()
        {
            int chunkCountX = Mathf.CeilToInt((float)(heightmapTerrainSettings.Width - 1) / ChunkSize);
            int chunkCountZ = Mathf.CeilToInt((float)(heightmapTerrainSettings.Height - 1) / ChunkSize);
            int chunkCountY = Mathf.CeilToInt((float)heightmapTerrainSettings.Amplitude / ChunkSize);

            for (int x = 0; x < chunkCountX; x++)
            {
                for (int y = 0; y < chunkCountY; y++)
                {
                    for (int z = 0; z < chunkCountZ; z++)
                    {
                        CreateChunk(new int3(x, y, z));
                    }
                }
            }
        }

        public override Chunk GetChunk(int3 worldPosition)
        {
            int newX = Utils.FloorToNearestX((float)worldPosition.x, ChunkSize) / ChunkSize;
            int newY = Utils.FloorToNearestX((float)worldPosition.y, ChunkSize) / ChunkSize;
            int newZ = Utils.FloorToNearestX((float)worldPosition.z, ChunkSize) / ChunkSize;

            int3 key = new int3(newX, newY, newZ);
            if (_chunks.TryGetValue(key, out HeightmapChunk chunk))
            {
                return chunk;
            }

            return null;
        }

        public HeightmapChunk CreateChunk(int3 chunkCoordinate)
        {
            HeightmapChunk chunk = Instantiate(ChunkPrefab, (chunkCoordinate * ChunkSize).ToVectorInt(), Quaternion.identity).GetComponent<HeightmapChunk>();
            chunk.World = this;
            chunk.Initialize(ChunkSize, Isolevel, chunkCoordinate);
            _chunks.Add(chunkCoordinate, chunk);

            return chunk;
        }

        public override float GetDensity(int3 worldPosition)
        {
            Chunk chunk = GetChunk(worldPosition);
            if (chunk == null) { return 0; }

            float density = chunk.GetDensity(worldPosition.x.Mod(ChunkSize),
                                             worldPosition.y.Mod(ChunkSize),
                                             worldPosition.z.Mod(ChunkSize));
            return density;
        }

        public override void SetDensity(float density, int3 pos)
        {
            for (int i = 0; i < 8; i++)
            {
                int3 chunkPos = (pos - LookupTables.CubeCorners[i]).FloorToNearestX(ChunkSize);
                Chunk chunk = GetChunk(chunkPos);
                if (chunk != null)
                {
                    int3 localPos = (pos - chunkPos).Mod(ChunkSize + 1);
                    chunk.SetDensity(density, localPos.x, localPos.y, localPos.z);
                }
            }
        }
    }
}