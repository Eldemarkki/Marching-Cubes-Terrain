using NUnit.Framework;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities.Intersection.Tests
{
    public class GetIntersectionVolumeTests
    {
        [Test]
        public void Bounds_Are_Same_Should_Return_Same()
        {
            BoundsInt a = new BoundsInt(new Vector3Int(41, 24, 85), new Vector3Int(48, 26, 23));
            BoundsInt b = a;

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(a, intersection);
        }

        [Test]
        public void Bounds_Are_Same_Swapped_Should_Return_Same()
        {
            BoundsInt a = new BoundsInt(new Vector3Int(41, 24, 85), new Vector3Int(48, 26, 23));
            BoundsInt b = a;

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(a, intersection);
        }

        [Test]
        public void Test1()
        {
            BoundsInt a = new BoundsInt(new Vector3Int(-2, -2, -2), new Vector3Int(4, 4, 4));
            BoundsInt b = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(4, 4, 4));

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2)), intersection);
        }

        [Test]
        public void Test1_Swapped()
        {
            BoundsInt a = new BoundsInt(new Vector3Int(-2, -2, -2), new Vector3Int(4, 4, 4));
            BoundsInt b = new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(4, 4, 4));
            
            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(new BoundsInt(new Vector3Int(0, 0, 0), new Vector3Int(2, 2, 2)), intersection);
        }

        [Test]
        public void Test2()
        {
            BoundsInt a = new BoundsInt(new Vector3Int(-3, -1, -1), new Vector3Int(5, 8, 4));
            BoundsInt b = new BoundsInt(new Vector3Int(0, 4, 2), new Vector3Int(1, 5, 3));

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(new BoundsInt(new Vector3Int(0, 4, 2), new Vector3Int(1, 3, 1)), intersection);
        }

        [Test]
        public void Test2_Swapped()
        {
            BoundsInt a = new BoundsInt(new Vector3Int(-3, -1, -1), new Vector3Int(5, 8, 4));
            BoundsInt b = new BoundsInt(new Vector3Int(0, 4, 2), new Vector3Int(1, 5, 3));

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(new BoundsInt(new Vector3Int(0, 4, 2), new Vector3Int(1, 3, 1)), intersection);
        }

        [Test]
        public void Test3()
        {
            BoundsInt a = new BoundsInt(new Vector3Int(-12, -12, -12), new Vector3Int(4, 4, 4));
            BoundsInt b = new BoundsInt(new Vector3Int(-8, -8, -8), new Vector3Int(16, 16, 16));

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(new BoundsInt(new Vector3Int(-8, -8, -8), new Vector3Int(0, 0, 0)), intersection);
        }

        [Test]
        public void Test3_Swapped()
        {
            BoundsInt a = new BoundsInt(new Vector3Int(-12, -12, -12), new Vector3Int(4, 4, 4));
            BoundsInt b = new BoundsInt(new Vector3Int(-8, -8, -8), new Vector3Int(16, 16, 16));

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(new BoundsInt(new Vector3Int(-8, -8, -8), new Vector3Int(0, 0, 0)), intersection);
        }

        [Test]
        public void Test4()
        {
            BoundsInt a = new BoundsInt(new Vector3Int(-12, -12, -12), new Vector3Int(4, 4, 4));
            BoundsInt b = new BoundsInt(new Vector3Int(-9, -9, -9), new Vector3Int(16, 16, 16));

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(new BoundsInt(new Vector3Int(-9, -9, -9), new Vector3Int(1, 1, 1)), intersection);
        }

        [Test]
        public void Test4_Swapped()
        {
            BoundsInt a = new BoundsInt(new Vector3Int(-12, -12, -12), new Vector3Int(4, 4, 4));
            BoundsInt b = new BoundsInt(new Vector3Int(-9, -9, -9), new Vector3Int(16, 16, 16));

            BoundsInt intersection = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(new BoundsInt(new Vector3Int(-9, -9, -9), new Vector3Int(1, 1, 1)), intersection);
        }
    }
}
