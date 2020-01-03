using Unity.Mathematics;

namespace MarchingCubes
{
    public struct VertexList
    {
        private float3 _c1, _c2, _c3, _c4, _c5, _c6, _c7, _c8, _c9, _c10, _c11, _c12;

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
                    default: throw new System.IndexOutOfRangeException();
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
                        throw new System.IndexOutOfRangeException();
                }
            }
        }
    }
}