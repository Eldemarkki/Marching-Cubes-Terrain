using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public abstract class World : MonoBehaviour
    {
        [SerializeField] private int chunkSize = 16;
        [SerializeField] private GameObject chunkPrefab;
        [SerializeField] private float isolevel;

        public int ChunkSize { get => chunkSize; set => chunkSize = value; }
        public GameObject ChunkPrefab { get => chunkPrefab; set => chunkPrefab = value; }
        public float Isolevel { get => isolevel; set => isolevel = value; }

        public abstract Chunk GetChunk(int3 worldPosition);

        public abstract float GetDensity(int3 worldPosition);
        public abstract void SetDensity(float density, int3 worldPosition);
    }
}