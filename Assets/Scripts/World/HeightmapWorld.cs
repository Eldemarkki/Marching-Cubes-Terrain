using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A heightmap world generated from a heightmap
    /// </summary>
    [RequireComponent(typeof(HeightmapChunkProvider))]
    public class HeightmapWorld : VoxelWorld<HeightmapChunk>
    {
        /// <summary>
        /// The heightmap terrain generation settings
        /// </summary>
        [SerializeField] private HeightmapTerrainSettings heightmapTerrainSettings = null;

        protected override void Awake()
        {
            base.Awake();
            heightmapTerrainSettings.Initialize(heightmapTerrainSettings.Heightmap, heightmapTerrainSettings.Amplitude, heightmapTerrainSettings.HeightOffset);
        }

        private void Start()
        {
            CreateHeightmapTerrain();
        }

        private void OnDestroy()
        {
            heightmapTerrainSettings.DisposeHeightmapData();
        }

        /// <summary>
        /// Creates the heightmap terrain and instantiates the chunks.
        /// </summary>
        private void CreateHeightmapTerrain()
        {
            int chunkCountX = Mathf.CeilToInt((float) (heightmapTerrainSettings.Width - 1) / ChunkProvider.ChunkGenerationParams.ChunkSize);
            int chunkCountZ = Mathf.CeilToInt((float) (heightmapTerrainSettings.Height - 1) / ChunkProvider.ChunkGenerationParams.ChunkSize);
            int chunkCountY = Mathf.CeilToInt(heightmapTerrainSettings.Amplitude / ChunkProvider.ChunkGenerationParams.ChunkSize);

            for (int x = 0; x < chunkCountX; x++)
            {
                for (int y = 0; y < chunkCountY; y++)
                {
                    for (int z = 0; z < chunkCountZ; z++)
                    {
                        ChunkProvider.CreateChunkAtCoordinate(new int3(x, y, z));
                    }
                }
            }
        }
    }
}