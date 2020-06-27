using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.VoxelData;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Meshing.MarchingCubes
{
    /// <summary>
    /// A mesher for the marching cubes algorithm
    /// </summary>
    public class MarchingCubesMesher : VoxelMesher
    {
        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        [SerializeField] private float isolevel = 0;

        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        public float Isolevel => isolevel;

        /// <summary>
        /// Starts a mesh generation job
        /// </summary>
        /// <param name="voxelDataStore">The store where to retrieve the voxel data from</param>
        /// <param name="chunkCoordinate">The coordinate of the chunk that will be generated</param>
        /// <returns>The job handle and the actual mesh generation job</returns>
        public override JobHandleWithData<IMesherJob> CreateMesh(VoxelDataStore voxelDataStore, int3 chunkCoordinate)
        {
            VoxelDataVolume boundsVoxelData = voxelDataStore.GetVoxelDataChunk(chunkCoordinate);
            NativeCounter vertexCountCounter = new NativeCounter(Allocator.TempJob);

            int voxelCount = (boundsVoxelData.Width - 1) * (boundsVoxelData.Height - 1) * (boundsVoxelData.Depth - 1);
            int maxLength = 15 * voxelCount;

            NativeArray<MeshingVertexData> outputVertices = new NativeArray<MeshingVertexData>(maxLength, Allocator.TempJob);
            NativeArray<ushort> outputTriangles = new NativeArray<ushort>(maxLength, Allocator.TempJob);

            MarchingCubesJob marchingCubesJob = new MarchingCubesJob
            {
                VoxelData = boundsVoxelData,
                isolevel = Isolevel,
                VertexCountCounter = vertexCountCounter,

                OutputVertices = outputVertices,
                OutputTriangles = outputTriangles
            };

            JobHandle jobHandle = marchingCubesJob.Schedule(voxelCount, 128);

            JobHandleWithData<IMesherJob> jobHandleWithData = new JobHandleWithData<IMesherJob>();
            jobHandleWithData.JobHandle = jobHandle;
            jobHandleWithData.JobData = marchingCubesJob;

            return jobHandleWithData;
        }
    }
}
