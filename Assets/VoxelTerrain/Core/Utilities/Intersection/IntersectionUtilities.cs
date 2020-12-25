using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Utilities.Intersection
{
    /// <summary>
    /// A utilities class for different kinds of geometric intersections
    /// </summary>
    public static class IntersectionUtilities
    {
        /// <summary>
        /// Intersects a plane with a line
        /// </summary>
        /// <param name="planeOrigin">The origin of the plane</param>
        /// <param name="planeNormal">The normal of the plane</param>
        /// <param name="lineOrigin">The origin of the line</param>
        /// <param name="lineDirection">The direction of the line</param>
        /// <param name="intersectionPoint">The point where the line hit the plane (if any)</param>
        /// <returns>The collision result</returns>
        public static PlaneLineIntersectionResult PlaneLineIntersection(float3 planeOrigin, float3 planeNormal, float3 lineOrigin,
            float3 lineDirection, out float3 intersectionPoint)
        {
            planeNormal = math.normalize(planeNormal);
            lineDirection = math.normalize(lineDirection);

            if (math.dot(planeNormal, lineDirection) == 0)
            {
                intersectionPoint = float3.zero;
                return (planeOrigin - lineOrigin).Equals(float3.zero) ? PlaneLineIntersectionResult.ParallelInsidePlane : PlaneLineIntersectionResult.NoHit;
            }

            float d = math.dot(planeOrigin, -planeNormal);
            float t = -(d + lineOrigin.z * planeNormal.z + lineOrigin.y * planeNormal.y + lineOrigin.x * planeNormal.x) / (lineDirection.z * planeNormal.z + lineDirection.y * planeNormal.y + lineDirection.x * planeNormal.x);
            intersectionPoint = lineOrigin + t * lineDirection;
            return PlaneLineIntersectionResult.OneHit;
        }

        /// <summary>
        /// Gets the volume where the bounds intersect
        /// </summary>
        /// <param name="a">The first bounds</param>
        /// <param name="b">The second bounds</param>
        /// <returns>The volume that is contained in both bounds</returns>
        public static BoundsInt GetIntersectionVolume(BoundsInt a, BoundsInt b)
        {
            int3 min = math.max(a.min.ToInt3(), b.min.ToInt3());
            int3 max = math.min(a.max.ToInt3(), b.max.ToInt3());

            int3 size = max - min;
            return new BoundsInt(min.ToVectorInt(), size.ToVectorInt());
        }
    }
}
