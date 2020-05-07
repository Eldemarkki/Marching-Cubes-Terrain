using Unity.Collections;

namespace MarchingCubes
{
    public struct DensityStorage
    {
        private NativeArray<byte> _densities;

        private int chunkSize;

        public int Length => chunkSize * chunkSize * chunkSize;

        public DensityStorage(int chunkSize)
        {
            this.chunkSize = chunkSize;
            _densities = new NativeArray<byte>(chunkSize * chunkSize * chunkSize, Allocator.Persistent);
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
            return _densities[index] / 127.5f - 1;
        }

        public void SetDensity(float density, int x, int y, int z)
        {
            int index = XYZToIndex(x, y, z);
            SetDensity(density, index);
        }

        public void SetDensity(float density, int index)
        {
            _densities[index] = (byte)(127.5 * (density + 1));
        }

        private int XYZToIndex(int x, int y, int z)
        {
            return x * chunkSize * chunkSize + y * chunkSize + z;
        }
    }
}