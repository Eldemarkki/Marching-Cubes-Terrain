using Eldemarkki.VoxelTerrain.Meshing.Data;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Meshing
{
    /// <summary>
    /// An interface for all the jobs that can extract a surface from voxel data
    /// </summary>
    public interface IMesherJob : IJob
    {
        /// <summary>
        /// A counter that keeps track of how many vertices there are
        /// </summary>
        NativeCounter VertexCountCounter { get; set; }

        /// <summary>
        /// The 3-dimensional size of <see cref="VoxelData"/>
        /// </summary>
        int3 VoxelDataDimensions { get; set; }

        /// <summary>
        /// The voxel data to generate the mesh from
        /// </summary>
        NativeArray<byte> VoxelData { get; set; }

        /// <summary>
        /// The voxel colors used to color the triangles
        /// </summary>
        NativeArray<Color32> VoxelColors { get; set; }

        /// <summary>
        /// The generated vertices
        /// </summary>
        NativeArray<MeshingVertexData> OutputVertices { get; set; }

        /// <summary>
        /// The generated triangles
        /// </summary>
        NativeArray<ushort> OutputTriangles { get; set; }
    }
}