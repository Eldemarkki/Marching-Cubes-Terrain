using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;

namespace Eldemarkki.VoxelTerrain.MarchingCubes
{
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

        public override Mesh CreateMesh(VoxelDataStore voxelDataStore, int3 chunkCoordinate)
        {
            DensityVolume boundsVoxelData = voxelDataStore.GetDensityChunk(chunkCoordinate);
            Counter counter = new Counter(Allocator.TempJob);

            int voxelCount = (boundsVoxelData.Width - 1) * (boundsVoxelData.Height - 1) * (boundsVoxelData.Depth - 1);
            int maxLength = 15 * voxelCount;

            var outputVertices = new NativeArray<MarchingCubesVertexData>(maxLength, Allocator.TempJob);
            var outputTriangles = new NativeArray<ushort>(maxLength, Allocator.TempJob);

            var marchingCubesJob = new MarchingCubesJob
            {
                VoxelData = boundsVoxelData,
                isolevel = Isolevel,
                counter = counter,

                OutputVertices = outputVertices,
                OutputTriangles = outputTriangles
            };

            JobHandle jobHandle = marchingCubesJob.Schedule(voxelCount, 128);

            Mesh mesh = new Mesh();
            var subMesh = new SubMeshDescriptor(0, 0, MeshTopology.Triangles);

            jobHandle.Complete();

            int vertexCount = counter.Count * 3;
            counter.Dispose();

            mesh.SetVertexBufferParams(vertexCount, VertexBufferMemoryLayout);
            mesh.SetIndexBufferParams(vertexCount, IndexFormat.UInt16);

            mesh.SetVertexBufferData(outputVertices, 0, 0, vertexCount, 0, MeshUpdateFlags.DontValidateIndices);
            mesh.SetIndexBufferData(outputTriangles, 0, 0, vertexCount, MeshUpdateFlags.DontValidateIndices);

            outputVertices.Dispose();
            outputTriangles.Dispose();

            mesh.subMeshCount = 1;
            subMesh.indexCount = vertexCount;
            mesh.SetSubMesh(0, subMesh);

            mesh.RecalculateBounds();

            return mesh;
        }
    }
}
