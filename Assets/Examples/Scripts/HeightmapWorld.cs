using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public class HeightmapWorld : World
    {
        [SerializeField] private HeightmapTerrainSettings heightmapTerrainSettings;
        public HeightmapTerrainSettings HeightmapTerrainSettings => heightmapTerrainSettings;

        private void Awake()
        {
            heightmapTerrainSettings = new HeightmapTerrainSettings(heightmapTerrainSettings.Heightmap, heightmapTerrainSettings.Amplitude, heightmapTerrainSettings.HeightOffset);
        }

        protected override void Start()
        {
            base.Start();
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
            int chunkCountY = Mathf.CeilToInt(heightmapTerrainSettings.Amplitude / ChunkSize);

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

        private HeightmapChunk CreateChunk(int3 chunkCoordinate)
        {
            HeightmapChunk chunk = Instantiate(ChunkPrefab, (chunkCoordinate * ChunkSize).ToVectorInt(), Quaternion.identity).GetComponent<HeightmapChunk>();
            chunk.name = $"Chunk_{chunkCoordinate.x}_{chunkCoordinate.y}_{chunkCoordinate.z}";
            chunk.World = this;
            chunk.Initialize(ChunkSize, Isolevel, chunkCoordinate);
            Chunks.Add(chunkCoordinate, chunk);

            return chunk;
        }
    }
}