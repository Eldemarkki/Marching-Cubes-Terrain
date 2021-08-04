using Eldemarkki.VoxelTerrain.Meshing.Data;
using Eldemarkki.VoxelTerrain.VoxelData;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Meshing
{
    /// <summary>
    /// An interface for all the jobs that can extract a surface from voxel data
    /// </summary>
    public interface IMesherJob
    {
        /// <summary>
        /// The voxel data to generate the mesh from
        /// </summary>
        VoxelDataVolume<byte> VoxelData { get; set; }

        /// <summary>
        /// The voxel colors used to color the triangles
        /// </summary>
        VoxelDataVolume<Color32> VoxelColors { get; set; }

        /// <summary>
        /// The generated vertices
        /// </summary>
        NativeList<MeshingVertexData> OutputVertices { get; set; }

        /// <summary>
        /// The generated triangles
        /// </summary>
        NativeList<ushort> OutputTriangles { get; set; }
    }

    public interface IMesherJobChunk : IMesherJob, IJob { }
    public interface IMesherJobVoxel : IMesherJob, IJobParallelFor { }
}