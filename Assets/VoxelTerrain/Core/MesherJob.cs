using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.MarchingCubes;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;

public interface IMesherJob : IJobParallelFor
{
    NativeCounter VertexCountCounter { get; set; }
    DensityVolume VoxelData { get; set; }
    NativeArray<MarchingCubesVertexData> OutputVertices { get; set; }
    NativeArray<ushort> OutputTriangles { get; set; }
}
