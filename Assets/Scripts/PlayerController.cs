using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //common variables
    private CharacterController controller;
    private PlayerInput playerInput;

    //WASD movement variables
    private Vector2 movementValue;
    [SerializeField] private float movementSpeed;
    Inventory inventory;

    //Camera movement variables
    [SerializeField] private Camera cam;
    private Vector2 cameraPanValue;
    [SerializeField] private float xSensitivity;
    [SerializeField] private float ySensitivity;
    private float xRotation; // rotation of camera around x-axis

    //Jump and gravity variables
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance;
    [SerializeField] private LayerMask groundMask;
    private bool isGrounded;
    private float gravity = -9.81f;
    private Vector3 velocity; // downward velocity
    [SerializeField] private float jumpHeight;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        inventory = GetComponent<Inventory>();
    }

    private void Update()
    {
        movementValue = playerInput.actions["Move"].ReadValue<Vector2>();
        cameraPanValue = playerInput.actions["Look"].ReadValue<Vector2>();

        xRotation += cameraPanValue.y * xSensitivity;
        // clamped so that the player cannot turn
        // and see behind their back
        xRotation = Mathf.Clamp(xRotation, -50f, 60f); 



        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        float inventoryWeight = inventory.getInventoryWeight();

        //convert WASD Vector2 from input into Vector3 and move character,
        //also taking inventory weight into account
        Vector3 movementVec3 = transform.right * movementValue.x + transform.forward * movementValue.y;
        controller.Move((movementSpeed - inventoryWeight) * Time.deltaTime * movementVec3);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(cameraPanValue.x * ySensitivity * Vector3.up);

        velocity.y += gravity * Time.deltaTime;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

}
