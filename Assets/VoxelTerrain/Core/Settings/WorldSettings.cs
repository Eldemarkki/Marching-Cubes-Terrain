using Eldemarkki.VoxelTerrain.World.Chunks;
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
        [SerializeField] private int chunkSize = 16;

        /// <summary>
        /// The chunk's prefab that will be instantiated
        /// </summary>
        [SerializeField] private Chunk chunkPrefab = null;

        /// <summary>
        /// The chunk's size. This represents the width, height and depth in Unity units.
        /// </summary>
        public int ChunkSize => chunkSize;

        /// <summary>
        /// The chunk's prefab that will be instantiated
        /// </summary>
        public Chunk ChunkPrefab => chunkPrefab;
    }
}