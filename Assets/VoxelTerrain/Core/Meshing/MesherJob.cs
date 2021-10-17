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
        VoxelDataVolume<byte> VoxelData { get; set; }
        VoxelDataVolume<Color32> VoxelColors { get; set; }

        NativeList<MeshingVertexData> OutputVertices { get; set; }
        NativeList<ushort> OutputTriangles { get; set; }
    }

    public interface IMesherJobChunk : IMesherJob, IJob { }
    public interface IMesherJobVoxel : IMesherJob, IJobParallelFor { }
}