using System;
using System.Collections.Generic;

namespace MarchingCubes
{
    public struct VoxelCorners<T> : IEquatable<VoxelCorners<T>>
    {
        public T c1, c2, c3, c4, c5, c6, c7, c8;

        public VoxelCorners(T c1, T c2, T c3, T c4, T c5, T c6, T c7, T c8)
        {
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
            this.c4 = c4;
            this.c5 = c5;
            this.c6 = c6;
            this.c7 = c7;
            this.c8 = c8;
        }

        public T this[int index]
        {
            get
            {
                switch (index)
                {
                    default: return c1;
                    case 1: return c2;
                    case 2: return c3;
                    case 3: return c4;
                    case 4: return c5;
                    case 5: return c6;
                    case 6: return c7;
                    case 7: return c8;
                }
            }
            set
            {
                switch (index)
                {
                    default:
                        c1 = value;
                        break;
                    case 1:
                        c2 = value;
                        break;
                    case 2:
                        c3 = value;
                        break;
                    case 3:
                        c4 = value;
                        break;
                    case 4:
                        c5 = value;
                        break;
                    case 5:
                        c6 = value;
                        break;
                    case 6:
                        c7 = value;
                        break;
                    case 7:
                        c8 = value;
                        break;
                }
            }
        }

        public bool Equals(VoxelCorners<T> other)
        {
            return EqualityComparer<T>.Default.Equals(c1, other.c1) && EqualityComparer<T>.Default.Equals(c2, other.c2) && EqualityComparer<T>.Default.Equals(c3, other.c3) && EqualityComparer<T>.Default.Equals(c4, other.c4) && EqualityComparer<T>.Default.Equals(c5, other.c5) && EqualityComparer<T>.Default.Equals(c6, other.c6) && EqualityComparer<T>.Default.Equals(c7, other.c7) && EqualityComparer<T>.Default.Equals(c8, other.c8);
        }
    }
}