using NUnit.Framework;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities.Tests
{
    public class CoordinateUtilitiesTests
    {
        [Test]
        public void GetChunkCoordinatesContainingPoint_Test1()
        {
            int3 worldPosition = new int3(0, 0, 0);
            int chunkSize = 16;

            IEnumerable<int3> coordinates = CoordinateUtilities.GetChunkCoordinatesContainingPoint(worldPosition, chunkSize);

            IEnumerable<int3> expectedCoordinates = new List<int3>
            {
                new int3( 0,  0,  0),
                new int3(-1,  0,  0),
                new int3( 0, -1,  0),
                new int3(-1, -1,  0),
                new int3( 0,  0, -1),
                new int3(-1,  0, -1),
                new int3( 0, -1, -1),
                new int3(-1, -1, -1),
            };

            Assert.That(coordinates, Is.EquivalentTo(expectedCoordinates));
        }

        [Test]
        public void GetChunkCoordinatesContainingPoint_Test2()
        {
            int3 worldPosition = new int3(16, 0, 16);
            int chunkSize = 16;

            IEnumerable<int3> coordinates = CoordinateUtilities.GetChunkCoordinatesContainingPoint(worldPosition, chunkSize);

            IEnumerable<int3> expectedCoordinates = new List<int3>
            {
                new int3( 1,  0, 1),
                new int3( 0,  0, 1),
                new int3( 1, -1, 1),
                new int3( 0, -1, 1),
                new int3( 1,  0, 0),
                new int3( 0,  0, 0),
                new int3( 1, -1, 0),
                new int3( 0, -1, 0),
            };

            Assert.That(coordinates, Is.EquivalentTo(expectedCoordinates));
        }

        [Test]
        public void GetChunkCoordinatesContainingPoint_Test3()
        {
            int3 worldPosition = new int3(16, 1, 16);
            int chunkSize = 16;

            IEnumerable<int3> coordinates = CoordinateUtilities.GetChunkCoordinatesContainingPoint(worldPosition, chunkSize);

            IEnumerable<int3> expectedCoordinates = new List<int3>
            {
                new int3( 1,  0, 1),
                new int3( 0,  0, 1),
                new int3( 1,  0, 0),
                new int3( 0,  0, 0),
            };

            Assert.That(coordinates, Is.EquivalentTo(expectedCoordinates));
        }

        [Test]
        public void GetChunkCoordinatesContainingPoint_Test4()
        {
            int3 worldPosition = new int3(5, 3, 16);
            int chunkSize = 16;

            IEnumerable<int3> coordinates = CoordinateUtilities.GetChunkCoordinatesContainingPoint(worldPosition, chunkSize);

            IEnumerable<int3> expectedCoordinates = new List<int3>
            {
                new int3( 0,  0, 1),
                new int3( 0,  0, 0),
            };

            Assert.That(coordinates, Is.EquivalentTo(expectedCoordinates));
        }

        [Test]
        public void GetChunkCoordinatesContainingPoint_Test5()
        {
            int3 worldPosition = new int3(-48, -32, -16);
            int chunkSize = 16;

            IEnumerable<int3> coordinates = CoordinateUtilities.GetChunkCoordinatesContainingPoint(worldPosition, chunkSize);

            IEnumerable<int3> expectedCoordinates = new List<int3>
            {
                new int3(-3, -2, -1),
                new int3(-4, -2, -1),
                new int3(-3, -3, -1),
                new int3(-4, -3, -1),
                new int3(-3, -2, -2),
                new int3(-4, -2, -2),
                new int3(-3, -3, -2),
                new int3(-4, -3, -2),
            };

            Assert.That(coordinates, Is.EquivalentTo(expectedCoordinates));
        }

        [Test]
        public void GetChunkCoordinatesContainingPoint_Test6()
        {
            int3 worldPosition = new int3(0, 5, 5);
            int chunkSize = 16;

            IEnumerable<int3> coordinates = CoordinateUtilities.GetChunkCoordinatesContainingPoint(worldPosition, chunkSize);

            IEnumerable<int3> expectedCoordinates = new List<int3>
            {
                new int3( 0, 0, 0),
                new int3(-1, 0, 0),
            };

            Assert.That(coordinates, Is.EquivalentTo(expectedCoordinates));
        }

        
        [Test]
        public void GetChunkCoordinatesContainingPoint_Test7()
        {
            int3 worldPosition = new int3(5, 0, 5);
            int chunkSize = 16;

            IEnumerable<int3> coordinates = CoordinateUtilities.GetChunkCoordinatesContainingPoint(worldPosition, chunkSize);

            IEnumerable<int3> expectedCoordinates = new List<int3>
            {
                new int3(0, 0, 0),
                new int3(0, -1, 0),
            };

            Assert.That(coordinates, Is.EquivalentTo(expectedCoordinates));
        }

        [Test]
        public void GetChunkCoordinatesContainingPoint_Test8()
        {
            int3 worldPosition = new int3(5, 5, 0);
            int chunkSize = 16;

            IEnumerable<int3> coordinates = CoordinateUtilities.GetChunkCoordinatesContainingPoint(worldPosition, chunkSize);

            IEnumerable<int3> expectedCoordinates = new List<int3>
            {
                new int3(0, 0, 0),
                new int3(0, 0, -1),
            };

            Assert.That(coordinates, Is.EquivalentTo(expectedCoordinates));
        }

        
        [Test]
        public void GetChunkCoordinatesContainingPoint_Test9()
        {
            int3 worldPosition = new int3(7, 7, 7);
            int chunkSize = 7;

            IEnumerable<int3> coordinates = CoordinateUtilities.GetChunkCoordinatesContainingPoint(worldPosition, chunkSize);

            IEnumerable<int3> expectedCoordinates = new List<int3>
            {
                new int3(1, 1, 1),
                new int3(0, 1, 1),
                new int3(1, 0, 1),
                new int3(0, 0, 1),
                new int3(1, 1, 0),
                new int3(0, 1, 0),
                new int3(1, 0, 0),
                new int3(0, 0, 0),
            };

            Assert.That(coordinates, Is.EquivalentTo(expectedCoordinates));
        }
    }
}
