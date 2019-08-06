using UnityEngine;

namespace MarchingCubes
{
    public struct VertexList
    {
        private Vector3 c1, c2, c3, c4, c5, c6, c7, c8, c9, c10, c11, c12;

        public VertexList(Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4, Vector3 c5, Vector3 c6, Vector3 c7,
            Vector3 c8, Vector3 c9, Vector3 c10, Vector3 c11, Vector3 c12)
        {
            this.c1 = c1;
            this.c2 = c2;
            this.c3 = c3;
            this.c4 = c4;
            this.c5 = c5;
            this.c6 = c6;
            this.c7 = c7;
            this.c8 = c8;
            this.c9 = c9;
            this.c10 = c10;
            this.c11 = c11;
            this.c12 = c12;
        }

        public Vector3 this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return c1;
                    case 1: return c2;
                    case 2: return c3;
                    case 3: return c4;
                    case 4: return c5;
                    case 5: return c6;
                    case 6: return c7;
                    case 7: return c8;
                    case 8: return c9;
                    case 9: return c10;
                    case 10: return c11;
                    case 11: return c12;
                    default: throw new System.IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0:
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
                    case 8:
                        c9 = value;
                        break;
                    case 9:
                        c10 = value;
                        break;
                    case 10:
                        c11 = value;
                        break;
                    case 11:
                        c12 = value;
                        break;
                    default: throw new System.IndexOutOfRangeException();
                }
            }
        }
    }
}