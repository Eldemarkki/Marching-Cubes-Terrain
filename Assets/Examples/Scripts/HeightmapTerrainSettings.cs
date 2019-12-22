using System;
using Unity.Collections;
using UnityEngine;

namespace MarchingCubes.Examples
{
    [Serializable]
    public struct HeightmapTerrainSettings : IEquatable<HeightmapTerrainSettings>
    {
        [SerializeField] private Texture2D heightmap;
        [SerializeField] private NativeArray<float> heightmapData;
        [SerializeField] private float amplitude;
        [SerializeField] private float heightOffset;

        private int width;
        private int height;

        public int Width { get => width; }
        public int Height { get => height; }

        public float Amplitude { get => amplitude; set => amplitude = value; }
        public float HeightOffset { get => heightOffset; set => heightOffset = value; }
        public NativeArray<float> HeightmapData { get => heightmapData; set => heightmapData = value; }
        public Texture2D Heightmap { get => heightmap; set => heightmap = value; }

        public HeightmapTerrainSettings(Texture2D heightmap, float amplitude, float heightOffset)
        {
            this.amplitude = amplitude;
            this.heightOffset = heightOffset;

            this.heightmap = heightmap;
            width = heightmap.width;
            height = heightmap.height;

            heightmapData = new NativeArray<float>(width * height, Allocator.Persistent);
            SetHeightmap(heightmap);
        }

        public void Dispose()
        {
            this.HeightmapData.Dispose();
        }

        public void SetHeightmap(Texture2D heightmap)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    heightmapData[x + width * y] = heightmap.GetPixel(x, y).grayscale;
                }
            }
        }

        public bool Equals(HeightmapTerrainSettings other)
        {
            return Heightmap == other.Heightmap && amplitude == other.amplitude && heightOffset == other.heightOffset;
        }
    }
}