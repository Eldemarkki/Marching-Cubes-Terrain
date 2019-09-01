using System;

namespace MarchingCubes
{
    public static class ValueGridExtensions
    {
        public static void Populate<T>(this ValueGrid<T> valueGrid, Func<int, int, int, T> fillFunction, int offsetX = 0, int offsetY = 0, int offsetZ = 0)
        {
            int width = valueGrid.Width;
            int height = valueGrid.Height;
            int depth = valueGrid.Depth;
            var i = 0;

            for (var x = 0; x < width; x++)
            {
                for (var y = 0; y < height; y++)
                {
                    for (var z = 0; z < depth; z++)
                    {
                        valueGrid.data[i++] = fillFunction(x + offsetX, y + offsetY, z + offsetZ);
                    }
                }
            }
        }
    }
}