using NUnit.Framework;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities.Intersection.Tests
{
    public class GetIntersectionVolumeTests
    {
        private static void TestIntersection(Vector3Int aPosition, Vector3Int aSize, Vector3Int bPosition, Vector3Int bSize, Vector3Int expectedPosition, Vector3Int expectedSize)
        {
            BoundsInt a = new BoundsInt(aPosition, aSize);
            BoundsInt b = new BoundsInt(bPosition, bSize);
            BoundsInt expected = new BoundsInt(expectedPosition, expectedSize);

            string message = $"Test failed with objects {a} and {b}. Expected {expected}";

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(expected, intersection, "Regular: " + message);

            BoundsInt intersectionSwapped = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(expected, intersectionSwapped, "Swapped: " + message);
        }

        [Test]
        public void Bounds_Are_Same_Should_Return_Same()
        {
            Vector3Int position = new Vector3Int(41, 24, 85);
            Vector3Int size = new Vector3Int(48, 26, 23);

            TestIntersection(position, size,
                             position, size,
                             position, size);
        }

        [Test]
        public void Test1()
        {
            TestIntersection(new Vector3Int(-2, -2, -2), new Vector3Int(4, 4, 4),
                             new Vector3Int(0, 0, 0), new Vector3Int(4, 4, 4),
                             new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2));
        }

        [Test]
        public void Test2()
        {
            TestIntersection(new Vector3Int(-3, -1, -1), new Vector3Int(5, 8, 4),
                             new Vector3Int(0, 4, 2), new Vector3Int(1, 5, 3),
                             new Vector3Int(0, 4, 2), new Vector3Int(1, 3, 1));
        }

        [Test]
        public void Test3()
        {
            TestIntersection(new Vector3Int(-12, -12, -12), new Vector3Int(4, 4, 4),
                             new Vector3Int(-8, -8, -8), new Vector3Int(16, 16, 16),
                             new Vector3Int(-8, -8, -8), new Vector3Int(0, 0, 0));
        }

        [Test]
        public void Test4()
        {
            TestIntersection(new Vector3Int(-12, -12, -12), new Vector3Int(4, 4, 4),
                             new Vector3Int(-9, -9, -9), new Vector3Int(16, 16, 16),
                             new Vector3Int(-9, -9, -9), new Vector3Int(1, 1, 1));
        }
    }
}
