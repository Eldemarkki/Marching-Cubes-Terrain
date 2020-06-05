using Eldemarkki.VoxelTerrain.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.MarchingCubes
{
    /// <summary>
    /// A marching cubes mesh generation job
    /// </summary>
    [BurstCompile]
    public struct MarchingCubesJob : IJobParallelFor
    {
        /// <summary>
        /// The densities to generate the mesh off of
        /// </summary>
        [ReadOnly] public DensityStorage densityStorage;
        
        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        [ReadOnly] public float isolevel;
        
        /// <summary>
        /// The chunk's size. This represents the width, height and depth in Unity units.
        /// </summary>
        [ReadOnly] public int chunkSize;
        
        /// <summary>
        /// The counter to keep track of the triangle index
        /// </summary>
        [WriteOnly] public Counter counter;

        /// <summary>
        /// The generated vertices
        /// </summary>
        [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<MarchingCubesVertexData> vertices;
        
        /// <summary>
        /// The generated triangles
        /// </summary>
        [NativeDisableParallelForRestriction, WriteOnly] public NativeArray<ushort> triangles;

        /// <summary>
        /// The execute method required by the Unity Job System's IJobParallelFor
        /// </summary>
        /// <param name="index">The iteration index</param>
        public void Execute(int index)
        {
            // Voxel's position inside the chunk. Goes from (0, 0, 0) to (chunkSize-1, chunkSize-1, chunkSize-1)
            int3 voxelLocalPosition = new int3(
                index / (chunkSize * chunkSize),
                index / chunkSize % chunkSize,
                index % chunkSize);
            
            VoxelCorners<float> densities = densityStorage.GetCubeVolume(voxelLocalPosition);

            int cubeIndex = MarchingCubesFunctions.CalculateCubeIndex(densities, isolevel);
            if (cubeIndex == 0 || cubeIndex == 255)
            {
                return;
            }

            VoxelCorners<int3> corners = MarchingCubesFunctions.GetCorners(voxelLocalPosition);

            int edgeIndex = MarchingCubesLookupTables.EdgeTable[cubeIndex];

            VertexList vertexList = MarchingCubesFunctions.GenerateVertexList(densities, corners, edgeIndex, isolevel);

            // Index at the beginning of the row
            int rowIndex = 15 * cubeIndex;

            for (int i = 0; MarchingCubesLookupTables.TriangleTable[rowIndex+i] != -1 && i < 15; i += 3)
            {
                int triangleIndex = counter.Increment() * 3;

                float3 vertex1 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 0]];
                float3 vertex2 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 1]];
                float3 vertex3 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 2]];

                float3 normal = math.normalize(math.cross(vertex2 - vertex1, vertex3 - vertex1));

                vertices[triangleIndex + 0] = new MarchingCubesVertexData(vertex1, normal);
                triangles[triangleIndex + 0] = (ushort)(triangleIndex + 0);

                vertices[triangleIndex + 1] = new MarchingCubesVertexData(vertex2, normal);
                triangles[triangleIndex + 1] = (ushort)(triangleIndex + 1);

                vertices[triangleIndex + 2] = new MarchingCubesVertexData(vertex3, normal);
                triangles[triangleIndex + 2] = (ushort)(triangleIndex + 2);
            }
        }
    }
}