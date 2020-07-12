using NUnit.Framework;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public class ToInt3Tests
    {
        [Test]
        public void TestToInt3ValueX([Random(0, 100, 4)] int x)
        {
            // Just some arbitrary values for y and z
            int y = 61;
            int z = 42;

            Vector3Int vector = new Vector3Int(x, y, z);
            Assert.AreEqual(vector.x, VectorUtilities.ToInt3(vector).x);
        }

        [Test]
        public void TestToInt3ValueY([Random(0, 100, 4)] int y)
        {
            // Just some arbitrary values for x and z
            int x = 33;
            int z = 98;

            Vector3Int vector = new Vector3Int(x, y, z);
            Assert.AreEqual(vector.y, VectorUtilities.ToInt3(vector).y);
        }

        [Test]
        public void TestToInt3ValueZ([Random(0, 100, 4)] int z)
        {
            // Just some arbitrary values for x and y
            int x = 48;
            int y = 86;

            Vector3Int vector = new Vector3Int(x, y, z);
            Assert.AreEqual(vector.z, VectorUtilities.ToInt3(vector).z);
        }
    }
}
