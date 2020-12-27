using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    /// <summary>
    /// A container for all of the chunks in the world
    /// </summary>
    public class ChunkStore : PerChunkStore<ChunkProperties>
    {
        public override void GenerateDataForChunk(int3 chunkCoordinate)
        {
            if (!DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                ChunkProperties chunkProperties = new ChunkProperties();
                chunkProperties.Initialize(chunkCoordinate, VoxelWorld.WorldSettings.ChunkSize);
                AddChunk(chunkCoordinate, chunkProperties);
            }
        }

        public override void GenerateDataForChunk(int3 chunkCoordinate, ChunkProperties existingData)
        {
            if (!DoesChunkExistAtCoordinate(chunkCoordinate))
            {
                existingData.MeshCollider.enabled = false;
                existingData.MeshRenderer.enabled = false;
                existingData.Initialize(chunkCoordinate, VoxelWorld.WorldSettings.ChunkSize);
                AddChunk(chunkCoordinate, existingData);
            }
        }
    }
}