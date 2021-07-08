using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
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
        /// Generates the colors for a chunk at <paramref name="chunkCoordinate"/>, where the output array is <paramref name="outputColors"/> to save memory by not needing to allocate a new array. This does not check if a color array already exists at <paramref name="chunkCoordinate"/>
        /// </summary>
        /// <param name="chunkCoordinate">The coordinate of the chunk which to generate the colors for</param>
        /// <param name="outputColors">The array that should be filled with the new colors</param>
        public override unsafe JobHandle GenerateDataForChunkUnchecked(int3 chunkCoordinate, VoxelDataVolume<Color32> outputColors, JobHandle dependency)
        {
            // Fill the array with the any color, with alpha=0 by default
            Color32 fillColor = new Color32(0, 0, 0, 0);

            FillColorJob job = new FillColorJob
            {
                FillColor = fillColor,
                OutputVoxelData = outputColors
            };

            JobHandle jobHandle = job.Schedule(dependency);
            JobHandleWithData<IVoxelDataGenerationJob<Color32>> jobHandleWithData = new JobHandleWithData<IVoxelDataGenerationJob<Color32>>
            {
                JobData = job,
                JobHandle = jobHandle,
            };

            _generationJobHandles.Add(chunkCoordinate, jobHandleWithData);
            return jobHandle;
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
                var fillColorPtr = stackalloc Color32[1];
                fillColorPtr[0] = _fillColor;

                UnsafeUtility.MemCpyReplicate(_outputVoxelData.GetUnsafePtr(), fillColorPtr, sizeof(Color32), _outputVoxelData.Length);
                _outputVoxelData.GetUnsafePtr();
            }
        }
    }
}
