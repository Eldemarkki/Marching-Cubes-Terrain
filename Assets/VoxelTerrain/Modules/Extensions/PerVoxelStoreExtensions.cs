using Eldemarkki.VoxelTerrain.Utilities;
using Eldemarkki.VoxelTerrain.World;
using System;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Extensions
{
    public static class PerVoxelStoreExtensions
    {
        public static void SetVoxelDataInSphere<T>(this PerVoxelStore<T> store, float3 point, float radius, Func<int3, float, T, T> setVoxelDataFunction, bool calculateDistance = true, bool getOriginalVoxelData = true) where T : struct
        {
            int3 hitPoint = (int3)math.round(point);
            int3 rangeInt3 = new int3(math.ceil(radius));
            BoundsInt worldSpaceQuery = new BoundsInt();
            worldSpaceQuery.SetMinMax((hitPoint - rangeInt3).ToVectorInt(), (hitPoint + rangeInt3).ToVectorInt());

            store.SetVoxelDataCustom(worldSpaceQuery, (voxelDataWorldPosition, voxelData) =>
            {
                float distancesq = math.distancesq(point, voxelDataWorldPosition);
                return distancesq <= radius * radius ? setVoxelDataFunction(voxelDataWorldPosition, calculateDistance ? math.sqrt(distancesq) : 0, voxelData) : voxelData;
            }, getOriginalVoxelData);
        }
    }
}