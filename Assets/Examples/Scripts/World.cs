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

        public abstract Chunk GetChunk(int3 worldPosition);

        public abstract float GetDensity(int3 worldPosition);
        public abstract void SetDensity(float density, int3 worldPosition);
    }
}