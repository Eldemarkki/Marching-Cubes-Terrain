﻿using Eldemarkki.VoxelTerrain.Meshing;
using Eldemarkki.VoxelTerrain.Settings;
using Eldemarkki.VoxelTerrain.VoxelData;
using Eldemarkki.VoxelTerrain.World.Chunks;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    public class VoxelWorld : MonoBehaviour
    {
        [SerializeField] private WorldSettings worldSettings;
        public WorldSettings WorldSettings => worldSettings;

        [SerializeField] private Transform player;
        public Transform Player => player;

        [SerializeField] private VoxelMesher voxelMesher;
        public VoxelMesher VoxelMesher => voxelMesher;

        [SerializeField] private VoxelDataStore voxelDataStore;
        public VoxelDataStore VoxelDataStore => voxelDataStore;

        [SerializeField] private VoxelColorStore voxelColorStore;
        public VoxelColorStore VoxelColorStore => voxelColorStore;

        [SerializeField] private VoxelDataGenerator voxelDataGenerator;
        public VoxelDataGenerator VoxelDataGenerator => voxelDataGenerator;

        [SerializeField] private ChunkProvider chunkProvider;
        public ChunkProvider ChunkProvider => chunkProvider;

        [SerializeField] private ChunkStore chunkStore;
        public ChunkStore ChunkStore => chunkStore;

        [SerializeField] private ChunkUpdater chunkUpdater;
        public ChunkUpdater ChunkUpdater => chunkUpdater;

        private void Awake()
        {
            voxelDataStore.VoxelWorld = this;
            voxelColorStore.VoxelWorld = this;
            chunkProvider.VoxelWorld = this;
            chunkUpdater.VoxelWorld = this;
            chunkStore.VoxelWorld = this;
            voxelMesher.VoxelWorld = this;
        }

        // The stores have to be disposed here manually in order to guarantee correct order
        private void OnApplicationQuit()
        {
            chunkStore.Dispose();
            voxelDataStore.Dispose();
            voxelColorStore.Dispose();
        }
    }
}