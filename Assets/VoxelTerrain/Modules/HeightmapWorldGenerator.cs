using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A procedurally generated world
    /// </summary>
    public class HeightmapWorldGenerator : MonoBehaviour
    {
        /// <summary>
        /// The voxel world the "owns" this world generator
        /// </summary>
        [SerializeField] private VoxelWorld voxelWorld;

        /// <summary>
        /// The settings for generating the heightmap terrain
        /// </summary>
        [SerializeField] private HeightmapTerrainSettings heightmapTerrainSettings;

        /// <summary>
        /// The settings for generating the heightmap terrain
        /// </summary>
        public HeightmapTerrainSettings HeightmapTerrainSettings => heightmapTerrainSettings;

        private void Awake()
        {
            heightmapTerrainSettings.Initialize(heightmapTerrainSettings.Heightmap, heightmapTerrainSettings.Amplitude, heightmapTerrainSettings.HeightOffset);
        }

        private void Start()
        {
            CreateHeightmapTerrain();
        }

        private void OnDestroy()
        {
            heightmapTerrainSettings.Dispose();
        }

        /// <summary>
        /// Creates the heightmap terrain and instantiates the chunks.
        /// </summary>
        private void CreateHeightmapTerrain()
        {
            int chunkCountX = Mathf.CeilToInt((float)(heightmapTerrainSettings.Width - 1) / voxelWorld.WorldSettings.ChunkSize);
            int chunkCountZ = Mathf.CeilToInt((float)(heightmapTerrainSettings.Height - 1) / voxelWorld.WorldSettings.ChunkSize);
            int chunkCountY = Mathf.CeilToInt(heightmapTerrainSettings.Amplitude / voxelWorld.WorldSettings.ChunkSize);

            for (int x = 0; x < chunkCountX; x++)
            {
                for (int y = 0; y < chunkCountY; y++)
                {
                    for (int z = 0; z < chunkCountZ; z++)
                    {
                        voxelWorld.ChunkProvider.EnsureChunkExistsAtCoordinate(new int3(x, y, z));
                    }
                }
            }
        }
    }
}