using System;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Settings
{
    /// <summary>
    /// The procedural terrain generation settings
    /// </summary>
    [Serializable]
    public struct ProceduralTerrainSettings : IEquatable<ProceduralTerrainSettings>
    {
        /// <summary>
        /// The frequency of the noise
        /// </summary>
        [SerializeField] private float noiseFrequency;
        
        /// <summary>
        /// How many octaves the noise will have
        /// </summary>
        [SerializeField] private int noiseOctaveCount;
        
        /// <summary>
        /// The height multiplier
        /// </summary>
        [SerializeField] private float amplitude;
        
        /// <summary>
        /// Moves the height up and down
        /// </summary>
        [SerializeField] private float heightOffset;

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
        /// Constructor
        /// </summary>
        /// <param name="noiseFrequency">The frequency of the noise</param>
        /// <param name="noiseOctaveCount">How many octaves the noise will have</param>
        /// <param name="amplitude">The height multiplier</param>
        /// <param name="heightOffset">Moves the height up and down</param>
        public ProceduralTerrainSettings(float noiseFrequency, int noiseOctaveCount, float amplitude, float heightOffset)
        {
            this.noiseFrequency = noiseFrequency;
            this.noiseOctaveCount = noiseOctaveCount;
            this.amplitude = amplitude;
            this.heightOffset = heightOffset;
        }

        public bool Equals(ProceduralTerrainSettings other)
        {
            if(other == null) { return false; }

            return NoiseFrequency == other.NoiseFrequency &&
                NoiseOctaveCount == other.NoiseOctaveCount &&
                Amplitude == other.Amplitude &&
                HeightOffset == other.HeightOffset;
        }

        public override bool Equals(object obj)
        {
            if(obj == null) { return false; }

            if(obj is ProceduralTerrainSettings other)
            {
                return Equals(other);
            }

            return false;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;

                hash = hash * 23 + noiseFrequency.GetHashCode();
                hash = hash * 23 + noiseOctaveCount.GetHashCode();
                hash = hash * 23 + amplitude.GetHashCode();
                hash = hash * 23 + heightOffset.GetHashCode();

                return hash;
            }
        }

        public static bool operator ==(ProceduralTerrainSettings left, ProceduralTerrainSettings right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ProceduralTerrainSettings left, ProceduralTerrainSettings right)
        {
            return !(left == right);
        }
    }
}