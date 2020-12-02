using Eldemarkki.VoxelTerrain.Meshing.Data;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Meshing.MarchingCubes.Tests
{
    public class MarchingCubesFunctionsTests
    {
        [TestCase(1, 1, 1, 1, 1, 1, 1, 1, 0.5f, 0)]
        [TestCase(0, 1, 1, 1, 1, 1, 1, 1, 0.5f, 1)]
        [TestCase(1, 0, 1, 1, 1, 1, 1, 1, 0.5f, 2)]
        [TestCase(1, 1, 0, 1, 1, 1, 1, 1, 0.5f, 4)]
        [TestCase(1, 1, 1, 0, 1, 1, 1, 1, 0.5f, 8)]
        [TestCase(1, 1, 1, 1, 0, 1, 1, 1, 0.5f, 16)]
        [TestCase(1, 1, 1, 1, 1, 0, 1, 1, 0.5f, 32)]
        [TestCase(1, 1, 1, 1, 1, 1, 0, 1, 0.5f, 64)]
        [TestCase(1, 1, 1, 1, 1, 1, 1, 0, 0.5f, 128)]
        [TestCase(0, 0, 0, 0, 0, 0, 0, 0, 0.5f, 255)]
        [TestCase(0, 1, 1, 0, 1, 1, 1, 1, 0.5f, 9)]
        [TestCase(0, 1, 1, 0, 1, 1, 1, 1, 0.9f, 9)]
        [TestCase(0, 1, 0.5f, 0, 1, 1, 1, 1, 0.6f, 13)]
        [TestCase(0.7f, 0.7f, 0.7f, 0.7f, 0.8f, 0.8f, 0.5f, 0.75f, 0.77f, 207)]
        [TestCase(0.3f, 0.45f, 0.9f, 0.4f, 0.5f, 0.55f, 0.75f, 0.625f, 0.55f, 27)]
        public void CalculateCubeIndex_Test(float c1, float c2, float c3, float c4, float c5, float c6, float c7, float c8, float isolevel, int expectedCubeIndex)
        {
            VoxelCorners<float> densities = new VoxelCorners<float>
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

            int cubeIndex = MarchingCubesFunctions.CalculateCubeIndex(densities, isolevel);

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
            VoxelCorners<float> densities = new VoxelCorners<float>
            {
                Corner1 = 1f,
                Corner2 = 1f,
                Corner3 = 1f,
                Corner4 = 0f,
                Corner5 = 1f,
                Corner6 = 1f,
                Corner7 = 1f,
                Corner8 = 1f
            };

            int edgeIndex = 0b1000_0000_1100;
            float isolevel = 0.5f;

            VertexList expected = new VertexList();
            expected[2] = new float3(0.5f, 0f, 1f);
            expected[3] = new float3(0f, 0f, 0.5f);
            expected[11] = new float3(0f, 0.5f, 1f);

            // Act
            IEnumerable<float3> actual = MarchingCubesFunctions.GenerateVertexList(densities, new int3(0, 0, 0), edgeIndex, isolevel);

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void GenerateVertexList_Test2()
        {
            // Arrange
            VoxelCorners<float> densities = new VoxelCorners<float>
            {
                Corner1 = 1f,
                Corner2 = 0f,
                Corner3 = 1f,
                Corner4 = 1f,
                Corner5 = 1f,
                Corner6 = 1f,
                Corner7 = 0f,
                Corner8 = 0f
            };

            float isolevel = 0.75f;
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
            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void GenerateVertexList_Test3()
        {
            // Arrange
            VoxelCorners<float> densities = new VoxelCorners<float>
            {
                Corner1 = 0.55f,
                Corner2 = 0.1f,
                Corner3 = 0.745f,
                Corner4 = 0.35f,
                Corner5 = 0.755f,
                Corner6 = 0.8f,
                Corner7 = 1f,
                Corner8 = 0.9f
            };

            float isolevel = 0.75f;
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
                Assert.AreEqual(0, math.distance(expected[i], actualPosition), 0.0001f, $"Expected: {expected[i]}, Actual: {actualPosition}");
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
