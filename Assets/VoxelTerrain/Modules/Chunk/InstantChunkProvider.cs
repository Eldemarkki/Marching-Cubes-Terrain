using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    public class InstantChunkProvider : ChunkProvider
    {
        /// <summary>
        /// Instantiates a chunk to <paramref name="chunkCoordinate"/>, initializes it and generates its mesh immediately
        /// </summary>
        /// <returns>The new chunk</returns>
        public override ChunkProperties EnsureChunkExistsAtCoordinate(int3 chunkCoordinate)
        {
            ChunkProperties chunkProperties = CreateUnloadedChunkToCoordinate(chunkCoordinate);
            VoxelWorld.ChunkUpdater.GenerateChunkImmediate(chunkProperties);
            return chunkProperties;
        }
    }
}