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

        private Camera _cam;

        private float _rotationX;
        private float _rotationY;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
        }

        private void Update()
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

            _cam.transform.Translate(movementSpeed * Time.deltaTime * movement.normalized, Space.Self);
        }

        private void LookAround()
        {
            float rotationSpeed = sensitivity * _cam.fieldOfView * 0.015f; // 0.015 is roughly 1/60, and 60 is the default field of view so this scales the sensitivity with different field of views
            _rotationX += Input.GetAxis("Mouse Y") * rotationSpeed;
            _rotationY += Input.GetAxis("Mouse X") * rotationSpeed;

            _rotationX = Mathf.Clamp(_rotationX, -90, 90);

            transform.eulerAngles = new Vector3(-_rotationX, _rotationY, 0);
        }

        private void Zoom()
        {
            _cam.fieldOfView = Mathf.Clamp(_cam.fieldOfView - Input.mouseScrollDelta.y * zoomSpeed, minimumFieldOfView, maximumFieldOfView);
        }
    }
}