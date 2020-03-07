using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// A heightmap world generated from a heightmap
    /// </summary>
    public class HeightmapWorld : World
    {
        /// <summary>
        /// The heightmap terrain generation settings
        /// </summary>
        [SerializeField] private HeightmapTerrainSettings heightmapTerrainSettings;

        /// <summary>
        /// The heightmap terrain generation settings
        /// </summary>
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
            HeightmapTerrainSettings.DisposeHeightmapData();
        }

        /// <summary>
        /// Creates the heightmap terrain and instantiates the chunks.
        /// </summary>
        private void CreateHeightmapTerrain()
        {
            int chunkCountX = Mathf.CeilToInt((float) (heightmapTerrainSettings.Width - 1) / ChunkSize);
            int chunkCountZ = Mathf.CeilToInt((float) (heightmapTerrainSettings.Height - 1) / ChunkSize);
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

        /// <summary>
        /// Instantiates a chunk to the specified coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The chunk's coordinate</param>
        /// <returns>The instantiated chunk</returns>
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