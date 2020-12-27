using Eldemarkki.VoxelTerrain.Meshing.Data;
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
        public void Execute()
        {
            byte isolevelByte = (byte)math.clamp(Isolevel * 255, 0, 255);
            for (int x = 0; x < VoxelData.Width - 1; x++)
            {
                for (int y = 0; y < VoxelData.Height - 1; y++)
                {
                    for (int z = 0; z < VoxelData.Depth - 1; z++)
                    {
                        int3 voxelLocalPosition = new int3(x, y, z);

                        VoxelCorners<byte> densities = _voxelData.GetVoxelDataUnitCube(voxelLocalPosition);

                        byte cubeIndex = MarchingCubesFunctions.CalculateCubeIndex(densities, isolevelByte);
                        if (cubeIndex == 0 || cubeIndex == 255)
                        {
                            continue;
                        }

                        int edgeIndex = MarchingCubesLookupTables.EdgeTable[cubeIndex];

                        VertexList vertexList = MarchingCubesFunctions.GenerateVertexList(densities, voxelLocalPosition, edgeIndex, isolevelByte);

                        // Index at the beginning of the row
                        int rowIndex = 15 * cubeIndex;

                        for (int i = 0; MarchingCubesLookupTables.TriangleTable[rowIndex + i] != -1 && i < 15; i += 3)
                        {
                            float3 vertex1 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 0]];
                            float3 vertex2 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 1]];
                            float3 vertex3 = vertexList[MarchingCubesLookupTables.TriangleTable[rowIndex + i + 2]];

                            if (!vertex1.Equals(vertex2) && !vertex1.Equals(vertex3) && !vertex2.Equals(vertex3))
                            {
                                float3 normal = math.normalize(math.cross(vertex2 - vertex1, vertex3 - vertex1));

                                int triangleIndex = VertexCountCounter.Increment() * 3;

                                float3 triangleMiddlePoint = (vertex1 + vertex2 + vertex3) / 3f;

                                // Take the position of the closest corner of the current voxel
                                int3 colorSamplePoint = (int3)math.round(triangleMiddlePoint);
                                Color32 color = VoxelColors.GetVoxelData(colorSamplePoint);

                                _vertices[triangleIndex + 0] = new MeshingVertexData(vertex1, normal, color);
                                _triangles[triangleIndex + 0] = (ushort)(triangleIndex + 0);

                                _vertices[triangleIndex + 1] = new MeshingVertexData(vertex2, normal, color);
                                _triangles[triangleIndex + 1] = (ushort)(triangleIndex + 1);

                                _vertices[triangleIndex + 2] = new MeshingVertexData(vertex3, normal, color);
                                _triangles[triangleIndex + 2] = (ushort)(triangleIndex + 2);
                            }
                        }
                    }
                }
            }
        }
    }
}