using Unity.Collections;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    public static class NativeArrayUtilities
    {
        public static bool TryGetElement<T>(this NativeArray<T> array, int3 arrayDimensions, int3 elementPosition, out T element) where T : struct
        {
            int index = IndexUtilities.XyzToIndex(elementPosition, arrayDimensions.x, arrayDimensions.y);
            if (index >= 0 && index < array.Length)
            {
                element = array[index];
                return true;
            }

            element = default;
            return false;
        }

        public static bool TryGetElement<T>(this NativeArray<T> array, int elementIndex, out T element) where T : struct
        {
            if (elementIndex >= 0 && elementIndex < array.Length)
            {
                element = array[elementIndex];
                return true;
            }

            element = default;
            return false;
        }

        public static void SetElement<T>(this NativeArray<T> array, T element, int elementIndex) where T : struct
        {
            array[elementIndex] = element;
        }

        public static void SetElement<T>(this NativeArray<T> array, T element, int3 dimensions, int3 localPosition) where T : struct
        {
            int index = IndexUtilities.XyzToIndex(localPosition, dimensions.x, dimensions.y);
            array[index] = element;
        }
    }
}
