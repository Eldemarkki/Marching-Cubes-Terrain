using System;

namespace MarchingCubes
{
    public static class ValueGridExtensions
    {
        public static void Populate<T>(this ValueGrid<T> valueGrid, Func<float, float, float, T> fillFunction, int offsetX, int offsetY, int offsetZ)
        {
            int width = valueGrid.Width;
            int height = valueGrid.Height;
            int depth = valueGrid.Depth;
            int i = 0;

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        valueGrid.Set(i, fillFunction(x + offsetX, y + offsetY, z + offsetZ));
                        i++;
                    }
                }
            }
        }
    }
}