using UnityEngine;

namespace MarchingCubes.Examples
{
    [RequireComponent(typeof(Camera))]
    public class PlayerCamera : MonoBehaviour
    {
        /// <summary>
        /// The movement speed of the camera
        /// </summary>
        [Header("Movement")]
        [SerializeField] private float movementSpeed = 30f;

        /// <summary>
        /// How fast the camera rotates when the player moves the mouse
        /// </summary>
        [Header("Rotation")]
        [SerializeField] private float sensitivity = 3f;

        /// <summary>
        /// The minimum field of view the player can go to
        /// </summary>
        [Header("Zoom")]
        [SerializeField] private float minimumFieldOfView = 2;

        /// <summary>
        /// The maximum field of view the player can go to
        /// </summary>
        [SerializeField] private float maximumFieldOfView = 170;

        /// <summary>
        /// How fast the player can zoom (change the FOV) their camera
        /// </summary>
        [SerializeField] private float zoomSpeed = 3f;

        /// <summary>
        /// The target camera component cached
        /// </summary>
        private Camera _cam;

        /// <summary>
        /// The camera's current rotation in the x-axis
        /// </summary>
        private float _rotationX;

        /// <summary>
        /// The camera's current rotation in the y-axis
        /// </summary>
        private float _rotationY;

        private void Awake()
        {
            _cam = GetComponent<Camera>();
            _rotationX = -transform.eulerAngles.x;
            _rotationY = transform.eulerAngles.y;
        }

        private void Update()
        {
            Move();
            LookAround();
            Zoom();
        }

        /// <summary>
        /// Gets the user input from the keyboard and uses that to move the camera
        /// </summary>
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

        /// <summary>
        /// Gets the user input from the mouse and uses that to rotate the camera
        /// </summary>
        private void LookAround()
        {
            // Dividing by 60 makes the sensitivity scale based on the FOV because the default FOV is 60.
            float rotationSpeed = sensitivity * _cam.fieldOfView / 60f;
            _rotationX += Input.GetAxis("Mouse Y") * rotationSpeed;
            _rotationY += Input.GetAxis("Mouse X") * rotationSpeed;

            _rotationX = Mathf.Clamp(_rotationX, -90, 90);

            transform.eulerAngles = new Vector3(-_rotationX, _rotationY, 0);
        }

        /// <summary>
        /// Gets the user input from the mouse scroll and uses that to zoom (change the FOV)
        /// </summary>
        private void Zoom()
        {
            _cam.fieldOfView = Mathf.Clamp(_cam.fieldOfView - Input.mouseScrollDelta.y * zoomSpeed, minimumFieldOfView, maximumFieldOfView);
        }
    }
}