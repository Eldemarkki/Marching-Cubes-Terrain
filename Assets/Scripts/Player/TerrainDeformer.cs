using Eldemarkki.VoxelTerrain.Extensions;
using Eldemarkki.VoxelTerrain.Meshing.MarchingCubes;
using Eldemarkki.VoxelTerrain.Utilities.Intersection;
using Eldemarkki.VoxelTerrain.World;
using Unity.Mathematics;
using UnityEngine;

namespace Eldemarkki.VoxelTerrain.Player
{
    public class TerrainDeformer : MonoBehaviour
    {
        [Header("Terrain Deforming Settings")]
        [SerializeField] private VoxelWorld voxelWorld;

        [SerializeField] private bool leftClickAddsTerrain = true;

        [Range(0, 1)]
        [SerializeField] private float deformSpeed = 0.05f;

        [SerializeField] private float deformRadius = 5f;

        [SerializeField] private float maxReachDistance = math.INFINITY;

        [SerializeField] private Transform hitIndicator;

        [Header("Flattening")]
        [SerializeField] private KeyCode flatteningKey = KeyCode.LeftControl;

        [Header("Material Painting")]
        [SerializeField] private Color32 paintColor;

        [Header("Player Settings")]
        [SerializeField] private Transform playerCamera;

        private bool _isFlattening;

        private float3 _flatteningOrigin;
        private float3 _flatteningNormal;

        private void Awake()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void Update()
        {
            Ray ray = new Ray(playerCamera.position, playerCamera.forward);
            if (!Physics.Raycast(ray, out RaycastHit hit, maxReachDistance))
            {
                if (hitIndicator) { hitIndicator.gameObject.SetActive(false); }
                return;
            }

            if (hitIndicator)
            {
                hitIndicator.SetPositionAndRotation(hit.point, Quaternion.FromToRotation(hitIndicator.up, hit.normal) * hitIndicator.rotation);
                hitIndicator.gameObject.SetActive(true);
            }

            if (Input.GetKey(flatteningKey))
            {
                if (Input.GetMouseButtonDown(0))
                {
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
                    EditTerrain(hit.point, leftClickAddsTerrain);
                }
            }
            else if (Input.GetMouseButton(1))
            {
                EditTerrain(hit.point, !leftClickAddsTerrain);
            }
            else if (Input.GetMouseButton(2))
            {
                PaintColor(hit.point);
            }
        }

        /// <summary>
        /// Deforms the terrain in a spherical region around <paramref name="point"/>
        /// </summary>
        /// <param name="point">The point to modify the terrain around</param>
        /// <param name="addTerrain">Should terrain be added or removed</param>
        private void EditTerrain(Vector3 point, bool addTerrain)
        {
            voxelWorld.VoxelDataStore.SetVoxelDataInSphere(point, deformRadius, (voxelDataWorldPosition, distance, voxelData) =>
            {
                float oldVoxelData = voxelData / (float)byte.MaxValue;
                float sphere = distance / deformRadius;
                float targetDensity = math.select(
                    math.min(sphere, oldVoxelData), 
                    math.max(1 - sphere, oldVoxelData), 
                    addTerrain);
                return (byte)(math.lerp(oldVoxelData, targetDensity, deformSpeed) * byte.MaxValue);
            });
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
            if (voxelWorld.VoxelMesher is MarchingCubesMesher marchingCubesMesher)
            {
                flattenOffset = marchingCubesMesher.Isolevel;
            }

            voxelWorld.VoxelDataStore.SetVoxelDataInSphere(intersectionPoint, deformRadius, (voxelDataWorldPosition, distance, voxelData) =>
            {
                float voxelDataChange = (math.dot(_flatteningNormal, voxelDataWorldPosition) - math.dot(_flatteningNormal, _flatteningOrigin)) / deformRadius;
                return (byte)(math.saturate((voxelDataChange * 0.5f + voxelData / (float)byte.MaxValue - flattenOffset) * 0.8f + flattenOffset) * byte.MaxValue);
            }, false);
        }

        private void PaintColor(Vector3 point)
        {
            voxelWorld.VoxelColorStore.SetVoxelDataInSphere(point, deformRadius, (voxelDataWorldPosition, distance, voxelData) => paintColor, false);
        }
    }
}