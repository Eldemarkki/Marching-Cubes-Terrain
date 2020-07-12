using NUnit.Framework;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public class FloorToMultipleOfXTests
    {
        [TestCase(16f    , 16f    , 16f    , 4, 16, 16 , 16)]
        [TestCase(16.5f  , 16.5f  , 16.5f  , 4, 16, 16 , 16)]
        [TestCase(16f    , 16f    , 16f    , 3, 15, 15 , 15)]
        [TestCase(15.9f  , 6.1f   , 129.3f , 2, 14, 6  , 128)]
        public void Test_Floor_To_Multiple_Of_X(float nx, float ny, float nz, int x, int expectedX, int expectedY, int expectedZ)
        {
            Assert.AreEqual(new int3(expectedX, expectedY, expectedZ), VectorUtilities.FloorToMultipleOfX(new float3(nx, ny, nz), x));
        }
    }
}
