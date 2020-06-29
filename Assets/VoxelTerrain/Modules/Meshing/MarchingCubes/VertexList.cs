using System;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Meshing.MarchingCubes
{
    /// <summary>
    /// A container for a vertex list with 12 vertices
    /// </summary>
    public struct VertexList : IEquatable<VertexList>
    {
        /// <summary>
        /// The first vertex
        /// </summary>
        private float3 _c1;

        /// <summary>
        /// The second vertex
        /// </summary>
        private float3 _c2;

        /// <summary>
        /// The third vertex
        /// </summary>
        private float3 _c3;

        /// <summary>
        /// The fourth vertex
        /// </summary>
        private float3 _c4;

        /// <summary>
        /// The fifth vertex
        /// </summary>
        private float3 _c5;

        /// <summary>
        /// The sixth vertex
        /// </summary>
        private float3 _c6;

        /// <summary>
        /// The seventh vertex
        /// </summary>
        private float3 _c7;

        /// <summary>
        /// The eighth vertex
        /// </summary>
        private float3 _c8;

        /// <summary>
        /// The ninth vertex
        /// </summary>
        private float3 _c9;

        /// <summary>
        /// The tenth vertex
        /// </summary>
        private float3 _c10;

        /// <summary>
        /// The eleventh vertex
        /// </summary>
        private float3 _c11;

        /// <summary>
        /// The twelfth vertex
        /// </summary>
        private float3 _c12;

        /// <summary>
        /// The indexer for the vertex list
        /// </summary>
        /// <param name="index">The vertex's index</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the index is more than 11.</exception>
        public float3 this[int index]
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
                    default: throw new ArgumentOutOfRangeException($"There are only 8 corners! You tried to access the corner at index {index.ToString()}");
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
                        throw new ArgumentOutOfRangeException($"There are only 8 corners! You tried to access the corner at index {index.ToString()}");
                }
            }
        }

        public bool Equals(VertexList other)
        {
            if (other == null) { return false; }

            return _c1.Equals(other._c1) &&
                   _c2.Equals(other._c2) &&
                   _c3.Equals(other._c3) &&
                   _c4.Equals(other._c4) &&
                   _c5.Equals(other._c5) &&
                   _c6.Equals(other._c6) &&
                   _c7.Equals(other._c7) &&
                   _c8.Equals(other._c8) &&
                   _c9.Equals(other._c9) &&
                   _c10.Equals(other._c10) &&
                   _c11.Equals(other._c11) &&
                   _c12.Equals(other._c12);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) { return false; }

            if(obj is VertexList other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + _c1.GetHashCode();
                hash = hash * 23 + _c2.GetHashCode();
                hash = hash * 23 + _c3.GetHashCode();
                hash = hash * 23 + _c4.GetHashCode();
                hash = hash * 23 + _c5.GetHashCode();
                hash = hash * 23 + _c6.GetHashCode();
                hash = hash * 23 + _c7.GetHashCode();
                hash = hash * 23 + _c8.GetHashCode();
                hash = hash * 23 + _c9.GetHashCode();
                hash = hash * 23 + _c10.GetHashCode();
                hash = hash * 23 + _c11.GetHashCode();
                hash = hash * 23 + _c12.GetHashCode();

                return hash;
            }
        }

        public static bool operator ==(VertexList left, VertexList right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(VertexList left, VertexList right)
        {
            return !(left == right);
        }
    }
}