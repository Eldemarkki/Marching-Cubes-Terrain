using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.VoxelData;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Meshing
{
    public abstract class VoxelMesher : MonoBehaviour
    {
        /// <summary>
        /// Starts a mesh generation job
        /// </summary>
        /// <param name="voxelDataStore">The store where to retrieve the voxel data from</param>
        /// <param name="chunkCoordinate">The coordinate of the chunk that will be generated</param>
        /// <returns>The job handle and the actual mesh generation job</returns>
        public abstract JobHandleWithData<IMesherJob> CreateMesh(VoxelDataStore voxelDataStore, int3 chunkCoordinate);
    }
}