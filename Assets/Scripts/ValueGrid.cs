namespace MarchingCubes
{
    public class ValueGrid<T>
    {
        private readonly T[] _data;

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

        public T this[int index]
        {
            get => _data[index];
            set => _data[index] = value;
        }

        public T this[int x, int y, int z]
        {
            get => _data[GetIndex(x, y, z)];
            set => _data[GetIndex(x, y, z)] = value;
        }

        public int GetIndex(int x, int y, int z)
        {
            return x * Width * Height + y * Width + z;
        }
    }
}