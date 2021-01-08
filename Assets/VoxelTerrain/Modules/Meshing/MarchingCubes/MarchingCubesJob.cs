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
    public struct MarchingCubesJob : IMesherJob
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

        /// <inheritdoc/>
        public NativeCounter VertexCountCounter { get; set; }

        /// <inheritdoc cref="OutputVertices"/>
        [NativeDisableParallelForRestriction, WriteOnly] private NativeArray<MeshingVertexData> _vertices;

        /// <inheritdoc cref="OutputTriangles"/>
        [NativeDisableParallelForRestriction, WriteOnly] private NativeArray<ushort> _triangles;

        /// <inheritdoc/>
        public VoxelDataVolume<byte> VoxelData { get => _voxelData; set => _voxelData = value; }

        /// <inheritdoc/>
        public VoxelDataVolume<Color32> VoxelColors { get => _voxelColors; set => _voxelColors = value; }

        /// <inheritdoc/>
        public NativeArray<MeshingVertexData> OutputVertices { get => _vertices; set => _vertices = value; }

        /// <inheritdoc/>
        public NativeArray<ushort> OutputTriangles { get => _triangles; set => _triangles = value; }

        /// <summary>
        /// The execute method required by the Unity Job System's IJob
        /// </summary>
        public unsafe void Execute(int index)
        {
            int3 voxelLocalPosition = IndexUtilities.IndexToXyz(index, _voxelData.Width - 1, _voxelData.Height - 1);

            float* densities = stackalloc float[8];
            for (int i = 0; i < 8; i++)
            {
                int3 voxelCorner = voxelLocalPosition + LookupTables.CubeCorners[i];
                densities[i] = _voxelData.GetVoxelData(voxelCorner) * MarchingCubesFunctions.ByteToFloat;
            }

            byte cubeIndex = MarchingCubesFunctions.CalculateCubeIndex(densities, Isolevel);
            if (cubeIndex == 0 || cubeIndex == 255)
            {
                return;
            }

            // Index at the beginning of the row
            int rowIndex = MarchingCubesLookupTables.TriangleTableAccessIndices[cubeIndex];

            int rowLength = MarchingCubesLookupTables.TriangleTableWithLengths[rowIndex]; // First item in the row
            int rowStartIndex = rowIndex + 1; // Second index in the row;

            // Increment it before the for loop to reduce the 'lock' operations which slow down the execution.
            // This in a way "reserves" the next 'rowLength' vertices for this thread.
            int triangleIndex = VertexCountCounter.Add(rowLength / 3) * 3;

            for (int i = 0; i < rowLength; i += 3)
            {
                float3 vertex1 = MarchingCubesFunctions.GetVertex(rowStartIndex + i + 0, voxelLocalPosition, Isolevel, densities);
                float3 vertex2 = MarchingCubesFunctions.GetVertex(rowStartIndex + i + 1, voxelLocalPosition, Isolevel, densities);
                float3 vertex3 = MarchingCubesFunctions.GetVertex(rowStartIndex + i + 2, voxelLocalPosition, Isolevel, densities);

                if (!vertex1.Equals(vertex2) && !vertex1.Equals(vertex3) && !vertex2.Equals(vertex3))
                {
                    float3 normal = math.normalize(math.cross(vertex2 - vertex1, vertex3 - vertex1));

                    float3 triangleMiddlePoint = (vertex1 + vertex2 + vertex3) / 3f;

                    // Take the position of the closest corner of the current voxel
                    int3 colorSamplePoint = (int3)math.round(triangleMiddlePoint);
                    Color32 color = _voxelColors.GetVoxelData(colorSamplePoint);

                    _vertices[triangleIndex + 0] = new MeshingVertexData(vertex1, normal, color);
                    _triangles[triangleIndex + 0] = (ushort)(triangleIndex + 0);

                    _vertices[triangleIndex + 1] = new MeshingVertexData(vertex2, normal, color);
                    _triangles[triangleIndex + 1] = (ushort)(triangleIndex + 1);

                    _vertices[triangleIndex + 2] = new MeshingVertexData(vertex3, normal, color);
                    _triangles[triangleIndex + 2] = (ushort)(triangleIndex + 2);

                    triangleIndex += 3;
                }
            }
        }
    }
}