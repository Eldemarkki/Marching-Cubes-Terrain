using Eldemarkki.VoxelTerrain.Meshing.Data;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
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
        public void CalculateCubeIndex_Test(byte c1, byte c2, byte c3, byte c4, byte c5, byte c6, byte c7, byte c8, byte isolevel, byte expectedCubeIndex)
        {
            VoxelCorners<byte> densities = new VoxelCorners<byte>
            {
                Corner1 = c1,
                Corner2 = c2,
                Corner3 = c3,
                Corner4 = c4,
                Corner5 = c5,
                Corner6 = c6,
                Corner7 = c7,
                Corner8 = c8
            };

            byte cubeIndex = MarchingCubesFunctions.CalculateCubeIndex(densities, isolevel);

            Assert.AreEqual(expectedCubeIndex, cubeIndex);
        }

        [TestCaseSource(nameof(VertexInterpolateTestCases))]
        public void VertexInterpolate_Test(float3 a, float3 b, float densityA, float densityB, float isolevel, float3 expectedPoint)
        {
            float3 actualPoint = MarchingCubesFunctions.VertexInterpolate(a, b, densityA, densityB, isolevel);

            Assert.AreEqual(0, math.distance(expectedPoint, actualPoint), 0.00001f, $"Expected: {expectedPoint}, but was: {actualPoint}");
        }

        [Test]
        public void GenerateVertexList_Test1()
        {
            // Arrange
            VoxelCorners<byte> densities = new VoxelCorners<byte>
            {
                Corner1 = 255,
                Corner2 = 255,
                Corner3 = 255,
                Corner4 = 0,
                Corner5 = 255,
                Corner6 = 255,
                Corner7 = 255,
                Corner8 = 255
            };

            int edgeIndex = 0b1000_0000_1100;
            byte isolevel = 127;

            VertexList expected = new VertexList();
            expected[2] = new float3(0.5f, 0f, 1f);
            expected[3] = new float3(0f, 0f, 0.5f);
            expected[11] = new float3(0f, 0.5f, 1f);

            // Act
            IEnumerable<float3> actual = MarchingCubesFunctions.GenerateVertexList(densities, new int3(0, 0, 0), edgeIndex, isolevel);

            // Assert
            for (int i = 0; i < 12; i++)
            {
                var actualPosition = actual.ElementAt(i);
                Assert.AreEqual(0, math.distance(expected[i], actualPosition), 0.011f, $"Expected: {expected[i]}, Actual: {actualPosition}");
            }
        }

        [Test]
        public void GenerateVertexList_Test2()
        {
            // Arrange
            VoxelCorners<byte> densities = new VoxelCorners<byte>
            {
                Corner1 = 255,
                Corner2 = 0,
                Corner3 = 255,
                Corner4 = 255,
                Corner5 = 255,
                Corner6 = 255,
                Corner7 = 0,
                Corner8 = 0
            };

            byte isolevel = 191;
            int edgeIndex = 0b1110_1010_0011;

            VertexList expected = new VertexList();
            expected[0] = new float3(6.25f, -13f, 100f);
            expected[1] = new float3(7f, -13f, 100.75f);
            expected[5] = new float3(7f, -12f, 100.25f);
            expected[7] = new float3(6f, -12f, 100.25f);
            expected[9] = new float3(7f, -12.25f, 100f);
            expected[10] = new float3(7f, -12.75f, 101f);
            expected[11] = new float3(6f, -12.75f, 101f);

            // Act
            IEnumerable<float3> actual = MarchingCubesFunctions.GenerateVertexList(densities, new int3(6, -13, 100), edgeIndex, isolevel);

            // Assert
            for (int i = 0; i < 12; i++)
            {
                var actualPosition = actual.ElementAt(i);
                Assert.AreEqual(0, math.distance(expected[i], actualPosition), 0.011f, $"Expected: {expected[i]}, Actual: {actualPosition}");
            }
        }

        [Test]
        public void GenerateVertexList_Test3()
        {
            // Arrange
            VoxelCorners<byte> densities = new VoxelCorners<byte>
            {
                Corner1 = 140,
                Corner2 = 25,
                Corner3 = 189,
                Corner4 = 89,
                Corner5 = 192,
                Corner6 = 204,
                Corner7 = 255,
                Corner8 = 229
            };

            byte isolevel = 191;
            int edgeIndex = 0b1111_0000_0000;

            VertexList expected = new VertexList();
            expected[8] = new float3(-56f, -0.02439022f, 9f);
            expected[9] = new float3(-55f, -0.07142866f, 9f);
            expected[10] = new float3(-55f, -0.9803922f, 10f);
            expected[11] = new float3(-56f, -0.272727272727f, 10f);

            // Act
            IEnumerable<float3> actual = MarchingCubesFunctions.GenerateVertexList(densities, new int3(-56, -1, 9), edgeIndex, isolevel);

            // Assert
            for (int i = 0; i < 12; i++)
            {
                var actualPosition = actual.ElementAt(i);
                Assert.AreEqual(0, math.distance(expected[i], actualPosition), 0.011f, $"Expected: {expected[i]}, Actual: {actualPosition}");
            }
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
