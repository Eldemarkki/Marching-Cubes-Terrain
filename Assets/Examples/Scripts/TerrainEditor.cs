using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace MarchingCubes.Examples
{
    public class TerrainEditor : MonoBehaviour
    {
        [Header("Terrain Modification Settings")]
        [SerializeField] private bool increaseTerrain = true;
        [SerializeField] private float modificationForce = 0.1f;
        [SerializeField] private float modificationRange = 3f;
        [SerializeField] private float maxReachDistance = 100f;
        [SerializeField] private AnimationCurve forceOverDistance = AnimationCurve.Constant(0, 1, 1);

        [Header("Player Settings")]
        [SerializeField] private World world;
        [SerializeField] private Transform playerCamera;

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

            if (Input.GetButton("Fire1"))
            {
                RaycastToTerrain(increaseTerrain);
            }
            else if (Input.GetButton("Fire2"))
            {
                RaycastToTerrain(!increaseTerrain);
            }
        }

        private void RaycastToTerrain(bool addTerrain)
        {
            Vector3 startP = playerCamera.position;
            Vector3 destP = startP + playerCamera.forward;
            Vector3 direction = destP - startP;

            var ray = new Ray(startP, direction);

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