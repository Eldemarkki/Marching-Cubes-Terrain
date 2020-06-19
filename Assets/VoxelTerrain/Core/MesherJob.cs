using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.MarchingCubes;
using Unity.Collections;
using Unity.Jobs;

public interface IMesherJob : IJobParallelFor
{
    DensityVolume VoxelData { get; set; }
    NativeArray<MarchingCubesVertexData> OutputVertices { get; set; }
    NativeArray<ushort> OutputTriangles { get; set; }
}
