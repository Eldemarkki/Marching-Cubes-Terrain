using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Settings
{
    [System.Serializable]
    public class WorldSettings
    {
        [SerializeField] private int3 chunkSize = 16;
        [SerializeField] private GameObject chunkPrefab;

        public int3 ChunkSize => chunkSize;
        public GameObject ChunkPrefab => chunkPrefab;
    }
}