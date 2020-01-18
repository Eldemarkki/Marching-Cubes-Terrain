using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public abstract class World : MonoBehaviour
    {
        [SerializeField] private int chunkSize = 16;
        [SerializeField] private GameObject chunkPrefab;
        [SerializeField] private float isolevel;

        public int ChunkSize => chunkSize;
        public GameObject ChunkPrefab => chunkPrefab;
        public float Isolevel => isolevel;
        public Dictionary<int3, Chunk> Chunks { get; set; }

        protected virtual void Start()
        {
            Chunks = new Dictionary<int3, Chunk>();
        }
        
        public bool TryGetChunk(int3 worldPosition, out Chunk chunk)
        {
            int3 chunkCoordinate = WorldPositionToCoordinate(worldPosition);
            return Chunks.TryGetValue(chunkCoordinate, out chunk);
        }

        public float GetDensity(int3 worldPosition)
        {
            if (TryGetChunk(worldPosition, out Chunk chunk))
            {
                return chunk.GetDensity(worldPosition.Mod(ChunkSize));
            }

            return 0;
        }

        public void SetDensity(float density, int3 worldPosition)
        {
            List<int3> modifiedChunkPositions = new List<int3>();
            for (int i = 0; i < 8; i++)
            {
                int3 chunkPos = chunkSize * WorldPositionToCoordinate(worldPosition - LookupTables.CubeCorners[i]);
                if (modifiedChunkPositions.Contains(chunkPos)) { continue; }

                if (TryGetChunk(chunkPos, out Chunk chunk))
                {
                    int3 localPos = (worldPosition - chunkPos).Mod(ChunkSize + 1);
                    chunk.SetDensity(density, localPos);
                    modifiedChunkPositions.Add(chunkPos);
                }
            }
        }

        public int3 WorldPositionToCoordinate(float3 worldPosition)
        {
            return worldPosition.FloorToNearestX(ChunkSize) / ChunkSize;
        }
    }
}