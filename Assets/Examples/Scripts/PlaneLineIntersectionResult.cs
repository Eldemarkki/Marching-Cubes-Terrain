namespace MarchingCubes.Examples
{
    /// <summary>
    /// A plane-line intersection result
    /// </summary>
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