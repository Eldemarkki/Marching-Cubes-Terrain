using NUnit.Framework;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public class XyzToIndexTests
    {
        public void TestXyzToIndex_Using_int3(int3 xyz, int expectedIndex, int width, int height)
        {
            int actualIndex = IndexUtilities.XyzToIndex(xyz, width, height);
            Assert.AreEqual(expectedIndex, actualIndex);
        }

        public void TestXyzToIndex_Using_int_xyz(int x, int y, int z, int expectedIndex, int width, int height)
        {
            int actualIndex = IndexUtilities.XyzToIndex(x, y, z, width, height);
            Assert.AreEqual(expectedIndex, actualIndex);
        }

        [TestCaseSource(nameof(Width_1_Height_5_Cases))]
        public void Width_1_Height_5_Using_int3(int x, int y, int z, int expectedIndex)
        {
            TestXyzToIndex_Using_int3(new int3(x, y, z), expectedIndex, 1, 5);
        }

        [TestCaseSource(nameof(Width_2_Height_5_Cases))]
        public void Width_2_Height_5_Using_int3(int x, int y, int z, int expectedIndex)
        {
            TestXyzToIndex_Using_int3(new int3(x, y, z), expectedIndex, 2, 5);
        }

        [TestCaseSource(nameof(Width_1_Height_5_Cases))]
        public void Width_1_Height_5_Using_int_xyz(int x, int y, int z, int expectedIndex)
        {
            TestXyzToIndex_Using_int_xyz(x, y, z, expectedIndex, 1, 5);
        }

        [TestCaseSource(nameof(Width_2_Height_5_Cases))]
        public void Width_2_Height_5_Using_int_xyz(int x, int y, int z, int expectedIndex)
        {
            TestXyzToIndex_Using_int_xyz(x, y, z, expectedIndex, 2, 5);
        }

        static object[] Width_1_Height_5_Cases =
        {
            new object[] { 0, 1, 0, 1 },
            new object[] { 0, 2, 0, 2 },
            new object[] { 0, 2, 0, 2 },
            new object[] { 0, 3, 0, 3 },
            new object[] { 0, 4, 0, 4 },
            new object[] { 0, 0, 1, 5 }
        };

        static object[] Width_2_Height_5_Cases =
        {
            new object[] { 1, 0, 0, 1  },
            new object[] { 0, 1, 0, 2  },
            new object[] { 1, 1, 0, 3  },
            new object[] { 0, 2, 0, 4  },
            new object[] { 0, 0, 1, 10 },
            new object[] { 1, 0, 1, 11 },
            new object[] { 0, 1, 1, 12 },
            new object[] { 1, 1, 1, 13 },
            new object[] { 0, 0, 2, 20 },
            new object[] { 0, 0, 3, 30 },
            new object[] { 1, 1, 3, 33 }
        };
    }
}