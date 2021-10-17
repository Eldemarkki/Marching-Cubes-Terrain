﻿using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.VoxelData;
using Eldemarkki.VoxelTerrain.World;
using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Jobs;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Meshing
{
    public abstract class VoxelMesher : MonoBehaviour
    {
        public VoxelWorld VoxelWorld { get; set; }

        /// <summary>
        /// Starts a mesh generation job
        /// </summary>
        /// <param name="voxelDataStore">The store where to retrieve the voxel data from</param>
        /// <param name="voxelColorStore">The store where to retrieve the voxels' color data from</param>
        /// <param name="chunkProperties">The chunk that will be generated</param>
        /// <returns>The job handle and the actual mesh generation job</returns>
        public abstract JobHandleWithDataAndChunkProperties<IMesherJob> CreateMesh(VoxelDataStore voxelDataStore, VoxelColorStore voxelColorStore, ChunkProperties chunkProperties, JobHandle dependency);
    }
}