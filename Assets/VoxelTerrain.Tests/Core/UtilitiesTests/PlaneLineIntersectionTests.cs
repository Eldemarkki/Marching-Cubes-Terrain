using NUnit.Framework;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities.Intersection
{
    public class PlaneLineIntersectionTests
    {
        [Test]
        public void Line_Above_Directly_Down_Result_Should_Be_OneHit()
        {
            float3 planeOrigin = new float3(0, 0, 0);
            float3 planeNormal = math.normalize(new float3(0, 1, 0));
            float3 lineOrigin = new float3(0, 10, 0);
            float3 lineDirection = new float3(0, -1, 0);

            PlaneLineIntersectionResult result = IntersectionUtilities.PlaneLineIntersection(planeOrigin, planeNormal, lineOrigin, lineDirection, out float3 intersectionPoint);
            Assert.AreEqual(PlaneLineIntersectionResult.OneHit, result);
        }

        [Test]
        public void Line_Above_Directly_Down_Should_Hit_0_0_0()
        {
            float3 planeOrigin = new float3(0, 0, 0);
            float3 planeNormal = math.normalize(new float3(0, 1, 0));
            float3 lineOrigin = new float3(0, 10, 0);
            float3 lineDirection = new float3(0, -1, 0);

            PlaneLineIntersectionResult result = IntersectionUtilities.PlaneLineIntersection(planeOrigin, planeNormal, lineOrigin, lineDirection, out float3 intersectionPoint);
            Assert.AreEqual(new float3(0, 0, 0), intersectionPoint);
        }

        [Test]
        public void Line_At_10_4_5_Directly_Down_Should_Hit_10_0_5()
        {
            float3 planeOrigin = new float3(0, 0, 0);
            float3 planeNormal = math.normalize(new float3(0, 1, 0));
            float3 lineOrigin = new float3(10, 4, 5);
            float3 lineDirection = new float3(0, -1, 0);

            PlaneLineIntersectionResult result = IntersectionUtilities.PlaneLineIntersection(planeOrigin, planeNormal, lineOrigin, lineDirection, out float3 intersectionPoint);
            Assert.AreEqual(new float3(10, 0, 5), intersectionPoint);
        }


        [Test]
        public void Line_Parallel_To_Plane_Not_Inside_Result_Should_Be_NoHit()
        {
            float3 planeOrigin = new float3(0, 4, 0);
            float3 planeNormal = math.normalize(new float3(0, 1, 0));
            float3 lineOrigin = new float3(0, 10, 0);
            float3 lineDirection = new float3(1, 0, 0);

            PlaneLineIntersectionResult result = IntersectionUtilities.PlaneLineIntersection(planeOrigin, planeNormal, lineOrigin, lineDirection, out float3 intersectionPoint);
            Assert.AreEqual(PlaneLineIntersectionResult.NoHit, result);
        }

        [Test]
        public void Line_Parallel_To_Plane_Inside_Result_Should_Be_ParallelInsidePlane()
        {
            float3 planeOrigin = new float3(0, 4, 0);
            float3 planeNormal = math.normalize(new float3(0, 1, 0));
            float3 lineOrigin = new float3(0, 4, 0);
            float3 lineDirection = new float3(1, 0, 0);

            PlaneLineIntersectionResult result = IntersectionUtilities.PlaneLineIntersection(planeOrigin, planeNormal, lineOrigin, lineDirection, out float3 intersectionPoint);
            Assert.AreEqual(PlaneLineIntersectionResult.ParallelInsidePlane, result);
        }

        [Test]
        public void Line_At_1_0_0_Plane_At_0_0_0_45_Degree_Angle_Should_Be_1_Negative1_0()
        {
            float3 planeOrigin = new float3(0, 0, 0);
            float3 planeNormal = math.normalize(new float3(1, 1, 0));
            float3 lineOrigin = new float3(1, 0, 0);
            float3 lineDirection = new float3(0, -1, 0);

            PlaneLineIntersectionResult result = IntersectionUtilities.PlaneLineIntersection(planeOrigin, planeNormal, lineOrigin, lineDirection, out float3 intersectionPoint);
            Assert.AreEqual(new float3(1, -1, 0), intersectionPoint);
        }


        [Test]
        public void Line_At_1_0_0_Plane_At_Negative1_0_0_45_Degree_Angle_Should_Be_1_Negative2_0()
        {
            float3 planeOrigin = new float3(-1, 0, 0);
            float3 planeNormal = math.normalize(new float3(1, 1, 0));
            float3 lineOrigin = new float3(1, 0, 0);
            float3 lineDirection = new float3(0, -1, 0);

            PlaneLineIntersectionResult result = IntersectionUtilities.PlaneLineIntersection(planeOrigin, planeNormal, lineOrigin, lineDirection, out float3 intersectionPoint);
            Assert.AreEqual(new float3(1, -2, 0), intersectionPoint);
        }


        [Test]
        public void Line_At_1_0_0_Plane_At_0_0_0_Custom_Angle_Should_Be_1_Negative2_0()
        {
            float3 planeOrigin = new float3(0, 0, 0);
            float3 planeNormal = math.normalize(new float3(1,0.5f, 0));
            float3 lineOrigin = new float3(1, 0, 0);
            float3 lineDirection = new float3(0, -1, 0);

            PlaneLineIntersectionResult result = IntersectionUtilities.PlaneLineIntersection(planeOrigin, planeNormal, lineOrigin, lineDirection, out float3 intersectionPoint);
            Assert.AreEqual(new float3(1, -2, 0), intersectionPoint);
        }
    }
}
