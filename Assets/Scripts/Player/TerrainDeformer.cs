using Eldemarkki.VoxelTerrain.Meshing.MarchingCubes;
using Eldemarkki.VoxelTerrain.Utilities;
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
        [Range(0, 1)]
        [SerializeField] private float deformSpeed = 0.05f;

        /// <summary>
        /// The radius of the deformation sphere
        /// </summary>
        [SerializeField] private float deformRadius = 5f;

        /// <summary>
        /// How far away points the player can deform
        /// </summary>
        [SerializeField] private float maxReachDistance = math.INFINITY;

        [SerializeField] private Transform hitIndicator;

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
        /// Deforms the terrain in a spherical region around the point
        /// </summary>
        /// <param name="point">The point to modify the terrain around</param>
        /// <param name="addTerrain">Should terrain be added or removed</param>
        private void EditTerrain(Vector3 point, bool addTerrain)
        {
            int3 hitPoint = (int3)math.round(point);
            int3 rangeInt3 = new int3(math.ceil(deformRadius));
            BoundsInt worldSpaceQuery = new BoundsInt();
            worldSpaceQuery.SetMinMax((hitPoint - rangeInt3).ToVectorInt(), (hitPoint + rangeInt3).ToVectorInt());

            voxelWorld.VoxelDataStore.SetVoxelDataCustom(worldSpaceQuery, (voxelDataWorldPosition, voxelData) =>
            {
                float distance = math.distance(voxelDataWorldPosition, point);
                if (distance > deformRadius)
                {
                    return voxelData;
                }

                float oldVoxelData = voxelData / (float)byte.MaxValue;
                float sphere = distance / deformRadius;
                float targetDensity = addTerrain ?
                    math.min(sphere, oldVoxelData) :
                    math.max(1 - sphere, oldVoxelData);

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

            int3 hitPoint = (int3)math.round(intersectionPoint);
            int3 rangeInt3 = new int3(math.ceil(deformRadius));
            BoundsInt worldSpaceQuery = new BoundsInt();
            worldSpaceQuery.SetMinMax((hitPoint - rangeInt3).ToVectorInt(), (hitPoint + rangeInt3).ToVectorInt());

            voxelWorld.VoxelDataStore.SetVoxelDataCustom(worldSpaceQuery, (voxelDataWorldPosition, voxelData) =>
            {
                float distanceSq = math.distancesq(voxelDataWorldPosition, intersectionPoint);
                if (distanceSq > deformRadius * deformRadius)
                {
                    return voxelData;
                }

                float voxelDataChange = (math.dot(_flatteningNormal, voxelDataWorldPosition) - math.dot(_flatteningNormal, _flatteningOrigin)) / deformRadius;
                return (byte)(math.saturate((voxelDataChange * 0.5f + voxelData / (float)byte.MaxValue - flattenOffset) * 0.8f + flattenOffset) * byte.MaxValue);
            });
        }

        /// <summary>
        /// Shoots a ray towards the terrain and changes the material around the hitpoint to <see cref="paintColor"/>
        /// </summary>
        private void PaintColor(Vector3 point)
        {
            int3 hitPoint = (int3)math.round(point);
            int3 rangeInt3 = new int3(math.ceil(deformRadius));
            BoundsInt worldSpaceQuery = new BoundsInt();
            worldSpaceQuery.SetMinMax((hitPoint - rangeInt3).ToVectorInt(), (hitPoint + rangeInt3).ToVectorInt());

            voxelWorld.VoxelColorStore.SetVoxelDataCustom(worldSpaceQuery, (voxelDataWorldPosition, voxelData) =>
            {
                float distanceSq = math.distancesq(voxelDataWorldPosition, point);
                bool shouldBePainted = distanceSq <= deformRadius * deformRadius;
                return shouldBePainted ? paintColor : voxelData;
            });
        }
    }
}