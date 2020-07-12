using NUnit.Framework;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public class ModTests
    {
        [TestCase(10, 16, 74, 5, 0, 1, 4)]
        [TestCase(84, -24, 0, 8, 4, 0, 0)]
        [TestCase(-85, -8, 14, 3, 2, 1, 2 )]
        [TestCase(20, 35, 71, 35, 20, 0, 1)]
        public void TestMod(int nx, int ny, int nz, int x, int expectedX, int expectedY, int expectedZ)
        {
            Assert.AreEqual(new int3(expectedX, expectedY, expectedZ), VectorUtilities.Mod(new int3(nx, ny, nz), x));
        }
    }
}
