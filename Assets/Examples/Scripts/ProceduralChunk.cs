using Unity.Jobs;
using Unity.Mathematics;

namespace MarchingCubes.Examples
{
    public class ProceduralChunk : Chunk
    {
        public ProceduralWorld World { get; set; }

        public void SetCoordinate(int3 coordinate)
        {
            Coordinate = coordinate;
            transform.position = coordinate.ToVectorInt() * ChunkSize;
            name = $"Chunk_{coordinate.x.ToString()}_{coordinate.y.ToString()}_{coordinate.z.ToString()}";

            StartDensityCalculation();
            StartMeshGeneration();
        }

        public override void StartDensityCalculation()
        {
            MarchingCubesJobHandle.Complete();
            
            int3 worldPosition = Coordinate * ChunkSize;

            var job = new ProceduralTerrainDensityCalculationJob
            {
                Densities = Densities,
                offset = worldPosition,
                chunkSize = ChunkSize + 1, // +1 because ChunkSize is the amount of "voxels", and that +1 is the amount of density points
                proceduralTerrainSettings = World.ProceduralTerrainSettings
            };
            
            DensityJobHandle = job.Schedule(Densities.Length, 256);
        }
    }
}