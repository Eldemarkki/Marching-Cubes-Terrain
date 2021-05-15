using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.VoxelData;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World.Chunks;
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
        /// <inheritdoc cref="Isolevel"/>
        [SerializeField, Range(0, 1)] private float isolevel = 0.5f;

        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        public float Isolevel => isolevel;

        /// <inheritdoc/>
        public override JobHandleWithDataAndChunkProperties<IMesherJob> CreateMesh(VoxelDataStore voxelDataStore, VoxelColorStore voxelColorStore, ChunkProperties chunkProperties, JobHandle dependency)
        {
            int3 chunkCoordinate = chunkProperties.ChunkCoordinate;
            voxelDataStore.TryGetDataChunkWithoutApplyingChangesIncludeQueue(chunkCoordinate, out VoxelDataVolume<byte> boundsVoxelData);

            if (!voxelColorStore.TryGetDataChunk(chunkCoordinate, out VoxelDataVolume<Color32> boundsVoxelColors))
            {
                return null;
            }

            NativeCounter vertexCountCounter = new NativeCounter(Allocator.TempJob);

            int voxelCount = VoxelWorld.WorldSettings.ChunkSize.x * VoxelWorld.WorldSettings.ChunkSize.y * VoxelWorld.WorldSettings.ChunkSize.z;
            int maxLength = 15 * voxelCount;

            NativeArray<MeshingVertexData> outputVertices = new NativeArray<MeshingVertexData>(maxLength, Allocator.TempJob);
            NativeArray<ushort> outputTriangles = new NativeArray<ushort>(maxLength, Allocator.TempJob);

            MarchingCubesJob marchingCubesJob = new MarchingCubesJob
            {
                VoxelData = boundsVoxelData,
                VoxelColors = boundsVoxelColors,
                Isolevel = Isolevel,
                VertexCountCounter = vertexCountCounter,

                OutputVertices = outputVertices,
                OutputTriangles = outputTriangles
            };

            JobHandle jobHandle = marchingCubesJob.Schedule(voxelCount, 256, dependency);

            JobHandleWithDataAndChunkProperties<IMesherJob> jobHandleWithData = new JobHandleWithDataAndChunkProperties<IMesherJob>
            {
                JobHandle = jobHandle,
                JobData = marchingCubesJob,
                ChunkProperties = chunkProperties
            };

            return jobHandleWithData;
        }
    }
}
