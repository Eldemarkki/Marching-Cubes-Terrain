using Unity.Collections;
using Unity.Mathematics;

namespace Eldemarkki.VoxelTerrain.Utilities
{
    /// <summary>
    /// Utility class for accessing the elements of a native array in different ways
    /// </summary>
    public static class NativeArrayIndexingUtilities
    {
        /// <summary>
        /// Tries to get an element in <paramref name="array"/> at position <paramref name="elementPosition"/>, when the size of <paramref name="array"/> is <paramref name="arrayDimensions"/>. If the position is valid, it sets <paramref name="element"/> to the value at that point and return true. If the position is invalid, <paramref name="element"/> will be set to default(<typeparamref name="T"/>) and false is returned.
        /// </summary>
        /// <typeparam name="T">The array type</typeparam>
        /// <param name="array">The array that is being indexed</param>
        /// <param name="arrayDimensions">The 3-dimensional size of the array</param>
        /// <param name="elementPosition">The position of the element in the array that should be gotten</param>
        /// <param name="element">The element at that position, assuming the position is valid</param>
        /// <returns>Returns true if the retrieval was successful, false otherwise</returns>
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

        /// <summary>
        /// Tries to get an element in <paramref name="array"/> at index <paramref name="elementIndex"/>. If the position is valid, it sets <paramref name="element"/> to the value at that point and return true. If the position is invalid, <paramref name="element"/> will be set to default(<typeparamref name="T"/>) and false is returned.
        /// </summary>
        /// <typeparam name="T">The array type</typeparam>
        /// <param name="array">The array that is being indexed</param>
        /// <param name="elementIndex">The index of the element that should be gotten</param>
        /// <param name="element">The element at that position, assuming the index is valid</param>
        /// <returns>Returns true if the retrieval was successful, false otherwise</returns>
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

        /// <summary>
        /// Sets the element in index <paramref name="elementIndex"/> in <paramref name="array"/> to <paramref name="element"/>
        /// </summary>
        /// <typeparam name="T">The array type</typeparam>
        /// <param name="array">The array that should be modified</param>
        /// <param name="element">The new element that will inserted into the array</param>
        /// <param name="elementIndex">The index of the element that will be set</param>
        public static void SetElement<T>(this NativeArray<T> array, T element, int elementIndex) where T : struct
        {
            array[elementIndex] = element;
        }

        /// <summary>
        /// Sets the element at position <paramref name="elementPosition"/> in <paramref name="array"/> to <paramref name="element"/>
        /// </summary>
        /// <typeparam name="T">The array type</typeparam>
        /// <param name="array">The array that should be modified</param>
        /// <param name="element">The new element that will inserted into the array</param>
        /// <param name="arrayDimensions">The 3-dimensional size of the array</param>
        /// <param name="elementPosition">The position of the element in the array that should be set</param>
        public static void SetElement<T>(this NativeArray<T> array, T element, int3 arrayDimensions, int3 elementPosition) where T : struct
        {
            int index = IndexUtilities.XyzToIndex(elementPosition, arrayDimensions.x, arrayDimensions.y);
            array[index] = element;
        }
    }
}
