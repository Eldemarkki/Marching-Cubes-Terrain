using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using System;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    /// <summary>
    /// A class that contains properties for a chunk
    /// </summary>
    public class ChunkProperties : IDisposable
    {
        private GameObject _chunkGameObject;

        /// <summary>
        /// The game object that corresponds to these properties
        /// </summary>
        public GameObject ChunkGameObject
        {
            get => _chunkGameObject;
            set
            {
                _chunkGameObject = value;
                MeshCollider = value.GetComponent<MeshCollider>();
                MeshFilter = value.GetComponent<MeshFilter>();
                MeshRenderer = value.GetComponent<MeshRenderer>();
            }
        }

        public MeshFilter MeshFilter { get; private set; }
        public MeshCollider MeshCollider { get; private set; }
        public MeshRenderer MeshRenderer { get; private set; }

        public int3 ChunkCoordinate { get; set; }

        public bool IsMeshGenerated { get; set; }

        private int3 _chunkSize;

        public JobHandle DataGenerationJobHandle { get; set; }

        public NativeList<MeshingVertexData> OutputVertices { get; private set; }
        public NativeList<ushort> OutputTriangles { get; private set; }

        public ChunkProperties(int3 chunkSize)
        {
            _chunkSize = chunkSize;
            int voxelCount = chunkSize.x * chunkSize.y * chunkSize.z;

            // Some arbitrary value, currently it assumes that 20% (=0.2) of the voxels will have 15 vertices or less
            int listInitialSize = (int)(voxelCount * 15 * 0.2f);

            OutputVertices = new NativeList<MeshingVertexData>(listInitialSize, Allocator.Persistent);
            OutputTriangles = new NativeList<ushort>(listInitialSize, Allocator.Persistent);
        }

        public void Move(int3 chunkCoordinate)
        {
            ChunkGameObject.transform.position = (chunkCoordinate * _chunkSize).ToVectorInt();
            ChunkCoordinate = chunkCoordinate;

            IsMeshGenerated = false;
        }

        public void Dispose()
        {
            OutputVertices.Dispose();
            OutputTriangles.Dispose();
        }
    }
}