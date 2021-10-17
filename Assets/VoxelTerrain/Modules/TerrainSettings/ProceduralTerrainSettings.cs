using System;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Settings
{
    [Serializable]
    public struct ProceduralTerrainSettings
    {
        /// <inheritdoc cref="NoiseFrequency"/>
        [SerializeField] private float noiseFrequency;

        /// <inheritdoc cref="NoiseOctaveCount"/>
        [SerializeField] private int noiseOctaveCount;

        /// <inheritdoc cref="Amplitude"/>
        [SerializeField] private float amplitude;

        /// <inheritdoc cref="HeightOffset"/>
        [SerializeField] private float heightOffset;

        /// <inheritdoc cref="NoiseSeed"/>
        [SerializeField] private int noiseSeed;

        /// <summary>
        /// The frequency of the noise
        /// </summary>
        public float NoiseFrequency { get => noiseFrequency; set => noiseFrequency = value; }

        /// <summary>
        /// How many octaves the noise will have
        /// </summary>
        public int NoiseOctaveCount { get => noiseOctaveCount; set => noiseOctaveCount = value; }

        /// <summary>
        /// The height multiplier
        /// </summary>
        public float Amplitude { get => amplitude; set => amplitude = value; }

        /// <summary>
        /// Moves the height up and down
        /// </summary>
        public float HeightOffset { get => heightOffset; set => heightOffset = value; }

        /// <summary>
        /// The seed that the noise function will be initialized with
        /// </summary>
        public int NoiseSeed { get => noiseSeed; set => noiseSeed = value; }

        /// <param name="noiseFrequency"><inheritdoc cref="noiseFrequency" path="/summary"/></param>
        /// <param name="noiseOctaveCount"><inheritdoc cref="noiseOctaveCount" path="/summary"/></param>
        /// <param name="amplitude"><inheritdoc cref="amplitude" path="/summary"/></param>
        /// <param name="heightOffset"><inheritdoc cref="heightOffset" path="/summary"/></param>
        /// <param name="noiseSeed"><inheritdoc cref="noiseSeed" path="/summary"/></param>
        public ProceduralTerrainSettings(float noiseFrequency, int noiseOctaveCount, float amplitude, float heightOffset, int noiseSeed)
        {
            this.noiseFrequency = noiseFrequency;
            this.noiseOctaveCount = noiseOctaveCount;
            this.amplitude = amplitude;
            this.heightOffset = heightOffset;
            this.noiseSeed = noiseSeed;
        }
    }
}