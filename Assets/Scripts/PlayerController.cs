using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    //common variables
    private CharacterController controller;
    private PlayerInput playerInput;

    //WASD movement variables
    private Vector2 movementValue;
    [SerializeField] private float movementSpeed;
    private Inventory inventory; // also used in interaction, and in selection and drop

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
    private readonly float gravity = -9.81f;
    private Vector3 velocity; // downward velocity
    [SerializeField] private float jumpHeight;

    //Interaction variables
    [SerializeField] private Transform crosshairTransform;
    [SerializeField] private GrabButtonScript grabButtonScript;
    private GameObject lookingAt = null;

    //Selection and drop variables
    [SerializeField] private RectTransform boxTransform;
    private bool selectionActive;   //while active, selection box is displayed
    private int selectedMetal;  //0 - platinum, 1 - gold and so on up to 3
    private IEnumerator boxCoroutine;
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject platinumPrefab;
    [SerializeField] private GameObject silverPrefab;
    [SerializeField] private GameObject copperPrefab;


    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        inventory = GetComponent<Inventory>();
        selectionActive = false;
        selectedMetal = 0;
        boxCoroutine = ShowBox(0);
    }

    private void Update()
    {
        float inventoryWeight = inventory.GetInventoryWeight();

        CastInteractionRay();
        HandleGravity(inventoryWeight);
        HandleMovement(inventoryWeight);
        HandleCamera(); //rotation of whole player model is also handled in this function
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (lookingAt!= null && context.started)
        {
            switch (lookingAt.GetComponent<Rigidbody>().mass)
            {
                case 1f:
                    inventory.IncreaseCopper();
                    break;
                case 2f:
                    inventory.IncreaseSilver();
                    break;
                case 3f:
                    inventory.IncreaseGold();
                    break;
                case 4f:
                    inventory.IncreasePlatinum();
                    break;
            }
            Destroy(lookingAt);
            grabButtonScript.SetButton(false);
        }
    }

    public void SwitchSelectionLeft(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            if (selectionActive)
            {
                StopCoroutine(boxCoroutine);
            }

            if (selectedMetal == 0)
            {
                selectedMetal = 3;
                boxCoroutine = ShowBox(selectedMetal);
                StartCoroutine(boxCoroutine);
            }
            else
            {
                selectedMetal -= 1;
                boxCoroutine = ShowBox(selectedMetal);
                StartCoroutine(boxCoroutine);
            }
        }
    }

    public void SwitchSelectionRight(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (selectionActive)
            {
                StopCoroutine(boxCoroutine);
            }

            if (selectedMetal == 3)
            {
                selectedMetal = 0;
                boxCoroutine = ShowBox(selectedMetal);
                StartCoroutine(boxCoroutine);
            }
            else
            {
                selectedMetal += 1;
                boxCoroutine = ShowBox(selectedMetal);
                StartCoroutine(boxCoroutine);
            }
        }
    }

    public void DropMetal(InputAction.CallbackContext context)
    {
        if(context.started)
        {
            RaycastHit hitInfo;
            Vector3 pos;
            Ray ray = cam.ScreenPointToRay(crosshairTransform.position);

            bool somethingHit = Physics.Raycast(ray, out hitInfo, 3.5f);

            if (somethingHit)
            {
                pos = hitInfo.point;
            }
            else
            {
                pos = cam.ScreenToWorldPoint(new Vector3(crosshairTransform.position.x,
                    crosshairTransform.position.y, 4f));
            }

            switch (selectedMetal)
            {
                case 0:
                    if(inventory.DecreasePlatinum())
                    {
                        Instantiate(platinumPrefab, pos, platinumPrefab.transform.rotation);
                    }
                    break;
                case 1:
                    if (inventory.DecreaseGold())
                    {
                        Instantiate(goldPrefab, pos, platinumPrefab.transform.rotation);
                    }
                    break;
                case 2:
                    if (inventory.DecreaseSilver())
                    {
                        Instantiate(silverPrefab, pos, platinumPrefab.transform.rotation);
                    }
                    break;
                case 3:
                    if (inventory.DecreaseCopper())
                    {
                        Instantiate(copperPrefab, pos, platinumPrefab.transform.rotation);
                    }
                    break;
            }
        }
    }

    private void HandleCamera()
    {
        cameraPanValue = playerInput.actions["Look"].ReadValue<Vector2>();

        xRotation += cameraPanValue.y * xSensitivity;
        // clamped so that the player cannot turn
        // and see behind their back
        xRotation = Mathf.Clamp(xRotation, -50f, 60f);

        //rotate camera around x-axis
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        //Rotate whole player model around y-axis
        transform.Rotate(cameraPanValue.x * ySensitivity * Vector3.up);
    }

    private void HandleMovement(float inventoryWeight)
    {
        float appliedSpeed = movementSpeed - inventoryWeight;
        if(appliedSpeed <= 0)
        {
            appliedSpeed = 0.5f;
        }

        movementValue = playerInput.actions["Move"].ReadValue<Vector2>();

        //convert WASD Vector2 from input into Vector3 and move character,
        //also taking inventory weight into account
        Vector3 movementVec3 = transform.right * movementValue.x + transform.forward * movementValue.y;
        controller.Move((appliedSpeed) * Time.deltaTime * movementVec3);
    }

    private void HandleGravity(float inventoryWeight)
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
        velocity.y += (gravity - inventoryWeight) * Time.deltaTime;

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        controller.Move(velocity * Time.deltaTime);
    }

    private void CastInteractionRay()
    {
        RaycastHit hitInfo;
        Ray ray = cam.ScreenPointToRay(crosshairTransform.position);

        bool metalHit = Physics.Raycast(ray, out hitInfo, 4f) 
                        && hitInfo.collider.CompareTag("Metal");

        if (metalHit && lookingAt != hitInfo.collider.gameObject)
        {
            if (lookingAt != null)
            {
                lookingAt.GetComponent<Outline>().enabled = false;
                grabButtonScript.SetButton(false);
            }

            lookingAt = hitInfo.collider.gameObject;
            lookingAt.GetComponent<Outline>().enabled = true;
            grabButtonScript.SetButton(true);
        }
        else if(lookingAt != null && !metalHit)
        {
            lookingAt.GetComponent<Outline>().enabled = false;
            grabButtonScript.SetButton(false);
            lookingAt = null;
        }
    }

    IEnumerator ShowBox(int position)
    {
        Vector3 pos = boxTransform.anchoredPosition;

        switch (position)
        {
            case 0:
                boxTransform.anchoredPosition = new Vector3(22, pos.y, pos.z);
                boxTransform.GetComponent<Image>().enabled = true;
                selectionActive = true;
                yield return new WaitForSeconds(4f);
                break;
            case 1:
                boxTransform.anchoredPosition = new Vector3(67, pos.y, pos.z);
                boxTransform.GetComponent<Image>().enabled = true;
                selectionActive = true;
                yield return new WaitForSeconds(4f);
                break;
            case 2:
                boxTransform.anchoredPosition = new Vector3(112, pos.y, pos.z);
                boxTransform.GetComponent<Image>().enabled = true;
                selectionActive = true;
                yield return new WaitForSeconds(4f);
                break;
            case 3:
                boxTransform.anchoredPosition = new Vector3(157, pos.y, pos.z);
                boxTransform.GetComponent<Image>().enabled = true;
                selectionActive = true;
                yield return new WaitForSeconds(4f);
                break;
        }

        boxTransform.GetComponent<Image>().enabled = false;
        selectionActive = false;
    }

}
