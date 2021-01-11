using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.World.Chunks
{
    /// <summary>
    /// A class that contains properties for a chunk
    /// </summary>
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
        //public bool HasChanges { get; set; }
        public bool IsMeshGenerated { get; set; }

        /// <summary>
        /// Initializes the chunk's properties.
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of this chunk</param>
        /// <param name="chunkSize">The size of this chunk</param>
        public void Initialize(int3 chunkCoordinate, int3 chunkSize)
        {
            ChunkGameObject.transform.position = (chunkCoordinate * chunkSize).ToVectorInt();
            ChunkCoordinate = chunkCoordinate;

            IsMeshGenerated = false;
            //HasChanges = false;
        }
    }
}