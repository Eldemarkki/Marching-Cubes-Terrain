﻿using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    /// <summary>
    /// A class that contains properties for a chunk
    /// </summary>
    public class ChunkProperties
    {
        /// <summary>
        /// The game object that corresponds to these properties
        /// </summary>
        public GameObject ChunkGameObject { get; set; }
        public MeshFilter MeshFilter { get; set; }
        public MeshCollider MeshCollider { get; set; }
        public MeshRenderer MeshRenderer { get; set; }

        public int3 ChunkCoordinate { get; set; }

        /// <summary>
        /// Has the voxel data of this chunk been changed during the last frame
        /// </summary>
        public bool HasChanges { get; set; }
        public bool IsMeshGenerated { get; set; }

        public NativeArray<MeshingVertexData> Vertices;
        public NativeArray<ushort> Triangles;

        /// <summary>
        /// Initializes the chunk's properties.
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of this chunk</param>
        /// <param name="chunkSize">The size of this chunk</param>
        public void Initialize(int3 chunkCoordinate, int3 chunkSize)
        {
#if UNITY_EDITOR
            ChunkGameObject.name = GetName(chunkCoordinate);
#endif
            ChunkGameObject.transform.position = (chunkCoordinate * chunkSize).ToVectorInt();
            ChunkCoordinate = chunkCoordinate;

            IsMeshGenerated = false;
            HasChanges = false;

            int maxLength = 15 * chunkSize.x * chunkSize.y * chunkSize.z;
            if (!Vertices.IsCreated)
            {
                Vertices = new NativeArray<MeshingVertexData>(maxLength, Allocator.Persistent);
            }
            if (!Triangles.IsCreated)
            {
                Triangles = new NativeArray<ushort>(maxLength, Allocator.Persistent);
            }
        }

        /// <summary>
        /// Generates a chunk name from a chunk coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk</param>
        /// <returns>The name of the chunk</returns>
        public static string GetName(int3 chunkCoordinate)
        {
            return $"Chunk_{chunkCoordinate.x}_{chunkCoordinate.y}_{chunkCoordinate.z}";
        }
    }
}