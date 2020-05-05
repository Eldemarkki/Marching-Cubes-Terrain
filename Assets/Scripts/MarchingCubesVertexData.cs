using System.Runtime.InteropServices;
using Unity.Mathematics;

namespace MarchingCubes
{
    [StructLayout(LayoutKind.Sequential)]
    public struct MarchingCubesVertexData
    {
        public float3 position;
        public float3 normal;

        public MarchingCubesVertexData(float3 position, float3 normal)
        {
            this.position = position;
            this.normal = normal;
        }
    }
}