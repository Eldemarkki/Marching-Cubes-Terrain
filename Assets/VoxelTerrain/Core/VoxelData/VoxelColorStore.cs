using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.VoxelData
{
    public class VoxelColorStore : PerVoxelStore<Color32>
    {
        /// <summary>
        /// Generates the colors for a chunk at <paramref name="chunkCoordinate"/>, where the output array is <paramref name="outputColors"/> to save memory by not needing to allocate a new array.
        /// </summary>
        /// <param name="outputColors">The array that should be filled with the new colors</param>
        protected override unsafe JobHandleWithData<IVoxelDataGenerationJob<Color32>> ScheduleGenerationJob(int3 chunkCoordinate, VoxelDataVolume<Color32> outputColors, JobHandle dependency)
        {
            FillColorJob job = new FillColorJob
            {
                // Fill the array with the any color, with alpha=0 by default
                FillColor = new Color32(0, 0, 0, 0),
                OutputVoxelData = outputColors
            };

            return new JobHandleWithData<IVoxelDataGenerationJob<Color32>>(job.Schedule(dependency), job);
        }

        [BurstCompile]
        private struct FillColorJob : IVoxelDataGenerationJob<Color32>
        {
            private Color32 _fillColor;
            public Color32 FillColor { get => _fillColor; set => _fillColor = value; }

            private VoxelDataVolume<Color32> _outputVoxelData;
            public VoxelDataVolume<Color32> OutputVoxelData { get => _outputVoxelData; set => _outputVoxelData = value; }

            public int3 WorldPositionOffset { get; set; }

            public unsafe void Execute()
            {
                Color32* fillColorPtr = stackalloc Color32[1] { _fillColor };
                UnsafeUtility.MemCpyReplicate(_outputVoxelData.GetUnsafePtr(), fillColorPtr, sizeof(Color32), _outputVoxelData.Length);
            }
        }
    }
}
