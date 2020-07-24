using NUnit.Framework;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities.Intersection.Tests
{
    public class GetIntersectionVolumeTests
    {
        [Test]
        public void Bounds_Are_Same_Should_Return_Same()
        {
            Bounds a = new Bounds(new Vector3(41, 24, 85), new Vector3(48, 26, 23));
            Bounds b = a;

            Bounds intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(a, intersection);
        }

        [Test]
        public void Bounds_Are_Same_Swapped_Should_Return_Same()
        {
            Bounds a = new Bounds(new Vector3(41, 24, 85), new Vector3(48, 26, 23));
            Bounds b = a;

            Bounds intersection = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(a, intersection);
        }

        [Test]
        public void Test1()
        {
            Bounds a = new Bounds(new Vector3(0, 0, 0), new Vector3(4, 4, 4));
            Bounds b = new Bounds(new Vector3(2, 2, 2), new Vector3(4, 4, 4));

            Bounds intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(new Bounds(new Vector3(1, 1, 1), new Vector3(2, 2, 2)), intersection);
        }

        [Test]
        public void Test1_Swapped()
        {
            Bounds a = new Bounds(new Vector3(0, 0, 0), new Vector3(4, 4, 4));
            Bounds b = new Bounds(new Vector3(2, 2, 2), new Vector3(4, 4, 4));

            Bounds intersection = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(new Bounds(new Vector3(1, 1, 1), new Vector3(2, 2, 2)), intersection);
        }

        [Test]
        public void Test2()
        {
            Bounds a = new Bounds(new Vector3(0, 0, 0), new Vector3(2, 3, 3));
            Bounds b = new Bounds(new Vector3(2, 0, 0), new Vector3(2, 3, 3));

            Bounds intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(new Bounds(new Vector3(1, 0, 0), new Vector3(0, 3, 3)), intersection);
        }

        [Test]
        public void Test2_Swapped()
        {
            Bounds a = new Bounds(new Vector3(0, 0, 0), new Vector3(2, 3, 3));
            Bounds b = new Bounds(new Vector3(2, 0, 0), new Vector3(2, 3, 3));

            Bounds intersection = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(new Bounds(new Vector3(1, 0, 0), new Vector3(0, 3, 3)), intersection);
        }

        [Test]
        public void Test3()
        {
            Bounds a = new Bounds(new Vector3(-10, -10, -10), new Vector3(4, 4, 4));
            Bounds b = new Bounds(new Vector3(0, 0, 0), new Vector3(16, 16, 16));

            Bounds intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(new Bounds(new Vector3(-8, -8, -8), new Vector3(0, 0, 0)), intersection);
        }

        [Test]
        public void Test3_Swapped()
        {
            Bounds a = new Bounds(new Vector3(-10, -10, -10), new Vector3(4, 4, 4));
            Bounds b = new Bounds(new Vector3(0, 0, 0), new Vector3(16, 16, 16));

            Bounds intersection = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(new Bounds(new Vector3(-8, -8, -8), new Vector3(0, 0, 0)), intersection);
        }

        [Test]
        public void Test4()
        {
            Bounds a = new Bounds(new Vector3(-10, -10, -10), new Vector3(4, 4, 4));
            Bounds b = new Bounds(new Vector3(-1, -1, -1), new Vector3(16, 16, 16));

            Bounds intersection = IntersectionUtilities.GetIntersectionVolume(a, b);
            Assert.AreEqual(new Bounds(new Vector3(-8.5f, -8.5f, -8.5f), new Vector3(1, 1, 1)), intersection);
        }


        [Test]
        public void Test4_Swapped()
        {
            Bounds a = new Bounds(new Vector3(-10, -10, -10), new Vector3(4, 4, 4));
            Bounds b = new Bounds(new Vector3(-1, -1, -1), new Vector3(16, 16, 16));

            Bounds intersection = IntersectionUtilities.GetIntersectionVolume(b, a);
            Assert.AreEqual(new Bounds(new Vector3(-8.5f, -8.5f, -8.5f), new Vector3(1, 1, 1)), intersection);
        }
    }
}
