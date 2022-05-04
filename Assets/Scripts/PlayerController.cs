using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    private CharacterController controller;
    private PlayerInput playerInput;
    private Vector2 movementValue;

    [SerializeField] private Camera cam;
    private Vector2 cameraPanValue;
    [SerializeField] private float xSensitivity;
    [SerializeField] private float ySensitivity;
    private float xRotation;

    private float gravity = -9.81f;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        movementValue = playerInput.actions["Move"].ReadValue<Vector2>();
        cameraPanValue = playerInput.actions["Look"].ReadValue<Vector2>();

        xRotation += cameraPanValue.y * xSensitivity;
        xRotation = Mathf.Clamp(xRotation, -50f, 60f);
    }

    private void FixedUpdate()
    {
        Vector3 movementVec3 = transform.right * movementValue.x + transform.forward * movementValue.y;
        controller.Move(movementVec3 * movementSpeed);



        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * cameraPanValue.x * ySensitivity);
    }
}
