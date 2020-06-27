using Eldemarkki.VoxelTerrain.VoxelData;
using Eldemarkki.VoxelTerrain.World.Chunks;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// The main entry point for interacting with the voxel world
    /// </summary>
    public class VoxelWorld : MonoBehaviour
    {
        [SerializeField] private WorldSettings worldSettings = null;
        public WorldSettings WorldSettings => worldSettings;

        [SerializeField] private VoxelMesher voxelMesher = null;
        public VoxelMesher VoxelMesher => voxelMesher;

        [SerializeField] private VoxelDataStore voxelDataStore = null;
        public VoxelDataStore VoxelDataStore => voxelDataStore;

        [SerializeField] private VoxelDataGenerator voxelDataGenerator = null;
        public VoxelDataGenerator VoxelDataGenerator => voxelDataGenerator;

        [SerializeField] private ChunkProvider chunkProvider = null;
        public ChunkProvider ChunkProvider => chunkProvider;

        [SerializeField] private ChunkStore chunkStore = null;
        public ChunkStore ChunkStore => chunkStore;

        [SerializeField] private ChunkLoader chunkLoader = null;
        public ChunkLoader ChunkLoader => chunkLoader;

        private void Awake()
        {
            voxelDataStore.VoxelWorld = this;
            chunkProvider.VoxelWorld = this;
            chunkLoader.VoxelWorld = this;
        }
    }
}