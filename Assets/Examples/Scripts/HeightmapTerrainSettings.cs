using System;
using Unity.Collections;
using UnityEngine;

namespace MarchingCubes.Examples
{
    [Serializable]
    public struct HeightmapTerrainSettings
    {
        /// <summary>
        /// The black and white heightmap texture
        /// </summary>
        [SerializeField] private Texture2D heightmap;

        /// <summary>
        /// The generated height data from the the heightmap
        /// </summary>
        [SerializeField] private NativeArray<float> heightmapData;

        /// <summary>
        /// Height multiplier
        /// </summary>
        [SerializeField] private float amplitude;

        /// <summary>
        /// Moves the sampling point up and down
        /// </summary>
        [SerializeField] private float heightOffset;

        /// <summary>
        /// The width of the heightmap in pixels
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// The height of the heightmap in pixels
        /// </summary>
        public int Height { get; private set; }

        /// <summary>
        /// The black and white heightmap texture
        /// </summary>
        public Texture2D Heightmap
        {
            get => heightmap;
            set => heightmap = value;
        }

        /// <summary>
        /// The generated height data from the the heightmap
        /// </summary>
        public NativeArray<float> HeightmapData
        {
            get => heightmapData;
            set => heightmapData = value;
        }

        /// <summary>
        /// Height multiplier
        /// </summary>
        public float Amplitude
        {
            get => amplitude;
            set => amplitude = value;
        }

        /// <summary>
        /// Moves the sampling point up and down
        /// </summary>
        public float HeightOffset
        {
            get => heightOffset;
            set => heightOffset = value;
        }

        /// <summary>
        /// HeightmapTerrainSettings constructor. Creates HeightmapData from the heightmap
        /// </summary>
        /// <param name="heightmap">The black and white heightmap</param>
        /// <param name="amplitude">Height multiplier</param>
        /// <param name="heightOffset">Moves the sampling point up and down</param>
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

        /// <summary>
        /// Generates the HeightmapData from the heightmap
        /// </summary>
        /// <param name="heightmap">The black and white heightmap</param>
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

        /// <summary>
        /// Disposes HeightmapData
        /// </summary>
        public void DisposeHeightmapData()
        {
            heightmapData.Dispose();
        }
    }
}