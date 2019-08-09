using System;
using UnityEngine;

namespace MarchingCubes
{
    public struct VertexList : IEquatable<VertexList>
    {
        private Vector3 _c1, _c2, _c3, _c4, _c5, _c6, _c7, _c8, _c9, _c10, _c11, _c12;

        public Vector3 this[int index]
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
                    case 8: return _c9;
                    case 9: return _c10;
                    case 10: return _c11;
                    case 11: return _c12;
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
                    case 8:
                        _c9 = value;
                        break;
                    case 9:
                        _c10 = value;
                        break;
                    case 10:
                        _c11 = value;
                        break;
                    case 11:
                        _c12 = value;
                        break;
                    default:
                        _c1 = value;
                        break;
                }
            }
        }

        public bool Equals(VertexList other)
        {
            return _c1.Equals(other._c1) && _c2.Equals(other._c2) && _c3.Equals(other._c3) && _c4.Equals(other._c4) && _c5.Equals(other._c5) && _c6.Equals(other._c6) && _c7.Equals(other._c7) && _c8.Equals(other._c8) && _c9.Equals(other._c9) && _c10.Equals(other._c10) && _c11.Equals(other._c11) && _c12.Equals(other._c12);
        }

        public override bool Equals(object obj)
        {
            return obj is VertexList other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = _c1.GetHashCode();
                hashCode = (hashCode * 397) ^ _c2.GetHashCode();
                hashCode = (hashCode * 397) ^ _c3.GetHashCode();
                hashCode = (hashCode * 397) ^ _c4.GetHashCode();
                hashCode = (hashCode * 397) ^ _c5.GetHashCode();
                hashCode = (hashCode * 397) ^ _c6.GetHashCode();
                hashCode = (hashCode * 397) ^ _c7.GetHashCode();
                hashCode = (hashCode * 397) ^ _c8.GetHashCode();
                hashCode = (hashCode * 397) ^ _c9.GetHashCode();
                hashCode = (hashCode * 397) ^ _c10.GetHashCode();
                hashCode = (hashCode * 397) ^ _c11.GetHashCode();
                hashCode = (hashCode * 397) ^ _c12.GetHashCode();
                return hashCode;
            }
        }
    }
}