using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    public class ChunkProperties
    {
        /// <summary>
        /// The game object that corresponds to these properties
        /// </summary>
        public GameObject ChunkGameObject { get; set; }
        public MeshFilter MeshFilter { get; set; }
        public MeshCollider MeshCollider { get; set; }
        public MeshRenderer MeshRenderer { get; set; }

        public int3 ChunkCoordinate { get; set; }

        /// <summary>
        /// Has the voxel data of this chunk been changed during the last frame
        /// </summary>
        public bool HasChanges { get; set; }
        public bool IsMeshGenerated { get; set; }

        /// <summary>
        /// Initializes the chunk's properties.
        /// </summary>
        /// <param name="chunkProperties">The chunk to initialize</param>
        /// <param name="chunkCoordinate">The new coordinate of the chunk</param>
        public void Initialize(int3 chunkCoordinate, int3 chunkSize)
        {
            ChunkGameObject.transform.position = (chunkCoordinate * chunkSize).ToVectorInt();
            ChunkGameObject.name = GetName(chunkCoordinate);
            ChunkCoordinate = chunkCoordinate;
        }

        /// <summary>
        /// Generates a chunk name from a chunk coordinate
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk</param>
        /// <returns>The name of the chunk</returns>
        public static string GetName(int3 chunkCoordinate)
        {
            return $"Chunk_{chunkCoordinate.x}_{chunkCoordinate.y}_{chunkCoordinate.z}";
        }
    }
}