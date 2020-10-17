using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Settings
{
    /// <summary>
    /// Parameters that specify how the world will be generated
    /// </summary>
    [System.Serializable]
    public class WorldSettings
    {
        /// <summary>
        /// The chunk's size. This represents the width, height and depth in Unity units.
        /// </summary>
        [SerializeField] private int3 chunkSize = new int3(16, 16, 16);

        /// <summary>
        /// The chunk's prefab that will be instantiated
        /// </summary>
        [SerializeField] private GameObject chunkPrefab;

        /// <summary>
        /// The chunk's size. This represents the width, height and depth in Unity units.
        /// </summary>
        public int3 ChunkSize => chunkSize;

        /// <summary>
        /// The chunk's prefab that will be instantiated
        /// </summary>
        public GameObject ChunkPrefab => chunkPrefab;
    }
}