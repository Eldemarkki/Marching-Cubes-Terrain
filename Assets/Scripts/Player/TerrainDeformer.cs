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
        [SerializeField] private float deformSpeed = 0.1f;

        /// <summary>
        /// How far the deformation can reach
        /// </summary>
        [SerializeField] private float deformRange = 3f;

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
                    EditTerrain(hit.point, leftClickAddsTerrain, deformSpeed, deformRange);
                }
            }
            else if (Input.GetMouseButton(1))
            {
                EditTerrain(hit.point, !leftClickAddsTerrain, deformSpeed, deformRange);
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
        /// <param name="deformSpeed">How fast the terrain should be deformed</param>
        /// <param name="range">How far the deformation can reach</param>
        private void EditTerrain(Vector3 point, bool addTerrain, float deformSpeed, float range)
        {
            int buildModifier = addTerrain ? 1 : -1;

            int3 hitPoint = (int3)math.round(point);
            int3 rangeInt3 = new int3(math.ceil(range));
            BoundsInt queryBounds = new BoundsInt((hitPoint - rangeInt3).ToVectorInt(), (rangeInt3 * 2).ToVectorInt());

            voxelWorld.VoxelDataStore.SetVoxelDataCustom(queryBounds, (voxelDataWorldPosition, voxelData) =>
            {
                float distanceSq = math.distancesq(voxelDataWorldPosition, point);
                if (distanceSq > range * range)
                {
                    return voxelData;
                }

                float distanceReciprocal = math.rsqrt(distanceSq);
                float modificationAmount = deformSpeed * distanceReciprocal * buildModifier;
                float oldVoxelData = voxelData / (float)byte.MaxValue;

                return (byte)(math.saturate(oldVoxelData - modificationAmount) * byte.MaxValue);
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

            int3 queryPosition = (int3)(intersectionPoint - math.ceil(deformRange));
            BoundsInt worldSpaceQuery = new BoundsInt(queryPosition.ToVectorInt(), (2 * new int3(math.ceil(deformRange)) + 1).ToVectorInt());

            voxelWorld.VoxelDataStore.SetVoxelDataCustom(worldSpaceQuery, (voxelDataWorldPosition, voxelData) =>
            {
                float distanceSq = math.distancesq(voxelDataWorldPosition, intersectionPoint);
                if (distanceSq > deformRange * deformRange)
                {
                    return voxelData;
                }

                float voxelDataChange = (math.dot(_flatteningNormal, voxelDataWorldPosition) - math.dot(_flatteningNormal, _flatteningOrigin)) / deformRange;
                return (byte)(math.saturate((voxelDataChange * 0.5f + voxelData / (float)byte.MaxValue - flattenOffset) * 0.8f + flattenOffset) * byte.MaxValue);
            });
        }

        /// <summary>
        /// Shoots a ray towards the terrain and changes the material around the hitpoint to <see cref="paintColor"/>
        /// </summary>
        private void PaintColor(Vector3 point)
        {
            int3 hitPoint = (int3)math.round(point);
            int3 intRange = new int3(math.ceil(deformRange));

            BoundsInt queryBounds = new BoundsInt((hitPoint - intRange).ToVectorInt(), (intRange * 2).ToVectorInt());

            voxelWorld.VoxelColorStore.SetVoxelDataCustom(queryBounds, (voxelDataWorldPosition, voxelData) =>
            {
                float distanceSq = math.distancesq(voxelDataWorldPosition, point);
                bool shouldBePainted = distanceSq <= deformRange * deformRange;
                return shouldBePainted ? paintColor : voxelData;
            });
        }
    }
}