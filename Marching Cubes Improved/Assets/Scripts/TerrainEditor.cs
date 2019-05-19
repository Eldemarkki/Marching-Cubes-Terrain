using UnityEngine;

public class TerrainEditor : MonoBehaviour
{
    [SerializeField] private bool addTerrain = true;
    [SerializeField] private float force = 2f;
    [SerializeField] private float range = 2f;

    [SerializeField] private float maxReachDistance = 100f;

    [SerializeField] private AnimationCurve forceOverDistance = AnimationCurve.Constant(0, 1, 1);

    [SerializeField] private World world;
    [SerializeField] private Transform playerCamera;

    Chunk[] _initChunks;

    private void Start()
    {
        _initChunks = new Chunk[8];
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        TryEditTerrain();
    }

    private void TryEditTerrain()
    {
        if (force <= 0 || range <= 0)
        {
            return;
        }

        if (Input.GetButton("Fire1"))
        {
            RaycastToTerrain(addTerrain);
        }
        else if (Input.GetButton("Fire2"))
        {
            RaycastToTerrain(!addTerrain);
        }
    }

    private void RaycastToTerrain(bool addTerrain)
    {
        Vector3 startP = playerCamera.position;
        Vector3 destP = startP + playerCamera.forward;
        Vector3 direction = destP - startP;

        Ray ray = new Ray(startP, direction);

        if (Physics.Raycast(ray, out RaycastHit hit, maxReachDistance))
        {
            Vector3 hitPoint = hit.point;

            if (addTerrain)
            {
                Collider[] hits = Physics.OverlapSphere(hitPoint, range / 2f * 0.8f);
                for (int i = 0; i < hits.Length; i++)
                {
                    if (hits[i].CompareTag("Player"))
                    {
                        return;
                    }
                }
            }

            EditTerrain(hitPoint, addTerrain, force, range);
        }
    }

    private void EditTerrain(Vector3 point, bool addTerrain, float force, float range)
    {
        int buildModifier = addTerrain ? 1 : -1;

        int hitX = point.x.Round();
        int hitY = point.y.Round();
        int hitZ = point.z.Round();

        int intRange = range.Ceil();

        for (int x = -intRange; x <= intRange; x++)
        {
            for (int y = -intRange; y <= intRange; y++)
            {
                for (int z = -intRange; z <= intRange; z++)
                {
                    int offsetX = hitX - x;
                    int offsetY = hitY - y;
                    int offsetZ = hitZ - z;

                    if (!world.IsPointInsideWorld(offsetX, offsetY, offsetZ))
                        continue;

                    float distance = Utils.Distance(offsetX, offsetY, offsetZ, point);
                    if (!(distance <= range)) continue;
                    
                    float modificationAmount = force / distance * forceOverDistance.Evaluate(1 - distance.Map(0, force, 0, 1)) * buildModifier;

                    float oldDensity = world.GetDensity(offsetX, offsetY, offsetZ);
                    float newDensity = oldDensity - modificationAmount;

                    newDensity = newDensity.Clamp01();

                    world.SetDensity(newDensity, offsetX, offsetY, offsetZ, true, _initChunks);
                }
            }
        }
    }
}

