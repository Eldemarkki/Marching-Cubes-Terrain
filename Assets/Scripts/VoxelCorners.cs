namespace MarchingCubes
{
    /// <summary>
    /// A container for the voxel's corners
    /// </summary>
    /// <typeparam name="T">The element's type to hold in the corners</typeparam>
    public struct VoxelCorners<T>
    {
        /// <summary>
        /// The first corner
        /// </summary>
        public T Corner1 { get; set; }

        /// <summary>
        /// The second corner
        /// </summary>
        public T Corner2 { get; set; }

        /// <summary>
        /// The third corner
        /// </summary>
        public T Corner3 { get; set; }

        /// <summary>
        /// The fourth corner
        /// </summary>
        public T Corner4 { get; set; }

        /// <summary>
        /// The fifth corner
        /// </summary>
        public T Corner5 { get; set; }

        /// <summary>
        /// The sixth corner
        /// </summary>
        public T Corner6 { get; set; }

        /// <summary>
        /// The seventh corner
        /// </summary>
        public T Corner7 { get; set; }

        /// <summary>
        /// The eighth corner
        /// </summary>
        public T Corner8 { get; set; }

        /// <summary>
        /// The indexer for the voxel corners
        /// </summary>
        /// <param name="index">The corner's index</param>
        /// <exception cref="System.IndexOutOfRangeException">Thrown when index is more than 7.</exception>
        public T this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return Corner1;
                    case 1: return Corner2;
                    case 2: return Corner3;
                    case 3: return Corner4;
                    case 4: return Corner5;
                    case 5: return Corner6;
                    case 6: return Corner7;
                    case 7: return Corner8;
                    default: throw new System.IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        Corner1 = value;
                        break;
                    case 1:
                        Corner2 = value;
                        break;
                    case 2:
                        Corner3 = value;
                        break;
                    case 3:
                        Corner4 = value;
                        break;
                    case 4:
                        Corner5 = value;
                        break;
                    case 5:
                        Corner6 = value;
                        break;
                    case 6:
                        Corner7 = value;
                        break;
                    case 7:
                        Corner8 = value;
                        break;
                    default: throw new System.IndexOutOfRangeException();
                }
            }
        }
    }
}