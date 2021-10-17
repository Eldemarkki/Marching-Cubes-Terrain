namespace Eldemarkki.VoxelTerrain.Utilities.Intersection
{
    public enum PlaneLineIntersectionResult
    {
        /// <summary>
        /// The plane and the line don't intersect
        /// </summary>
        NoHit,

        /// <summary>
        /// The plane and the line intersected once
        /// </summary>
        OneHit,

        /// <summary>
        /// The line is inside the plane parallel to it
        /// </summary>
        ParallelInsidePlane
    }
}