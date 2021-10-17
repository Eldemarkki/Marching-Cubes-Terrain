using Eldemarkki.VoxelTerrain.Settings;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    public class HeightmapWorldGenerator : MonoBehaviour
    {
        [SerializeField] private VoxelWorld voxelWorld;
        [SerializeField] private HeightmapTerrainSettings heightmapTerrainSettings;
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
                        voxelWorld.ChunkProvider.EnsureChunkExistsAtCoordinate(new int3(x, y, z));
                    }
                }
            }
        }
    }
}