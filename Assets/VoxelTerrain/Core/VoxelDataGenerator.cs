using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World
{
    public abstract class VoxelDataGenerator : MonoBehaviour
    {
        public VoxelWorld VoxelWorld { get; set; }
        public abstract JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(Bounds bounds, Allocator allocator = Allocator.Persistent);
        public virtual JobHandleWithData<IVoxelDataGenerationJob> GenerateVoxelData(int3 chunkCoordinate, Allocator allocator = Allocator.Persistent)
        {
            Bounds bounds = BoundsUtilities.GetChunkBounds(chunkCoordinate, VoxelWorld.WorldSettings.ChunkSize);
            return GenerateVoxelData(bounds, allocator);
        }
    }
}