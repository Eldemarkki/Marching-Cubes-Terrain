using System;

namespace MarchingCubes
{
    public class ValueGrid<T>
    {
        private T[] data;

        private int size;
        public int Size { get => size; }

        private int width;
        public int Width { get => width; }

        private int height;
        public int Height { get => height; }

        private int depth;
        public int Depth { get => depth; }

        public ValueGrid(int width, int height, int depth)
        {
            this.size = width * height * depth;
            this.data = new T[size];

            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        public T this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        public T this[int x, int y, int z]
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

        public int GetIndex(int x, int y, int z)
        {
            return (x * width * height) + (y * width) + z;
        }

        public void Populate(Func<int, int, int, T> fillFunction, int offsetX = 0, int offsetY = 0, int offsetZ = 0)
        {
            int i = 0;
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        this[i++] = fillFunction(x + offsetX, y + offsetY, z + offsetZ);
                    }
                }
            }
        }

        public void Populate(Func<int, int, int, T> fillFunction, UnityEngine.Vector3Int offset)
        {
            Populate(fillFunction, offset.x, offset.y, offset.z);
        }
    }
}