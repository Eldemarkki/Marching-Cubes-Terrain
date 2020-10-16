using Eldemarkki.VoxelTerrain.Meshing.MarchingCubes;
using Eldemarkki.VoxelTerrain.Utilities.Intersection;
using Eldemarkki.VoxelTerrain.World;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Player
{
    /// <summary>
    /// The terrain deformer which modifies the terrain
    /// </summary>
    public class TerrainDeformer : MonoBehaviour
    {
        /// <summary>
        /// The voxel data store that will be deformed
        /// </summary>
        [Header("Terrain Deforming Settings")]
        [SerializeField] private VoxelWorld voxelWorld;

        /// <summary>
        /// Does the left mouse button add or remove terrain
        /// </summary>
        [SerializeField] private bool leftClickAddsTerrain = true;

        /// <summary>
        /// How fast the terrain is deformed
        /// </summary>
        [SerializeField] private float deformSpeed = 0.1f;

        /// <summary>
        /// How far the deformation can reach
        /// </summary>
        [SerializeField] private float deformRange = 3f;

        /// <summary>
        /// How far away points the player can deform
        /// </summary>
        [SerializeField] private float maxReachDistance = Mathf.Infinity;

        /// <summary>
        /// Which key must be held down to flatten the terrain
        /// </summary>
        [Header("Flattening")]
        [SerializeField] private KeyCode flatteningKey = KeyCode.LeftControl;

        /// <summary>
        /// The color that the terrain will be painted with
        /// </summary>
        [Header("Material Painting")]
        [SerializeField] private Color32 paintColor;

        /// <summary>
        /// The game object that the deformation raycast will be cast from
        /// </summary>
        [Header("Player Settings")]
        [SerializeField] private Transform playerCamera;

        /// <summary>
        /// Is the terrain currently being flattened
        /// </summary>
        private bool _isFlattening;

        /// <summary>
        /// The point where the flattening started
        /// </summary>
        private float3 _flatteningOrigin;

        /// <summary>
        /// The normal of the flattening plane
        /// </summary>
        private float3 _flatteningNormal;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            if (Input.GetKey(flatteningKey))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 startP = playerCamera.position;
                    Vector3 destP = startP + playerCamera.forward;
                    Vector3 direction = destP - startP;

                    Ray ray = new Ray(startP, direction);

                    if (!Physics.Raycast(ray, out RaycastHit hit, maxReachDistance)) { return; }
                    _isFlattening = true;

                    _flatteningOrigin = hit.point;
                    _flatteningNormal = hit.normal;
                }
                else if (Input.GetMouseButtonUp(0))
                {
                    _isFlattening = false;
                }
            }

            if (Input.GetKeyUp(flatteningKey))
            {
                _isFlattening = false;
            }

            if (Input.GetMouseButton(0))
            {
                if (_isFlattening)
                {
                    FlattenTerrain();
                }
                else
                {
                    RaycastToTerrain(leftClickAddsTerrain);
                }
            }
            else if (Input.GetMouseButton(1))
            {
                RaycastToTerrain(!leftClickAddsTerrain);
            }
            else if (Input.GetMouseButton(2))
            {
                PaintColor();
            }
        }

        /// <summary>
        /// Shoots a raycast to the terrain and deforms the terrain around the hit point
        /// </summary>
        /// <param name="addTerrain">Should terrain be added or removed</param>
        private void RaycastToTerrain(bool addTerrain)
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);

            if (!Physics.Raycast(ray, out RaycastHit hit, maxReachDistance)) { return; }
            Vector3 hitPoint = hit.point;

            EditTerrain(hitPoint, addTerrain, deformSpeed, deformRange);
        }

        /// <summary>
        /// Deforms the terrain in a spherical region around the point
        /// </summary>
        /// <param name="point">The point to modify the terrain around</param>
        /// <param name="addTerrain">Should terrain be added or removed</param>
        /// <param name="deformSpeed">How fast the terrain should be deformed</param>
        /// <param name="range">How far the deformation can reach</param>
        private void EditTerrain(Vector3 point, bool addTerrain, float deformSpeed, float range)
        {
            int buildModifier = addTerrain ? 1 : -1;

            int hitX = Mathf.RoundToInt(point.x);
            int hitY = Mathf.RoundToInt(point.y);
            int hitZ = Mathf.RoundToInt(point.z);

            int intRange = Mathf.CeilToInt(range);

            for (int x = -intRange; x <= intRange; x++)
            {
                for (int y = -intRange; y <= intRange; y++)
                {
                    for (int z = -intRange; z <= intRange; z++)
                    {
                        int offsetX = hitX - x;
                        int offsetY = hitY - y;
                        int offsetZ = hitZ - z;

                        int3 offsetPoint = new int3(offsetX, offsetY, offsetZ);
                        float distance = math.distance(offsetPoint, point);
                        if (distance > range)
                        {
                            continue;
                        }

                        float modificationAmount = deformSpeed / distance * buildModifier;
                        voxelWorld.VoxelDataStore.IncreaseVoxelData(offsetPoint, -modificationAmount);
                    }
                }
            }
        }

        /// <summary>
        /// Get a point on the flattening plane and flatten the terrain around it
        /// </summary>
        private void FlattenTerrain()
        {
            PlaneLineIntersectionResult result = IntersectionUtilities.PlaneLineIntersection(_flatteningOrigin, _flatteningNormal, playerCamera.position, playerCamera.forward, out float3 intersectionPoint);
            if (result != PlaneLineIntersectionResult.OneHit) { return; }

            float flattenOffset = 0;

            // This is a bit hacky. One fix could be that the VoxelMesher class has a flattenOffset property, but I'm not sure if that's a good idea either.
            if(voxelWorld.VoxelMesher is MarchingCubesMesher marchingCubesMesher)
            {
                flattenOffset = marchingCubesMesher.Isolevel;
            }

            int intRange = (int)math.ceil(deformRange);
            for (int x = -intRange; x <= intRange; x++)
            {
                for (int y = -intRange; y <= intRange; y++)
                {
                    for (int z = -intRange; z <= intRange; z++)
                    {
                        int3 localPosition = new int3(x, y, z);
                        float3 offsetPoint = intersectionPoint + localPosition;

                        float distance = math.distance(offsetPoint, intersectionPoint);
                        if (distance > deformRange)
                        {
                            continue;
                        }

                        int3 voxelDataWorldPosition = (int3)offsetPoint;
                        if (voxelWorld.VoxelDataStore.TryGetVoxelData(voxelDataWorldPosition, out float oldVoxelData))
                        {
                            float voxelDataChange = (math.dot(_flatteningNormal, voxelDataWorldPosition) - math.dot(_flatteningNormal, _flatteningOrigin)) / deformRange;
                            voxelWorld.VoxelDataStore.SetVoxelData((voxelDataChange * 0.5f + oldVoxelData - flattenOffset) * 0.8f + flattenOffset, voxelDataWorldPosition);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Shoots a ray towards the terrain and changes the material around the hitpoint to <see cref="paintColor"/>
        /// </summary>
        private void PaintColor()
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);

            if (!Physics.Raycast(ray, out RaycastHit hit, maxReachDistance)) { return; }
            Vector3 point = hit.point;

            int hitX = Mathf.RoundToInt(point.x);
            int hitY = Mathf.RoundToInt(point.y);
            int hitZ = Mathf.RoundToInt(point.z);

            int intRange = Mathf.CeilToInt(deformRange);

            for (int x = -intRange; x <= intRange; x++)
            {
                for (int y = -intRange; y <= intRange; y++)
                {
                    for (int z = -intRange; z <= intRange; z++)
                    {
                        int offsetX = hitX - x;
                        int offsetY = hitY - y;
                        int offsetZ = hitZ - z;

                        int3 offsetPoint = new int3(offsetX, offsetY, offsetZ);
                        float distance = math.distance(offsetPoint, point);
                        if (distance > deformRange)
                        {
                            continue;
                        }

                        voxelWorld.VoxelColorStore.SetColor(offsetPoint, paintColor);
                    }
                }
            }
        }
    }
}