using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.VoxelData;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Meshing.MarchingCubes
{
    /// <summary>
    /// A marching cubes mesh generation job
    /// </summary>
    [BurstCompile]
    public struct MarchingCubesJob : IMesherJob
    {
        /// <summary>
        /// The densities to generate the mesh off of
        /// </summary>
        [ReadOnly] private VoxelDataVolume _voxelData;

        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        public float Isolevel { get; set; }

        /// <summary>
        /// The counter to keep track of the triangle index
        /// </summary>
        public NativeCounter VertexCountCounter { get; set; }

        /// <summary>
        /// The generated vertices
        /// </summary>
        [NativeDisableParallelForRestriction, WriteOnly] private NativeArray<MeshingVertexData> _vertices;

        /// <summary>
        /// The generated triangles
        /// </summary>
        [NativeDisableParallelForRestriction, WriteOnly] private NativeArray<ushort> _triangles;

        /// <summary>
        /// The voxel data to generate the mesh from
        /// </summary>
        public VoxelDataVolume VoxelData { get => _voxelData; set => _voxelData = value; }

        /// <summary>
        /// The generated vertices
        /// </summary>
        public NativeArray<MeshingVertexData> OutputVertices { get => _vertices; set => _vertices = value; }

        /// <summary>
        /// The generated triangles
        /// </summary>
        public NativeArray<ushort> OutputTriangles { get => _triangles; set => _triangles = value; }

        /// <summary>
        /// The execute method required by the Unity Job System's IJobParallelFor
        /// </summary>
        /// <param name="index">The iteration index</param>
        public void Execute(int index)
        {
            // The position of the voxel Voxel inside the chunk. Goes from (0, 0, 0) to (densityVolume.Width-1, densityVolume.Height-1, densityVolume.Depth-1). Both are inclusive.
            int3 voxelLocalPosition = IndexUtilities.IndexToXyz(index, _voxelData.Width - 1, _voxelData.Height - 1);

            VoxelCorners<float> densities = _voxelData.GetVoxelDataUnitCube(voxelLocalPosition);

            byte cubeIndex = MarchingCubesFunctions.CalculateCubeIndex(densities, Isolevel);
            if (cubeIndex == 0 || cubeIndex == 255)
            {
                return;
            }

            VoxelCorners<int3> corners = MarchingCubesFunctions.GetCorners(voxelLocalPosition);

            int edgeIndex = MarchingCubesLookupTables.EdgeTable[cubeIndex];

            VertexList vertexList = MarchingCubesFunctions.GenerateVertexList(densities, corners, edgeIndex, Isolevel);

            // Index at the beginning of the row
            int rowIndex = 15 * cubeIndex;

            for (int i = 0; MarchingCubesLookupTables.TriangleTable[rowIndex+i] != -1 && i < 15; i += 3)
            {
                float3 vertex1 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 0]];
                float3 vertex2 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 1]];
                float3 vertex3 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 2]];

                if (!vertex1.Equals(vertex2) && !vertex1.Equals(vertex3) && !vertex2.Equals(vertex3))
                {
                    float3 normal = math.normalize(math.cross(vertex2 - vertex1, vertex3 - vertex1));

                    int triangleIndex = VertexCountCounter.Increment() * 3;
                    
                    _vertices[triangleIndex + 0] = new MeshingVertexData(vertex1, normal);
                    _triangles[triangleIndex + 0] = (ushort)(triangleIndex + 0);

                    _vertices[triangleIndex + 1] = new MeshingVertexData(vertex2, normal);
                    _triangles[triangleIndex + 1] = (ushort)(triangleIndex + 1);

                    _vertices[triangleIndex + 2] = new MeshingVertexData(vertex3, normal);
                    _triangles[triangleIndex + 2] = (ushort)(triangleIndex + 2);
                }
            }
        }
    }
}