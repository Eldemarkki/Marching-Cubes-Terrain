using NUnit.Framework;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public class ToVectorIntTests
    {
        [Test]
        public void TestToVectorIntValueX([Random(0, 100, 4)] int x)
        {
            // Just some arbitrary values for y and z
            int y = 61;
            int z = 42;

            int3 vector = new int3(x, y, z);
            Assert.AreEqual(vector.x, VectorUtilities.ToVectorInt(vector).x);
        }

        [Test]
        public void TestToVectorIntValueY([Random(0, 100, 4)] int y)
        {
            // Just some arbitrary values for x and z
            int x = 33;
            int z = 98;

            int3 vector = new int3(x, y, z);
            Assert.AreEqual(vector.y, VectorUtilities.ToVectorInt(vector).y);
        }

        [Test]
        public void TestToVectorIntValueZ([Random(0, 100, 4)] int z)
        {
            // Just some arbitrary values for x and y
            int x = 48;
            int y = 86;

            int3 vector = new int3(x, y, z);
            Assert.AreEqual(vector.z, VectorUtilities.ToVectorInt(vector).z);
        }
    }
}
