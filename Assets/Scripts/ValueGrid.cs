using System;

namespace MarchingCubes
{
    public class ValueGrid<T>
    {
        private T[] _data;

        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }

        public ValueGrid(int width, int height, int depth)
        {
            _data = new T[width * height * depth];

            Width = width;
            Height = height;
            Depth = depth;
        }

        public T this[int index] => _data[index];

        public T this[int x, int y, int z]
        {
            get => _data[GetIndex(x, y, z)];
            set => _data[GetIndex(x, y, z)] = value;
        }

        public int GetIndex(int x, int y, int z)
        {
            return x * Width * Height + y * Width + z;
        }

        public void Populate(Func<int, int, int, T> fillFunction, int offsetX = 0, int offsetY = 0, int offsetZ = 0)
        {
            var i = 0;
            for (var x = 0; x < Width; x++)
            {
                for (var y = 0; y < Height; y++)
                {
                    for (var z = 0; z < Depth; z++)
                    {
                        _data[i++] = fillFunction(x + offsetX, y + offsetY, z + offsetZ);
                    }
                }
            }
        }
    }
}