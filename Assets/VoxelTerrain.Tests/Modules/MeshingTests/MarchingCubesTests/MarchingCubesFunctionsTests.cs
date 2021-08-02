﻿using NUnit.Framework;
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
                c1 / (float)byte.MaxValue,
                c2 / (float)byte.MaxValue,
                c3 / (float)byte.MaxValue,
                c4 / (float)byte.MaxValue,
                c5 / (float)byte.MaxValue,
                c6 / (float)byte.MaxValue,
                c7 / (float)byte.MaxValue,
                c8 / (float)byte.MaxValue
            };

            byte cubeIndex = MarchingCubesFunctions.CalculateCubeIndex(densities, isolevel / (float)byte.MaxValue);

            Assert.AreEqual(expectedCubeIndex, cubeIndex);
        }

        [TestCaseSource(nameof(VertexInterpolateTestCases))]
        public void VertexInterpolate_Test(int3 a, int3 b, float densityA, float densityB, float isolevel, float3 expectedPoint)
        {
            float3x3 actualPoint = MarchingCubesFunctions.VertexInterpolateTriangle(new int3x3(a, a, a), new int3x3(b, b, b), densityA, densityB, isolevel);

            Assert.AreEqual(0, math.distance(expectedPoint, actualPoint.c0), 0.00001f, $"Expected: {expectedPoint}, but was: {actualPoint}");
            Assert.AreEqual(0, math.distance(expectedPoint, actualPoint.c1), 0.00001f, $"Expected: {expectedPoint}, but was: {actualPoint}");
            Assert.AreEqual(0, math.distance(expectedPoint, actualPoint.c2), 0.00001f, $"Expected: {expectedPoint}, but was: {actualPoint}");
        }

        private static IEnumerable<TestCaseData> VertexInterpolateTestCases
        {
            get
            {
                yield return new TestCaseData(new int3(-1, -1, -1), new int3(1, 1, 1), 0, 1, 0.5f, new float3(0, 0, 0));
                yield return new TestCaseData(new int3(-2, -2, -2), new int3(1, 1, 1), 0, 1, 1f / 3, new float3(-1, -1, -1));
                yield return new TestCaseData(new int3(-10, -10, -10), new int3(-5, -5, -5), 0, 1, 0.5f, new float3(-7.5f, -7.5f, -7.5f));
                yield return new TestCaseData(new int3(-5, 0, 7), new int3(3, 2, -5), 0.25f, 0.6f, 0.3f, new float3(-3.857142857142f, 0.2857142857142857f, 5.2857142857142f));
            }
        }
    }
}
