namespace MarchingCubes
{
    public class ValueGrid<T>
    {
        public readonly T[] data;
        private int width;
        private int height;
        private int depth;

        public int Width { get => width; }
        public int Height { get => height; }
        public int Depth { get => depth; }

        public int Size { get => Width * Height * Depth; }

        public ValueGrid(int width, int height, int depth)
        {
            data = new T[width * height * depth];

            this.width = width;
            this.height = height;
            this.depth = depth;
        }

        public T Get(int x, int y, int z){
            return data[x * width * height + y * width + z];
        }
        
        public void Set(int x, int y, int z, T value){
            data[x * width * height + y * width + z] = value;
        }
    }
}