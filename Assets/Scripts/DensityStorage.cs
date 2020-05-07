using Unity.Collections;

namespace MarchingCubes
{
    public struct DensityStorage
    {
        private NativeArray<float> _densities;

        private int chunkSize;

        public int Length => chunkSize * chunkSize * chunkSize;

        public DensityStorage(int chunkSize)
        {
            this.chunkSize = chunkSize;
            _densities = new NativeArray<float>(chunkSize * chunkSize * chunkSize, Allocator.Persistent);
        }

        public void Dispose()
        {
            _densities.Dispose();
        }

        public float GetDensity(int x, int y, int z)
        {
            int index = XYZToIndex(x, y, z);
            return GetDensity(index);
        }

        public float GetDensity(int index)
        {
            return _densities[index];
        }

        public void SetDensity(float density, int x, int y, int z)
        {
            int index = XYZToIndex(x, y, z);
            SetDensity(density, index);
        }

        public void SetDensity(float density, int index)
        {
            _densities[index] = density;
        }

        private int XYZToIndex(int x, int y, int z)
        {
            return x * chunkSize * chunkSize + y * chunkSize + z;
        }
    }
}