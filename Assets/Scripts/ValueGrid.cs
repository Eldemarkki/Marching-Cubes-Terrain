namespace MarchingCubes
{
    public class ValueGrid<T>
    {
        public readonly T[] data;

        public int Width { get; }
        public int Height { get; }
        public int Depth { get; }

        public int Size { get => Width * Height * Depth; }

        public ValueGrid(int width, int height, int depth)
        {
            data = new T[width * height * depth];

            Width = width;
            Height = height;
            Depth = depth;
        }

        public T this[int index]
        {
            get => data[index];
            set => data[index] = value;
        }

        public T this[int x, int y, int z]
        {
            get => data[GetIndex(x, y, z)];
            set => data[GetIndex(x, y, z)] = value;
        }

        public int GetIndex(int x, int y, int z)
        {
            return x * Width * Height + y * Width + z;
        }
    }
}