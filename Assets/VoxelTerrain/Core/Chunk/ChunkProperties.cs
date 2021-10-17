﻿using Eldemarkki.VoxelTerrain.Meshing;
using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    public class ChunkProperties : IDisposable
    {
        private GameObject _chunkGameObject;

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

        public NativeList<MeshingVertexData> OutputVertices { get; private set; }
        public NativeList<ushort> OutputTriangles { get; private set; }
        public JobHandleWithDataAndChunkProperties<IMesherJob> MeshingJobHandle { get; set; }

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
            if (MeshingJobHandle != null)
            {
                MeshingJobHandle.JobHandle.Complete();
                MeshingJobHandle.JobData.OutputTriangles.Dispose();
                MeshingJobHandle.JobData.OutputVertices.Dispose();
            }
            else
            {
                OutputVertices.Dispose();
                OutputTriangles.Dispose();
            }
        }
    }
}