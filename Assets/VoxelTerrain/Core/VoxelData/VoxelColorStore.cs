using Eldemarkki.VoxelTerrain.World;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// A store which handles getting and setting the voxel colors for the world
    /// </summary>
    public class VoxelColorStore : PerVoxelStore<Color32>
    {
        /// <summary>
        /// The default color that the terrain will be colored with on load
        /// </summary>
        [SerializeField] private Color32 defaultTerrainColor = new Color32(11, 91, 33, 255);

        /// <summary>
        /// Generates the colors for a chunk at <paramref name="chunkCoordinate"/>, where the output array is <paramref name="outputColors"/> to save memory by not needing to allocate a new array. This does not check if a color array already exists at <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the colors for</param>
        /// <param name="outputColors">The array that should be filled with the new colors</param>
        public override unsafe void GenerateDataForChunkUnchecked(int3 chunkCoordinate, VoxelDataVolume<Color32> outputColors)
        {
            // Fill the array with the default terrain color
            Color32* defaultColorArray = stackalloc Color32[1]
            {
                defaultTerrainColor
            };

            unsafe
            {
                UnsafeUtility.MemCpyReplicate(outputColors.GetUnsafePtr(), defaultColorArray, sizeof(Color32), outputColors.Length);
            }

            SetDataChunkUnchecked(chunkCoordinate, outputColors, false);
        }
    }
}
