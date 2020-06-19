using Eldemarkki.VoxelTerrain.Density;
using Eldemarkki.VoxelTerrain.Utilities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;

namespace Eldemarkki.VoxelTerrain.World
{
    public class HeightmapVoxelDataGenerator : VoxelDataGenerator
    {
        [SerializeField] private HeightmapWorldGenerator heightmapWorldGenerator;

        public override DensityVolume GenerateVoxelData(Bounds bounds, Allocator allocator = Allocator.Persistent)
        {
            DensityVolume voxelData = new DensityVolume(bounds.size.ToInt3(), allocator);

            JobHandle jobHandle = new HeightmapTerrainDensityCalculationJob
            {
                WorldPositionOffset = bounds.min.ToInt3(),
                OutputVoxelData = voxelData,
                heightmapData = heightmapWorldGenerator.HeightmapTerrainSettings.HeightmapData,
                heightmapWidth = heightmapWorldGenerator.HeightmapTerrainSettings.Width,
                heightmapHeight = heightmapWorldGenerator.HeightmapTerrainSettings.Height,
                amplitude = heightmapWorldGenerator.HeightmapTerrainSettings.Amplitude,
                heightOffset = heightmapWorldGenerator.HeightmapTerrainSettings.HeightOffset
            }.Schedule(voxelData.Length, 256);

            jobHandle.Complete();

            return voxelData;
        }
    }
}