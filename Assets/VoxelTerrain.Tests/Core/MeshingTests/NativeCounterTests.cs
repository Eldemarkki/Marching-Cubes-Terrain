using NUnit.Framework;
using Unity.Collections;

namespace Eldemarkki.VoxelTerrain.Meshing.Tests
{
    public class NativeCounterTests
    {
        private NativeCounter counter;

        [SetUp]
        public void SetupNativeCounterTest()
        {
            counter = new NativeCounter(Allocator.Temp);
        }

        [TearDown]
        public void TeardownNativeCounterTest()
        {
            counter.Dispose();
        }

        [Test]
        public void NativeCounter_Count_Initialized_As_0()
        {
            Assert.AreEqual(0, counter.Count);
        }

        [Test]
        public void NativeCounter_Increment_Increases_Count_By_1()
        {
            counter.Increment();
            Assert.AreEqual(1, counter.Count);
        }

        [Test]
        public void NativeCounter_Random_Increment_Sets_Count_To_N([Random(5, 100, 3)] int value)
        {
            for (int i = 0; i < value; i++)
            {
                counter.Increment();
            }

            Assert.AreEqual(value, counter.Count);
        }        
    }
}
