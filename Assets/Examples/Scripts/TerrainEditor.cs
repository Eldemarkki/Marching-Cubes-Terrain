using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public class TerrainEditor : MonoBehaviour
    {
        [Header("Terrain Modification Settings")]
        [SerializeField] private bool leftClickAddsTerrain = true;
        [SerializeField] private float modificationForce = 0.1f;
        [SerializeField] private float modificationRange = 3f;
        [SerializeField] private float maxReachDistance = 100f;
        [SerializeField] private AnimationCurve forceOverDistance = AnimationCurve.Constant(0, 1, 1);

        [Header("Flattening")]
        [SerializeField] private float flatteningRadius = 3f;
        [SerializeField] private KeyCode flatteningKey = KeyCode.LeftControl;

        [Header("Player Settings")]
        [SerializeField] private World world;
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
            TryEditTerrain();
        }

        private void TryEditTerrain()
        {
            if (modificationForce <= 0 || modificationRange <= 0)
            {
                return;
            }

            if (Input.GetKey(flatteningKey))
            {
                if (Input.GetMouseButtonDown(0))
                {
                    Vector3 startP = playerCamera.position;
                    Vector3 destP = startP + playerCamera.forward;
                    Vector3 direction = destP - startP;

                    var ray = new Ray(startP, direction);

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
        }

        private void FlattenTerrain()
        {
            var result = PlaneLineIntersection(_flatteningOrigin, _flatteningNormal, playerCamera.position, playerCamera.forward, out float3 intersectionPoint);
            if (result != PlaneLineIntersectionResult.OneHit) return;

            int intRange = (int)math.ceil(flatteningRadius);
            for (int x = -intRange; x <= intRange; x++)
            {
                for (int y = -intRange; y <= intRange; y++)
                {
                    for (int z = -intRange; z <= intRange; z++)
                    {
                        int3 localPosition = new int3(x, y, z);
                        float3 offsetPoint = intersectionPoint + localPosition;

                        float distance = math.distance(offsetPoint, intersectionPoint);
                        if (distance > flatteningRadius)
                        {
                            continue;
                        }

                        int3 densityWorldPosition = (int3)offsetPoint;
                        float density = (math.dot(_flatteningNormal, densityWorldPosition) - math.dot(_flatteningNormal, _flatteningOrigin)) / flatteningRadius;
                        float oldDensity = world.GetDensity(densityWorldPosition);

                        world.SetDensity((density+oldDensity) * 0.8f, densityWorldPosition);
                    }
                }
            }
        }

        private PlaneLineIntersectionResult PlaneLineIntersection(float3 planeOrigin, float3 planeNormal, float3 lineOrigin,
            float3 lineDirection, out float3 intersectionPoint)
        {
            planeNormal = math.normalize(planeNormal);
            lineDirection = math.normalize(lineDirection);

            if (math.dot(planeNormal, lineDirection) == 0)
            {
                intersectionPoint = float3.zero;
                return (planeOrigin - lineOrigin).Equals(float3.zero) ? PlaneLineIntersectionResult.ParallelInsidePlane : PlaneLineIntersectionResult.NoHit;
            }

            var d = math.dot(planeOrigin, -planeNormal);
            var t = -(d + lineOrigin.z * planeNormal.z + lineOrigin.y * planeNormal.y + lineOrigin.x * planeNormal.x) / (lineDirection.z * planeNormal.z + lineDirection.y * planeNormal.y + lineDirection.x * planeNormal.x);
            intersectionPoint = lineOrigin + t * lineDirection;
            return PlaneLineIntersectionResult.OneHit;
        }

        private void RaycastToTerrain(bool addTerrain)
        {
            var ray = new Ray(playerCamera.position, playerCamera.forward);

            if (!Physics.Raycast(ray, out RaycastHit hit, maxReachDistance)) { return; }
            Vector3 hitPoint = hit.point;

            if (addTerrain)
            {
                Collider[] hits = Physics.OverlapSphere(hitPoint, modificationRange / 2f * 0.8f);
                if (hits.Any(h => h.CompareTag("Player"))) { return; }
            }

            EditTerrain(hitPoint, addTerrain, modificationForce, modificationRange);
        }

        private void EditTerrain(Vector3 point, bool addTerrain, float force, float range)
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
                        
                        var offsetPoint = new int3(offsetX, offsetY, offsetZ);
                        float distance = math.distance(offsetPoint, point);
                        if (distance > range)
                        {
                            continue;
                        }

                        float modificationAmount = force / distance * forceOverDistance.Evaluate(1 - distance.Map(0, force, 0, 1)) * buildModifier;

                        float oldDensity = world.GetDensity(offsetPoint);
                        float newDensity = Mathf.Clamp(oldDensity - modificationAmount, -1, 1);

                        world.SetDensity(newDensity, offsetPoint);
                    }
                }
            }
        }
    }
}