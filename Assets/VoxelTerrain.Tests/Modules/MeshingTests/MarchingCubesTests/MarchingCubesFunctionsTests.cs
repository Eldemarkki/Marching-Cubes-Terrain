using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Meshing.MarchingCubes.Tests
{
    public class MarchingCubesFunctionsTests
    {
        [TestCase(255, 255, 255, 255, 255, 255, 255, 255, 127, 0)]
        [TestCase(0, 255, 255, 255, 255, 255, 255, 255, 127, 1)]
        [TestCase(255, 0, 255, 255, 255, 255, 255, 255, 127, 2)]
        [TestCase(255, 255, 0, 255, 255, 255, 255, 255, 127, 4)]
        [TestCase(255, 255, 255, 0, 255, 255, 255, 255, 127, 8)]
        [TestCase(255, 255, 255, 255, 0, 255, 255, 255, 127, 16)]
        [TestCase(255, 255, 255, 255, 255, 0, 255, 255, 127, 32)]
        [TestCase(255, 255, 255, 255, 255, 255, 0, 255, 127, 64)]
        [TestCase(255, 255, 255, 255, 255, 255, 255, 0, 127, 128)]
        [TestCase(0, 0, 0, 0, 0, 0, 0, 0, 127, 255)]
        [TestCase(0, 255, 255, 0, 255, 255, 255, 255, 127, 9)]
        [TestCase(0, 255, 255, 0, 255, 255, 255, 255, 229, 9)]
        [TestCase(0, 255, 127, 0, 255, 255, 255, 255, 153, 13)]
        [TestCase(178, 178, 178, 178, 204, 204, 127, 191, 196, 207)]
        [TestCase(76, 114, 229, 102, 127, 140, 191, 159, 140, 27)]
        public unsafe void CalculateCubeIndex_Test(byte c1, byte c2, byte c3, byte c4, byte c5, byte c6, byte c7, byte c8, byte isolevel, byte expectedCubeIndex)
        {
            float* densities = stackalloc float[8]
            {
                c1 / 255f,
                c2 / 255f,
                c3 / 255f,
                c4 / 255f,
                c5 / 255f,
                c6 / 255f,
                c7 / 255f,
                c8 / 255f
            };

            byte cubeIndex = MarchingCubesFunctions.CalculateCubeIndex(densities, isolevel / 255f);

            Assert.AreEqual(expectedCubeIndex, cubeIndex);
        }

        [TestCaseSource(nameof(VertexInterpolateTestCases))]
        public void VertexInterpolate_Test(float3 a, float3 b, float densityA, float densityB, float isolevel, float3 expectedPoint)
        {
            float3 actualPoint = MarchingCubesFunctions.VertexInterpolate(a, b, densityA, densityB, isolevel);

            Assert.AreEqual(0, math.distance(expectedPoint, actualPoint), 0.00001f, $"Expected: {expectedPoint}, but was: {actualPoint}");
        }

        private static IEnumerable<TestCaseData> VertexInterpolateTestCases
        {
            get
            {
                yield return new TestCaseData(new float3(-1, -1, -1), new float3(1, 1, 1), 0, 1, 0.5f, new float3(0, 0, 0));
                yield return new TestCaseData(new float3(-2, -2, -2), new float3(1, 1, 1), 0, 1, 1f / 3, new float3(-1, -1, -1));
                yield return new TestCaseData(new float3(-10, -10, -10), new float3(-5, -5, -5), 0, 1, 0.5f, new float3(-7.5f, -7.5f, -7.5f));
                yield return new TestCaseData(new float3(-5, 0, 7), new float3(3, 2, -5), 0.25f, 0.6f, 0.3f, new float3(-3.857142857142f, 0.2857142857142857f, 5.2857142857142f));
            }
        }
    }
}
