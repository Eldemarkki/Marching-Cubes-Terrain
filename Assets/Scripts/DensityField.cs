using System;
using UnityEngine;

namespace MarchingCubes
{
    [Serializable]
    public class DensityField : MonoBehaviour
    {
        private float[] data;

        private int size;
        public int Size { get => size; }

        private int width;
        public int Width { get => width; }

        private int height;
        public int Height { get => height; }

        private int depth;
        public int Depth { get => depth; }

        public DensityField(int width, int height, int depth)
        {
            this.size = width * height * depth;
            this.data = new float[size];

            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        public float this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public float this[int x, int y, int z]
        {
            get
            {
                int index = GetIndex(x, y, z);
                return data[index];
            }
            set
            {
                int index = GetIndex(x, y, z);
                data[index] = value;
            }
        }

        private int GetIndex(int x, int y, int z)
        {
            return (x * width * height) + (y * width) + z;
        }

        public void Populate(Func<Vector3Int, float> densityFunction, Vector3Int offset)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        this[x, y, z] = densityFunction.Invoke(new Vector3Int(x, y, z) + offset);
                    }
                }
            }
        }

        public void Populate(Func<int, int, int, float> densityFunction, Vector3Int offset)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    for (int z = 0; z < Depth; z++)
                    {
                        this[x, y, z] = densityFunction.Invoke(x + offset.x, y + offset.y, z + offset.z);
                    }
                }
            }
        }
    }
}