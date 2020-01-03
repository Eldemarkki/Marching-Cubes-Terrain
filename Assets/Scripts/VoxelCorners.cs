namespace MarchingCubes
{
    public struct VoxelCorners<T>
    { 
        public T Corner1 { get; set; }
        public T Corner2 { get; set; }
        public T Corner3 { get; set; }
        public T Corner4 { get; set; }
        public T Corner5 { get; set; }
        public T Corner6 { get; set; }
        public T Corner7 { get; set; }
        public T Corner8 { get; set; }

        public VoxelCorners(T c1, T c2, T c3, T c4, T c5, T c6, T c7, T c8)
        {
            Corner1 = c1;
            Corner2 = c2;
            Corner3 = c3;
            Corner4 = c4;
            Corner5 = c5;
            Corner6 = c6;
            Corner7 = c7;
            Corner8 = c8;
        }

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