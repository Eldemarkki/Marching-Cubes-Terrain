using Unity.Mathematics;

namespace MarchingCubes.Examples.Utilities
{
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

            var d = math.dot(planeOrigin, -planeNormal);
            var t = -(d + lineOrigin.z * planeNormal.z + lineOrigin.y * planeNormal.y + lineOrigin.x * planeNormal.x) / (lineDirection.z * planeNormal.z + lineDirection.y * planeNormal.y + lineDirection.x * planeNormal.x);
            intersectionPoint = lineOrigin + t * lineDirection;
            return PlaneLineIntersectionResult.OneHit;
        }
    }
}
