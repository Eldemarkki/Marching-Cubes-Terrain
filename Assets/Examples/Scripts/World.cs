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
        
        public virtual bool TryGetChunk(int3 worldPosition, out Chunk chunk)
        {
            int newX = Utils.FloorToNearestX((float)worldPosition.x, ChunkSize) / ChunkSize;
            int newY = Utils.FloorToNearestX((float)worldPosition.y, ChunkSize) / ChunkSize;
            int newZ = Utils.FloorToNearestX((float)worldPosition.z, ChunkSize) / ChunkSize;

            int3 key = new int3(newX, newY, newZ);
            return Chunks.TryGetValue(key, out chunk);
        }

        public virtual float GetDensity(int3 worldPosition)
        {
            if (TryGetChunk(worldPosition, out Chunk chunk))
            {
                return chunk.GetDensity(worldPosition.Mod(ChunkSize));
            }

            return 0;
        }

        public virtual void SetDensity(float density, int3 pos)
        {
            List<int3> chunks = new List<int3>();
            for (int i = 0; i < 8; i++)
            {
                int3 chunkPos = (pos - LookupTables.CubeCorners[i]).FloorToNearestX(ChunkSize);
                if (chunks.Contains(chunkPos)) { continue; }

                if (TryGetChunk(chunkPos, out Chunk chunk))
                {
                    int3 localPos = (pos - chunkPos).Mod(ChunkSize + 1);
                    chunk.SetDensity(density, localPos.x, localPos.y, localPos.z);
                    chunks.Add(chunkPos);
                }
            }
        }
    }
}