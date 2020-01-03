using System;
using Unity.Collections;
using UnityEngine;

namespace MarchingCubes.Examples
{
    [Serializable]
    public struct HeightmapTerrainSettings
    {
        [SerializeField] private Texture2D heightmap;
        [SerializeField, NonSerialized] private NativeArray<float> heightmapData;
        [SerializeField] private float amplitude;
        [SerializeField] private float heightOffset;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Texture2D Heightmap { get => heightmap; set => heightmap = value; }
        public NativeArray<float> HeightmapData { get => heightmapData; set => heightmapData = value; }
        public float Amplitude { get => amplitude; set => amplitude = value; }
        public float HeightOffset { get => heightOffset; set => heightOffset = value; }

        public HeightmapTerrainSettings(Texture2D heightmap, float amplitude, float heightOffset)
        {
            this.amplitude = amplitude;
            this.heightOffset = heightOffset;

            this.heightmap = heightmap;
            Width = heightmap.width;
            Height = heightmap.height;

            heightmapData = new NativeArray<float>(Width * Height, Allocator.Persistent);
            SetHeightmap(heightmap);
        }

        private void SetHeightmap(Texture2D heightmap)
        {
            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    heightmapData[x + Width * y] = heightmap.GetPixel(x, y).grayscale;
                }
            }
        }

        public void Dispose()
        {
            heightmapData.Dispose();
        }
    }
}