using Eldemarkki.VoxelTerrain.Settings;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// A world generated from a heightmap
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
            CreateHeightmapTerrainImmediate();
        }

        private void OnDestroy()
        {
            heightmapTerrainSettings.Dispose();
        }

        /// <summary>
        /// Creates the heightmap terrain and loads the chunks.
        /// </summary>
        private void CreateHeightmapTerrainImmediate()
        {
            int chunkCountX = (int)math.ceil((float)(heightmapTerrainSettings.Width - 1) / voxelWorld.WorldSettings.ChunkSize.x);
            int chunkCountZ = (int)math.ceil((float)(heightmapTerrainSettings.Height - 1) / voxelWorld.WorldSettings.ChunkSize.z);
            int chunkCountY = (int)math.ceil(heightmapTerrainSettings.Amplitude / voxelWorld.WorldSettings.ChunkSize.y);

            for (int x = 0; x < chunkCountX; x++)
            {
                for (int y = 0; y < chunkCountY; y++)
                {
                    for (int z = 0; z < chunkCountZ; z++)
                    {
                        voxelWorld.ChunkProvider.CreateLoadedChunkToCoordinateImmediate(new int3(x, y, z));
                    }
                }
            }
        }
    }
}