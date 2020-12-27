using System.Collections.Generic;
using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using Eldemarkki.VoxelTerrain.World.Chunks;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    /// <summary>
    /// A store which handles getting and setting the voxel colors for the world
    /// </summary>
    public class VoxelColorStore : MonoBehaviour
    {
        /// <summary>
        /// The default color that the terrain will be colored with on load
        /// </summary>
        [SerializeField] private Color32 defaultTerrainColor = new Color32(11, 91, 33, 255);

        /// <summary>
        /// A dictionary containing the colors of the voxels. Key is the chunk's coordinate, and the value is the voxel colors of the chunk.
        /// </summary>
        private Dictionary<int3, NativeArray<Color32>> _chunkColors;

        /// <summary>
        /// The world that "owns" this voxel color store
        /// </summary>
        public VoxelWorld VoxelWorld { get; set; }

        private void Awake()
        {
            _chunkColors = new Dictionary<int3, NativeArray<Color32>>();
        }

        private void OnApplicationQuit()
        {
            foreach (NativeArray<Color32> colors in _chunkColors.Values)
            {
                if (colors.IsCreated)
                {
                    colors.Dispose();
                }
            }
        }

        /// <summary>
        /// Set's the color of the voxel corner at <paramref name="colorWorldPosition"/> to <paramref name="color"/>
        /// </summary>
        /// <param name="colorWorldPosition">The world position of the corner</param>
        /// <param name="color">The new color of the corner</param>
        public void SetColor(int3 colorWorldPosition, Color32 color)
        {
            IEnumerable<int3> affectedChunkCoordinates = ChunkUtilities.GetChunkCoordinatesContainingPoint(colorWorldPosition, VoxelWorld.WorldSettings.ChunkSize);

            foreach (int3 chunkCoordinate in affectedChunkCoordinates)
            {
                if (TryGetVoxelColorsChunk(chunkCoordinate, out NativeArray<Color32> colors))
                {
                    int3 localPos = (colorWorldPosition - chunkCoordinate * VoxelWorld.WorldSettings.ChunkSize).Mod(VoxelWorld.WorldSettings.ChunkSize + 1);

                    int index = IndexUtilities.XyzToIndex(localPos, VoxelWorld.WorldSettings.ChunkSize.x + 1, VoxelWorld.WorldSettings.ChunkSize.y + 1);

                    colors[index] = color;

                    if (VoxelWorld.ChunkStore.TryGetChunkAtCoordinate(chunkCoordinate, out ChunkProperties chunkProperties))
                    {
                        chunkProperties.HasChanges = true;
                    }
                }
            }
        }

        /// <summary>
        /// Moves a chunk from coordinate <paramref name="from"/> to the coordinate <paramref name="to"/> and generates the colors for the chunk at <paramref name="to"/>
        /// </summary>
        /// <param name="from">The coordinate to move the chunk from</param>
        /// <param name="to">The new coordinate of the chunk</param>
        public void MoveChunk(int3 from, int3 to)
        {
            // Check that 'from' and 'to' are not equal
            if (from.Equals(to)) { return; }

            // Check that a chunk exists at 'from'
            if (TryGetVoxelColorsChunk(from, out NativeArray<Color32> fromColors))
            {
                _chunkColors.Remove(from);

                GenerateColorsForChunk(to, fromColors);
            }
        }

        /// <summary>
        /// Generates the colors for a chunk at <paramref name="chunkCoordinate"/>; fills the color array with the default color
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the colors for</param>
        public void GenerateColorsForChunk(int3 chunkCoordinate)
        {
            if (!_chunkColors.ContainsKey(chunkCoordinate))
            {
                NativeArray<Color32> colors = new NativeArray<Color32>((VoxelWorld.WorldSettings.ChunkSize.x + 1) * (VoxelWorld.WorldSettings.ChunkSize.y + 1) * (VoxelWorld.WorldSettings.ChunkSize.z + 1), Allocator.Persistent);

                GenerateColorsForChunkUnchecked(chunkCoordinate, colors);
            }
        }

        /// <summary>
        /// Generates the colors for a chunk at <paramref name="chunkCoordinate"/>, where the output array is <paramref name="outputColors"/> to save memory by not needing to allocate a new array; fills the color array with the default color
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the colors for</param>
        /// <param name="outputColors">The array that should be filled with the new colors</param>
        public unsafe void GenerateColorsForChunk(int3 chunkCoordinate, NativeArray<Color32> outputColors)
        {
            if (!_chunkColors.ContainsKey(chunkCoordinate))
            {
                GenerateColorsForChunkUnchecked(chunkCoordinate, outputColors);
            }
        }

        /// <summary>
        /// Generates the colors for a chunk at <paramref name="chunkCoordinate"/>, where the output array is <paramref name="outputColors"/> to save memory by not needing to allocate a new array. This does not check if a color array already exists at <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the colors for</param>
        /// <param name="outputColors">The array that should be filled with the new colors</param>
        private unsafe void GenerateColorsForChunkUnchecked(int3 chunkCoordinate, NativeArray<Color32> outputColors)
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

            SetVoxelColorsChunkUnchecked(chunkCoordinate, outputColors, false);
        }

        /// <summary>
        /// Sets the voxel colors of a chunk at <paramref name="chunkCoordinate"/> without checking if colors already exist for that chunk
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk</param>
        /// <param name="newColors">The colors to set the chunk's colors to</param>
        public void SetVoxelColorsChunkUnchecked(int3 chunkCoordinate, NativeArray<Color32> newColors, bool exists)
        {
            if (exists)
            {
                NativeArray<Color32> oldColors = _chunkColors[chunkCoordinate];
                oldColors.CopyFrom(newColors);
                newColors.Dispose();
            }
            else
            {
                _chunkColors.Add(chunkCoordinate, newColors);
            }

            if (VoxelWorld.ChunkStore.TryGetChunkAtCoordinate(chunkCoordinate, out ChunkProperties chunkProperties))
            {
                chunkProperties.HasChanges = true;
            }
        }

        /// <summary>
        /// Tries to get the colors of a chunk
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk whose colors should be gotten</param>
        /// <param name="colors">The colors of the chunk</param>
        /// <returns>True if a chunk exists at <paramref name="chunkCoordinate"/>, otherwise false.</returns>
        public bool TryGetVoxelColorsChunk(int3 chunkCoordinate, out NativeArray<Color32> colors)
        {
            return _chunkColors.TryGetValue(chunkCoordinate, out colors);
        }
    }
}
