using NUnit.Framework;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities.Intersection.Tests
{
    public class PlaneLineIntersectionTests
    {
        private static void TestPlaneLineIntersectionResult(float3 planeOrigin, float3 planeNormal, float3 lineOrigin, float3 lineDirection, PlaneLineIntersectionResult expected)
        {
            PlaneLineIntersectionResult result = IntersectionUtilities.PlaneLineIntersection(planeOrigin, math.normalize(planeNormal), lineOrigin, lineDirection, out _);
            Assert.AreEqual(expected, result);
        }

        private static void TestPlaneLineIntersectionPoint(float3 planeOrigin, float3 planeNormal, float3 lineOrigin, float3 lineDirection, float3 expected)
        {
            _ = IntersectionUtilities.PlaneLineIntersection(planeOrigin, planeNormal, lineOrigin, lineDirection, out float3 intersectionPoint);
            Assert.AreEqual(expected, intersectionPoint);
        }

        [Test]
        public void Line_Above_Directly_Down_Result_Should_Be_OneHit()
        {
            TestPlaneLineIntersectionResult(new float3(0, 0, 0), new float3(0, 1, 0), new float3(0, 10, 0), new float3(0, -1, 0), PlaneLineIntersectionResult.OneHit);
        }

        [Test]
        public void Line_Above_Directly_Down_Should_Hit_0_0_0()
        {
            TestPlaneLineIntersectionPoint(new float3(0, 0, 0), new float3(0, 1, 0), new float3(0, 10, 0), new float3(0, -1, 0), new float3(0, 0, 0));
        }

        [Test]
        public void Line_At_10_4_5_Directly_Down_Should_Hit_10_0_5()
        {
            TestPlaneLineIntersectionPoint(new float3(0, 0, 0), new float3(0, 1, 0), new float3(10, 4, 5), new float3(0, -1, 0), new float3(10, 0, 5));
        }

        [Test]
        public void Line_Parallel_To_Plane_Not_Inside_Result_Should_Be_NoHit()
        {
            TestPlaneLineIntersectionResult(new float3(0, 4, 0), new float3(0, 1, 0), new float3(0, 10, 0), new float3(1, 0, 0), PlaneLineIntersectionResult.NoHit);
        }

        [Test]
        public void Line_Parallel_To_Plane_Inside_Result_Should_Be_ParallelInsidePlane()
        {
            TestPlaneLineIntersectionResult(new float3(0, 4, 0), new float3(0, 1, 0), new float3(0, 4, 0), new float3(1, 0, 0), PlaneLineIntersectionResult.ParallelInsidePlane);
        }

        [Test]
        public void Line_At_1_0_0_Plane_At_0_0_0_45_Degree_Angle_Should_Be_1_Negative1_0()
        {
            TestPlaneLineIntersectionPoint(new float3(0, 0, 0), new float3(1, 1, 0), new float3(1, 0, 0), new float3(0, -1, 0), new float3(1, -1, 0));
        }

        [Test]
        public void Line_At_1_0_0_Plane_At_Negative1_0_0_45_Degree_Angle_Should_Be_1_Negative2_0()
        {
            TestPlaneLineIntersectionPoint(new float3(-1, 0, 0), new float3(1, 1, 0), new float3(1, 0, 0), new float3(0, -1, 0), new float3(1, -2, 0));
        }

        [Test]
        public void Line_At_1_0_0_Plane_At_0_0_0_Custom_Angle_Should_Be_1_Negative2_0()
        {
            TestPlaneLineIntersectionPoint(new float3(0, 0, 0), new float3(1, 0.5f, 0), new float3(1, 0, 0), new float3(0, -1, 0), new float3(1, -2, 0));
        }
    }
}
