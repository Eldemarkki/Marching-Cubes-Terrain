using NUnit.Framework;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public class IndexToXyzTests
    {
        public void TestIndexToXyz(int index, int expectedX, int expectedY, int expectedZ, int width, int height)
        {
            Assert.AreEqual(new int3(expectedX, expectedY, expectedZ), IndexUtilities.IndexToXyz(index, width, height));
        }

        [TestCaseSource(nameof(Width_1_Height_5_Cases))]
        public void Width_1_Height_5(int index, int expectedX, int expectedY, int expectedZ)
        {
            TestIndexToXyz(index, expectedX, expectedY, expectedZ, 1, 5);
        }

        [TestCaseSource(nameof(Width_2_Height_5_Cases))]
        public void Width_2_Height_5(int index, int expectedX, int expectedY, int expectedZ)
        {
            TestIndexToXyz(index, expectedX, expectedY, expectedZ, 2, 5);
        }

        static object[] Width_1_Height_5_Cases =
        {
            new object[] { 1, 0, 1, 0},
            new object[] { 2, 0, 2, 0},
            new object[] { 2, 0, 2, 0},
            new object[] { 3, 0, 3, 0},
            new object[] { 4, 0, 4, 0},
            new object[] { 5, 0, 0, 1}
        };

        static object[] Width_2_Height_5_Cases =
        {
            new object[] { 1 , 1, 0, 0},
            new object[] { 2 , 0, 1, 0},
            new object[] { 3 , 1, 1, 0},
            new object[] { 4 , 0, 2, 0},
            new object[] { 10, 0, 0, 1},
            new object[] { 11, 1, 0, 1},
            new object[] { 12, 0, 1, 1},
            new object[] { 13, 1, 1, 1},
            new object[] { 20, 0, 0, 2},
            new object[] { 30, 0, 0, 3},
            new object[] { 33, 1, 1, 3}
        };
    }
}
