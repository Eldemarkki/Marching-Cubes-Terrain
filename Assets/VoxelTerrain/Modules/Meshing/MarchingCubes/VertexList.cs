using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Meshing.MarchingCubes
{
    /// <summary>
    /// A container for a vertex list with 12 vertices
    /// </summary>
    public struct VertexList : IEnumerable<float3>
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
        public unsafe float3 this[int index]
        {
            // Thanks for the idea to use unsafe pointers to Jackson Dunstan
            // https://www.jacksondunstan.com/articles/5051
            get
            {
                fixed (float3* elements = &_c1)
                {
                    return elements[index];
                }
            }
            // Thanks for the idea to use unsafe pointers to Jackson Dunstan
            // https://www.jacksondunstan.com/articles/5051
            set
            {
                fixed (float3* elements = &_c1)
                {
                    elements[index] = value;
                }
            }
        }

        public IEnumerator<float3> GetEnumerator()
        {
            for (int i = 0; i < 12; i++)
            {
                yield return this[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}