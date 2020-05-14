using Unity.Jobs;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    /// <summary>
    /// A chunk that is generated procedurally
    /// </summary>
    public class ProceduralChunk : Chunk
    {
        /// <summary>
        /// The procedural terrain generation settings
        /// </summary>
        public ProceduralTerrainSettings TerrainGenerationSettings { get; set; }

        /// <summary>
        /// Completes the mesh generation (if that is still running) and starts calculating the densities
        /// </summary>
        public override void StartDensityCalculation()
        {
            MarchingCubesJobHandle.Complete();
            
            int3 worldPosition = Coordinate * ChunkSize;

            var job = new ProceduralTerrainDensityCalculationJob
            {
                DensityStorage = DensityStorage,
                offset = worldPosition,
                chunkSize = ChunkSize + 1, // +1 because ChunkSize is the amount of "voxels", and that +1 is the amount of density points
                proceduralTerrainSettings = TerrainGenerationSettings
            };
            
            DensityJobHandle = job.Schedule(DensityStorage.Length, 256);
        }

        /// <summary>
        /// Sets the coordinate of this chunk and starts generating the densities and the mesh.
        /// </summary>
        /// <param name="coordinate">The new coordinate</param>
        public void SetCoordinate(int3 coordinate)
        {
            _meshRenderer.enabled = false;
            Coordinate = coordinate;
            transform.position = coordinate.ToVectorInt() * ChunkSize;
            name = $"Chunk_{coordinate.x.ToString()}_{coordinate.y.ToString()}_{coordinate.z.ToString()}";
            _densityModifications.Clear();

            StartDensityCalculation();
            StartMeshGeneration();
        }
    }
}