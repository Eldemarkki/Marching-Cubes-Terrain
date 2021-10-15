using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    public class FixedSizeWorldGenerator : MonoBehaviour
    {
        [SerializeField] private VoxelWorld voxelWorld;
        [SerializeField] private int3 worldSize;

        private void Start()
        {
            for (int x = 0; x < worldSize.x; x++)
            {
                for (int y = 0; y < worldSize.y; y++)
                {
                    for (int z = 0; z < worldSize.z; z++)
                    {
                        var generationCoordinate = new int3(x, y, z);
                        voxelWorld.ChunkProvider.EnsureChunkExistsAtCoordinate(generationCoordinate);
                    }
                }
            }
        }
    }
}