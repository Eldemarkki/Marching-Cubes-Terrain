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
        /// A chunk provider which providers chunks with heightmap data
        /// </summary>
        [SerializeField] private HeightmapChunkProvider chunkProvider;

        private void Start()
        {
            CreateHeightmapTerrain();
        }

        /// <summary>
        /// Creates the heightmap terrain and instantiates the chunks.
        /// </summary>
        private void CreateHeightmapTerrain()
        {
            int chunkCountX = Mathf.CeilToInt((float)(chunkProvider.HeightmapTerrainSettings.Width - 1) / chunkProvider.ChunkGenerationParams.ChunkSize);
            int chunkCountZ = Mathf.CeilToInt((float)(chunkProvider.HeightmapTerrainSettings.Height - 1) / chunkProvider.ChunkGenerationParams.ChunkSize);
            int chunkCountY = Mathf.CeilToInt(chunkProvider.HeightmapTerrainSettings.Amplitude / chunkProvider.ChunkGenerationParams.ChunkSize);

            for (int x = 0; x < chunkCountX; x++)
            {
                for (int y = 0; y < chunkCountY; y++)
                {
                    for (int z = 0; z < chunkCountZ; z++)
                    {
                        chunkProvider.CreateChunkAtCoordinate(new int3(x, y, z));
                    }
                }
            }
        }
    }
}