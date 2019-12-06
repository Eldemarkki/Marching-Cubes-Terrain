namespace MarchingCubes.Examples
{
    [System.Serializable]
    public struct TerrainSettings
    {
        public float noiseFrequency;
        public int noiseOctaveCount;
        public float amplitude;
        public float heightOffset;

        public TerrainSettings(float noiseFrequency, int noiseOctaveCount, float amplitude, float heightOffset)
        {
            this.noiseFrequency = noiseFrequency;
            this.noiseOctaveCount = noiseOctaveCount;
            this.amplitude = amplitude;
            this.heightOffset = heightOffset;
        }
    }
}