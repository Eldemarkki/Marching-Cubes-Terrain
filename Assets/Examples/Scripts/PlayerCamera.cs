using UnityEngine;

namespace MarchingCubes.Examples
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float movementSpeed = 30f;

        [Header("Rotation")]
        [SerializeField] private float sensitivity = 3f;

        [Header("Zoom")]
        [SerializeField] private float minimumFieldOfView = 2;
        [SerializeField] private float maximumFieldOfView = 170;
        [SerializeField] private float zoomSpeed = 3f;

        private Camera cam;

        private float rotationX;
        private float rotationY;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        void Update()
        {
            Move();
            LookAround();
            Zoom();
        }

        private void Move()
        {
            Vector3 movement = Vector3.zero;
            movement.x += Input.GetAxisRaw("Horizontal");
            if (Input.GetKey(KeyCode.Space))
            {
                movement.y++;
            }
            else if (Input.GetKey(KeyCode.LeftShift))
            {
                movement.y--;
            }
            movement.z += Input.GetAxisRaw("Vertical");

            cam.transform.Translate(movement.normalized * movementSpeed * Time.deltaTime, Space.Self);
        }

        private void LookAround()
        {
            float rotationSpeed = sensitivity * cam.fieldOfView * 0.015f; // 0.015 is roughly 1/60, and 60 is the default field of view so this scales the sensitivity with different field of views
            rotationX += Input.GetAxis("Mouse Y") * rotationSpeed;
            rotationY += Input.GetAxis("Mouse X") * rotationSpeed;

            rotationX = Mathf.Clamp(rotationX, -90, 90);

            transform.eulerAngles = new Vector3(-rotationX, rotationY, 0);
        }

        private void Zoom()
        {
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView - Input.mouseScrollDelta.y * zoomSpeed, minimumFieldOfView, maximumFieldOfView);
        }
    }
}