using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.World.Chunks;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    /// <summary>
    /// The main entry point for interacting with the voxel world
    /// </summary>
    public class VoxelWorld : MonoBehaviour
    {
        [SerializeField] private WorldSettings worldSettings;
        public WorldSettings WorldSettings => worldSettings;

        [SerializeField] private VoxelMesher voxelMesher;
        public VoxelMesher VoxelMesher => voxelMesher;

        [SerializeField] private VoxelDataStore voxelDataStore;
        public VoxelDataStore VoxelDataStore => voxelDataStore;

        [SerializeField] private VoxelDataGenerator voxelDataGenerator;
        public VoxelDataGenerator VoxelDataGenerator => voxelDataGenerator;

        [SerializeField] private ChunkProvider chunkProvider;
        public ChunkProvider ChunkProvider => chunkProvider;

        [SerializeField] private ChunkStore chunkStore;
        public ChunkStore ChunkStore => chunkStore;

        [SerializeField] private ChunkLoader chunkLoader;
        public ChunkLoader ChunkLoader => chunkLoader;

        void Awake()
        {
            voxelDataStore.VoxelWorld = this;
            chunkProvider.VoxelWorld = this;
            chunkLoader.VoxelWorld = this;
        }
    }
}