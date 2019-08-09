using System;
using System.Collections.Generic;

namespace MarchingCubes
{
    public struct VoxelCorners<T> : IEquatable<VoxelCorners<T>>
    {
        private T _c1, _c2, _c3, _c4, _c5, _c6, _c7, _c8;

        public VoxelCorners(T c1, T c2, T c3, T c4, T c5, T c6, T c7, T c8)
        {
            _c1 = c1;
            _c2 = c2;
            _c3 = c3;
            _c4 = c4;
            _c5 = c5;
            _c6 = c6;
            _c7 = c7;
            _c8 = c8;
        }

        public T this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return _c1;
                    case 1: return _c2;
                    case 2: return _c3;
                    case 3: return _c4;
                    case 4: return _c5;
                    case 5: return _c6;
                    case 6: return _c7;
                    case 7: return _c8;
                    default: return _c1;
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
                        _c1 = value;
                        break;
                    case 1:
                        _c2 = value;
                        break;
                    case 2:
                        _c3 = value;
                        break;
                    case 3:
                        _c4 = value;
                        break;
                    case 4:
                        _c5 = value;
                        break;
                    case 5:
                        _c6 = value;
                        break;
                    case 6:
                        _c7 = value;
                        break;
                    case 7:
                        _c8 = value;
                        break;
                    default:
                        _c1 = value;
                        break;
                }
            }
        }

        public bool Equals(VoxelCorners<T> other)
        {
            return EqualityComparer<T>.Default.Equals(_c1, other._c1) && EqualityComparer<T>.Default.Equals(_c2, other._c2) && EqualityComparer<T>.Default.Equals(_c3, other._c3) && EqualityComparer<T>.Default.Equals(_c4, other._c4) && EqualityComparer<T>.Default.Equals(_c5, other._c5) && EqualityComparer<T>.Default.Equals(_c6, other._c6) && EqualityComparer<T>.Default.Equals(_c7, other._c7) && EqualityComparer<T>.Default.Equals(_c8, other._c8);
        }

        public override bool Equals(object obj)
        {
            return obj is VoxelCorners<T> other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = EqualityComparer<T>.Default.GetHashCode(_c1);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(_c2);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(_c3);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(_c4);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(_c5);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(_c6);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(_c7);
                hashCode = (hashCode * 397) ^ EqualityComparer<T>.Default.GetHashCode(_c8);
                return hashCode;
            }
        }
    }
}