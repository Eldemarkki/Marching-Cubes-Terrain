using NUnit.Framework;
using System.Linq;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.World
{
    public class LoadingCoordinatesTests
    {
        private static void TestLoadingCoordinatesExcept(LoadingCoordinates a, LoadingCoordinates b)
        {
            var expected = a.GetCoordinates().Except(b.GetCoordinates());
            var actual = a.GetCoordinatesExcept(b);

            Assert.That(actual, Is.EquivalentTo(expected));
        }

        [Test]
        public void LoadingCoordinatesExcept_1()
        {
            LoadingCoordinates a = new LoadingCoordinates(new int3(0, 0, 0), 1, 1);
            LoadingCoordinates b = new LoadingCoordinates(new int3(3, 3, 3), 1, 1);

            TestLoadingCoordinatesExcept(a, b);
        }

        [Test]
        public void LoadingCoordinatesExcept_2()
        {
            LoadingCoordinates a = new LoadingCoordinates(new int3(0, 2, 0), 1, 2);
            LoadingCoordinates b = new LoadingCoordinates(new int3(3, 3, 3), 3, 1);

            TestLoadingCoordinatesExcept(a, b);
        }

        [Test]
        public void LoadingCoordinatesExcept_3()
        {
            LoadingCoordinates a = new LoadingCoordinates(new int3(0, 2, 0), 1, 2);
            LoadingCoordinates b = new LoadingCoordinates(new int3(0, 2, 0), 1, 2);

            TestLoadingCoordinatesExcept(a, b);
        }
    }
}
