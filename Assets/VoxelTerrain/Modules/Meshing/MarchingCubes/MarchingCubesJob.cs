using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.VoxelData;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Meshing.MarchingCubes
{
    /// <summary>
    /// A marching cubes mesh generation job
    /// </summary>
    [BurstCompile]
    public struct MarchingCubesJob : IMesherJobChunk
    {
        /// <inheritdoc cref="VoxelData"/>
        [ReadOnly] private VoxelDataVolume<byte> _voxelData;

        /// <inheritdoc cref="VoxelColors"/>
        [ReadOnly] private VoxelDataVolume<Color32> _voxelColors;

        /// <summary>
        /// The density level where a surface will be created. Densities below this will be inside the surface (solid),
        /// and densities above this will be outside the surface (air)
        /// </summary>
        public float Isolevel { get; set; }

        /// <inheritdoc cref="OutputVertices"/>
        [NativeDisableParallelForRestriction] private NativeList<MeshingVertexData> _vertices;

        /// <inheritdoc cref="OutputTriangles"/>
        [NativeDisableParallelForRestriction] private NativeList<ushort> _triangles;

        /// <inheritdoc/>
        public VoxelDataVolume<byte> VoxelData { get => _voxelData; set => _voxelData = value; }

        /// <inheritdoc/>
        public VoxelDataVolume<Color32> VoxelColors { get => _voxelColors; set => _voxelColors = value; }

        /// <inheritdoc/>
        public NativeList<MeshingVertexData> OutputVertices { get => _vertices; set => _vertices = value; }

        /// <inheritdoc/>
        public NativeList<ushort> OutputTriangles { get => _triangles; set => _triangles = value; }

        /// <summary>
        /// The execute method required by the Unity Job System's IJob
        /// </summary>
        public unsafe void Execute()
        {
            int voxelCount = (_voxelData.Width - 1) * (_voxelData.Height - 1) * (_voxelData.Depth - 1);
            for (int index = 0; index < voxelCount; index++)
            {
                int3 voxelLocalPosition = IndexUtilities.IndexToXyz(index, _voxelData.Width - 1, _voxelData.Height - 1);

                float* densities = stackalloc float[8];
                for (int i = 0; i < 8; i++)
                {
                    int3 voxelCorner = voxelLocalPosition + LookupTables.CubeCorners[i];
                    densities[i] = _voxelData.GetVoxelData(voxelCorner) * MarchingCubesFunctions.ByteToFloat;
                }

                byte cubeIndex = MarchingCubesFunctions.CalculateCubeIndex(densities, Isolevel);
                if (cubeIndex == 0 || cubeIndex == byte.MaxValue)
                {
                    continue;
                }

                // Index at the beginning of the row
                int rowIndex = MarchingCubesLookupTables.TriangleTableAccessIndices[cubeIndex];

                int voxelTriangleCount = MarchingCubesLookupTables.TriangleTableWithLengths[rowIndex]; // First item in the row
                int rowStartIndex = rowIndex + 1; // Second index in the row;

                for (int i = 0; i < voxelTriangleCount; i++)
                {
                    float3x3 triangle = MarchingCubesFunctions.GetTriangle(rowStartIndex + i * 3, voxelLocalPosition, Isolevel, densities);

                    if (!triangle.c0.Equals(triangle.c1) && !triangle.c0.Equals(triangle.c2) && !triangle.c1.Equals(triangle.c2))
                    {
                        int originalVerticesLength = _vertices.Length;
                        ushort* indices = stackalloc ushort[3];
                        indices[0] = (ushort)originalVerticesLength;
                        indices[1] = (ushort)(originalVerticesLength + 1);
                        indices[2] = (ushort)(originalVerticesLength + 2);
                        _triangles.AddRange(indices, 3);

                        float3 normal = math.normalize(math.cross(triangle.c1 - triangle.c0, triangle.c2 - triangle.c0));
                        float3 triangleMiddlePoint = (triangle.c0 + triangle.c1 + triangle.c2) / 3f;

                        // Take the position of the closest corner of the current voxel
                        int3 colorSamplePoint = (int3)math.round(triangleMiddlePoint);
                        Color32 color = _voxelColors.GetVoxelData(colorSamplePoint);

                        MeshingVertexData* triangleVertices = stackalloc MeshingVertexData[3];
                        triangleVertices[0] = new MeshingVertexData(triangle.c0, normal, color);
                        triangleVertices[1] = new MeshingVertexData(triangle.c1, normal, color);
                        triangleVertices[2] = new MeshingVertexData(triangle.c2, normal, color);
                        _vertices.AddRange(triangleVertices, 3);
                    }
                }
            }
        }
    }
}