using System;
using Unity.Collections;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Settings
{
    [CreateAssetMenu(fileName = "New Heightmap Terrain Settings", menuName = "Marching Cubes Terrain/Heightmap Terrain Settings")]
    public class HeightmapTerrainSettings : ScriptableObject, IDisposable
    {
        /// <summary>
        /// The black and white heightmap texture
        /// </summary>
        [SerializeField] private Texture2D heightmap;

        /// <summary>
        /// The generated height data from the the heightmap
        /// </summary>
        [SerializeField] private NativeArray<float> heightmapData;

        /// <inheritdoc cref="ProceduralTerrainSettings.Amplitude"/>
        [SerializeField] private float amplitude;

        /// <inheritdoc cref="ProceduralTerrainSettings.HeightOffset"/>
        [SerializeField] private float heightOffset;

        /// <summary>
        /// How wide the heightmap is (in pixels). 1 pixel = 1 Unity unit
        /// </summary>
        public int Width { get; private set; }

        /// <summary>
        /// How high the heightmap is (in pixels). 1 pixel = 1 Unity unit
        /// </summary>
        public int Height { get; private set; }

        /// <inheritdoc cref="heightmap"/>
        public Texture2D Heightmap
        {
            get => heightmap;
            set => heightmap = value;
        }

        /// <inheritdoc cref="heightmapData"/>
        public NativeArray<float> HeightmapData
        {
            get => heightmapData;
            set => heightmapData = value;
        }

        /// <inheritdoc cref="amplitude"/>
        public float Amplitude
        {
            get => amplitude;
            set => amplitude = value;
        }

        /// <inheritdoc cref="heightOffset"/>
        public float HeightOffset
        {
            get => heightOffset;
            set => heightOffset = value;
        }

        /// <summary>
        /// Converts the parameters to a format the can be used by the Job System
        /// </summary>
        /// <param name="heightmap"><inheritdoc cref="heightmap" path="/summary"/></param>
        /// <param name="amplitude"><inheritdoc cref="amplitude" path="/summary"/></param>
        /// <param name="heightOffset"><inheritdoc cref="heightOffset" path="/summary"/></param>
        public void Initialize(Texture2D heightmap, float amplitude, float heightOffset)
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
        /// Generates <see cref="HeightmapData"/> from <paramref name="heightmap"/>
        /// </summary>
        /// <param name="heightmap"><inheritdoc cref="heightmap" path="/summary"/></param>
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