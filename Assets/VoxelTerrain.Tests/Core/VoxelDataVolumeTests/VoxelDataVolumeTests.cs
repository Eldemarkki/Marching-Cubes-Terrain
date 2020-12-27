using NUnit.Framework;
using System;
using Unity.Collections;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.VoxelData.Tests
{
    public class VoxelDataVolumeTests
    {
        private VoxelDataVolume<float> voxelDataVolume;

        [TestCase(0, 9, 3)]
        [TestCase(5, 2, 13)]
        [TestCase(16, 7, 4)]
        public void Test_Xyz_Constructor_Width(int width, int height, int depth)
        {
            voxelDataVolume = new VoxelDataVolume<float>(width, height, depth, Allocator.Temp);
            Assert.AreEqual(width, voxelDataVolume.Width);
        }

        [TestCase(0, 9, 3)]
        [TestCase(5, 2, 13)]
        [TestCase(16, 7, 4)]
        public void Test_Xyz_Constructor_Height(int width, int height, int depth)
        {
            voxelDataVolume = new VoxelDataVolume<float>(width, height, depth, Allocator.Temp);
            Assert.AreEqual(height, voxelDataVolume.Height);
        }

        [TestCase(0, 9, 3)]
        [TestCase(5, 2, 13)]
        [TestCase(16, 7, 4)]
        public void Test_Xyz_Constructor_Depth(int width, int height, int depth)
        {
            voxelDataVolume = new VoxelDataVolume<float>(width, height, depth, Allocator.Temp);
            Assert.AreEqual(depth, voxelDataVolume.Depth);
        }

        [TestCase(0)]
        [TestCase(5)]
        [TestCase(16)]
        public void Test_Int_Size_Constructor_Size(int size)
        {
            voxelDataVolume = new VoxelDataVolume<float>(size, Allocator.Temp);
            Assert.AreEqual(new int3(size, size, size), voxelDataVolume.Size);
        }

        [TestCase(0, 9, 3)]
        [TestCase(5, 2, 13)]
        [TestCase(16, 7, 4)]
        public void Test_Xyz_Constructor_Length(int width, int height, int depth)
        {
            voxelDataVolume = new VoxelDataVolume<float>(width, height, depth, Allocator.Temp);
            Assert.AreEqual(width * height * depth, voxelDataVolume.Length);
        }

        [TestCase(0, 9, 3)]
        [TestCase(5, 2, 13)]
        [TestCase(16, 7, 4)]
        public void Test_Xyz_Constructor_IsCreated(int width, int height, int depth)
        {
            voxelDataVolume = new VoxelDataVolume<float>(width, height, depth, Allocator.Temp);
            Assert.AreEqual(true, voxelDataVolume.IsCreated);
        }

        [TestCase(-10)]
        [TestCase(-5)]
        public void Test_Int_Size_Constructor_Negative_Throws(int size)
        {
            Assert.Throws<System.ArgumentException>(() =>
            {
                voxelDataVolume = new VoxelDataVolume<float>(size, Allocator.Temp);
            });
        }

        [Test]
        public void Test_Default_Constructor_IsNotCreated()
        {
            voxelDataVolume = new VoxelDataVolume<float>();
            Assert.AreEqual(false, voxelDataVolume.IsCreated);
        }

        [Test]
        public void Test_Voxel_Datas_Are_Initialized_To_0()
        {
            voxelDataVolume = new VoxelDataVolume<float>(5, Allocator.Temp);
            for (int i = 0; i < voxelDataVolume.Length; i++)
            {
                Assert.AreEqual(0, voxelDataVolume.GetVoxelData(i));
            }
        }

        private void TestSetGetVoxelData(Func<float> function, float newVoxelData)
        {
            voxelDataVolume = new VoxelDataVolume<float>(5, Allocator.Temp);
            float actualVoxelData = function();
            Assert.IsTrue(AreVoxelDatasSame(newVoxelData, actualVoxelData), $"Expected {newVoxelData}, actual was {actualVoxelData}");
        }

        [Test]
        public void Test_SetGetVoxelData_Index([Random(0, 1f, 5)] float newVoxelData, [Random(0, 5 * 5 * 5 - 1, 5)] int index)
        {
            TestSetGetVoxelData(() =>
            {
                voxelDataVolume.SetVoxelData(newVoxelData, index);
                return voxelDataVolume.GetVoxelData(index);
            }, newVoxelData);
        }

        [Test]
        public void Test_SetGetVoxelData_Xyz([Random(0, 1f, 5)] float newVoxelData, [Random(0, 4, 3)] int x, [Random(0, 4, 3)] int y, [Random(0, 4, 3)] int z)
        {
            TestSetGetVoxelData(() =>
            {
                voxelDataVolume.SetVoxelData(newVoxelData, x, y, z);
                return voxelDataVolume.GetVoxelData(x, y, z);
            }, newVoxelData);
        }

        [Test]
        public void Test_SetGetVoxelData_Int3([Random(0, 1f, 5)] float newVoxelData, [Random(0, 4, 3)] int x, [Random(0, 4, 3)] int y, [Random(0, 4, 3)] int z)
        {
            TestSetGetVoxelData(() =>
            {
                voxelDataVolume.SetVoxelData(newVoxelData, new int3(x, y, z));
                return voxelDataVolume.GetVoxelData(new int3(x, y, z));
            }, newVoxelData);
        }

        private static bool AreVoxelDatasSame(float a, float b)
        {
            byte aByte = (byte)RoundUpIfDecimal5(255f * a);
            byte bByte = (byte)RoundUpIfDecimal5(255f * b);

            return aByte == bByte;
        }

        // This function is an "improvement" (not really...) of math.round()
        // This rounds a number down if its decimal is less than 0.5, and otherwise up:
        // 0.5 => 1
        // 0.49999 => 0
        // 76.5 => 77
        // 76.49999 => 76
        // This was needed because math.round rounded 0.5 => 0, which was incorrect when comparing voxel datas.
        private static int RoundUpIfDecimal5(float x)
        {
            if (x < 0)
            {
                throw new System.ArgumentException("This function only works with positive numbers!");
            }

            int result;
            float decimals = x % 1;
            if (decimals < 0.5f)
            {
                result = (int)math.floor(x);
            }
            else
            {
                result = (int)math.ceil(x);
            }

            return result;
        }

        [TearDown]
        public void Teardown()
        {
            if (voxelDataVolume.IsCreated)
            {
                voxelDataVolume.Dispose();
            }
        }
    }
}
