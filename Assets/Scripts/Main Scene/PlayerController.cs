using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    //common variables
    [Header("common")]
    [SerializeField] private int health;
    private CharacterController controller;
    private PlayerInput playerInput;

    //WASD movement variables
    [Header("WASD movement")]
    [SerializeField] private float movementSpeed;
    private Vector2 movementValue;
    private Inventory inventory; // also used in interaction, and in selection and drop

    //Camera panning variables
    [Header("Camera panning")]
    [SerializeField] private Camera cam;
    [SerializeField] private float xSensitivity;
    [SerializeField] private float ySensitivity;
    private Vector2 cameraPanValue;
    private float xRotation; // rotation of camera around x-axis

    //Jumping and gravity variables
    [Header("Jumping and gravity")]
    [SerializeField] private Transform groundCheck; //GameObject that checks for ground
    [SerializeField] private float groundDistance;  //with Physics.CheckSphere(), using groundDistance as radius
    [SerializeField] private LayerMask groundMask;
    [SerializeField] private float jumpHeight;
    private bool isGrounded;
    private readonly float gravity = -9.81f;
    private Vector3 velocity; // downward velocity

    //Interaction variables
    [Header("Interaction")]
    [SerializeField] private Transform crosshairTransform;
    [SerializeField] private GrabButtonScript grabButtonScript;
    private GameObject lookingAt = null; //object that the player is currently looking at

    //Selection and dropping variables
    [Header("Selection and dropping")]
    [SerializeField] private RectTransform boxTransform; //Red box that marks the selected metal in inventory
    [SerializeField] private GameObject goldPrefab;
    [SerializeField] private GameObject platinumPrefab;
    [SerializeField] private GameObject silverPrefab;
    [SerializeField] private GameObject copperPrefab;
    private bool selectionActive;   //while active, selection box is displayed
    private int selectedMetal;  //0 - platinum, 1 - gold and so on up to 3
    private IEnumerator boxCoroutine; //Coroutine that disables the selectionBox after set time

    //Shooting variables (turning rates and rotations for recoil)
    [Header("Shooting")]
    [SerializeField] private ParticleSystem muzzleFlash;
    [SerializeField] private ParticleSystem enemyHitEffect;
    [SerializeField] private ParticleSystem otherHitEffect;
    [SerializeField] private GameObject gun;
    [SerializeField] private float gunRange;
    [SerializeField] private float upTurningRate;
    [SerializeField] private float downTurningRate;
    [SerializeField] private Vector3 rotationAfterShot;
    [SerializeField] private AudioSource gunAudioSource;
    private Quaternion gunBaseRotation;
    private Quaternion gunQuaternionRotation;
    private bool recoilFirstPhase;
    private bool recoilSecondPhase;

    //Health, rotating when hit and dying
    [Header("Health, rotating when hit and dying")]
    [SerializeField] private float playerTurningRate;
    [SerializeField] private float rotationAfterHit;
    [SerializeField] private Slider healthSlider;
    private AudioSource playerAudioSource;
    private bool playerHitFirstPhase;
    private bool playerHitSecondPhase;
    private bool playerHitThirdPhase;
    private bool dead; //also used when winUI or deathUI is displayed to disable moving
    private Quaternion baseRotation = new();
    private Vector3 targetRotation;
    private Quaternion quaternionTargetRotation = new();

    //UI
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] GameObject deathUI;
    [SerializeField] GameObject winUI;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        Cursor.lockState = CursorLockMode.Locked;
        inventory = GetComponent<Inventory>();
        playerAudioSource = GetComponent<AudioSource>();
        selectionActive = false;
        selectedMetal = 0;
        boxCoroutine = ShowBox(0);
        gunBaseRotation = gun.transform.localRotation;
        gunQuaternionRotation = Quaternion.Euler(rotationAfterShot);
        recoilFirstPhase = false;
        recoilSecondPhase = false;
        playerHitFirstPhase = false;
        playerHitSecondPhase = false;
        playerHitThirdPhase = false;
        dead = false;
    }

    private void Update()
    {
        if (playerHitFirstPhase || playerHitSecondPhase || playerHitThirdPhase)
        {
            RotateOnHit();
        }

        
        if (recoilFirstPhase)
        {
            RotateGunUp();
            if (gun.transform.localRotation == gunQuaternionRotation)
            {
                recoilFirstPhase = false;
                recoilSecondPhase = true;
            }
        }

        if (recoilSecondPhase)
        {
            RotateGunDown();
            if (gun.transform.localRotation == gunBaseRotation)
            {
                recoilSecondPhase = false;
            }
        }

        CastInteractionRay();
        HandleGravity(0);
        HandleMovement(0);
        HandleCamera(); //rotation of whole player model is also handled in this function
    }

    public void PauseGame(InputAction.CallbackContext context)
    {
        if (context.started)
        {
            if (!dead)
            {
                Time.timeScale = 0;
                dead = true;
                Cursor.lockState = CursorLockMode.None;
                pauseMenu.SetActive(true);
            }
            else if (dead && !winUI.activeInHierarchy && !deathUI.activeInHierarchy)
            {
                Time.timeScale = 1;
                dead = false;
                Cursor.lockState = CursorLockMode.Locked;
                pauseMenu.SetActive(false);
            }
        }
    }

    // additional function for exiting pause with clicking on "resume" button
    // instead of clicking escape
    public void PauseGameOnClick() 
    {
            if (!dead)
            {
                Time.timeScale = 0;
                dead = true;
                Cursor.lockState = CursorLockMode.None;
                pauseMenu.SetActive(true);
            }
            else if (dead && !winUI.activeInHierarchy && !deathUI.activeInHierarchy)
            {
                Time.timeScale = 1;
                dead = false;
                Cursor.lockState = CursorLockMode.Locked;
                pauseMenu.SetActive(false);
            }
    }


    public void Jump(InputAction.CallbackContext context)
    {
        if (context.started && isGrounded && !dead)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    public void Shoot(InputAction.CallbackContext context)
    {
        //left-mouse exits to menu when the player beat the game or died
        if (context.canceled && dead && (winUI.activeInHierarchy || deathUI.activeInHierarchy))
        {
            SceneManager.LoadScene(0);
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.None;
            return;
        }

        //standard functionality
        if (context.started && !recoilFirstPhase && !recoilSecondPhase)
        {
            gunAudioSource.PlayOneShot(gunAudioSource.clip, 1f);
            muzzleFlash.Play();
            recoilFirstPhase = true;

            if (Physics.Raycast(cam.transform.position, cam.transform.forward, out RaycastHit hit,
                gunRange))
            {
                if (hit.collider.CompareTag("Enemy") )
                {
                    //different animations are played depending on whether the enemy was hit
                    //in his front or back
                    float angle = Vector3.Angle(hit.collider.gameObject.transform.forward,
                        hit.normal);

                    if (angle >= 90f)
                    {
                        hit.collider.GetComponentInParent<EnemyController>().Stagger("Back");
                    }
                    else
                    {
                        hit.collider.GetComponentInParent<EnemyController>().Stagger("Front");
                    }

                    Instantiate(enemyHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                }
                else if (hit.collider.CompareTag("Enemy Head"))
                {
                    Instantiate(enemyHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    hit.collider.GetComponentInParent<EnemyController>().Die();
                }
                else
                {
                    Instantiate(otherHitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                }    
            }
        }
    }

    public void Interact(InputAction.CallbackContext context)
    {
        if (lookingAt!= null && context.started && !dead)
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
        if(context.started && !dead)
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
        if (context.started && !dead)
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
        if(context.started && !dead)
        {
            RaycastHit hitInfo;
            Vector3 pos;
            Ray ray = cam.ScreenPointToRay(crosshairTransform.position);

            bool somethingHit = Physics.Raycast(ray, out hitInfo, 4f);

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

    public void ReceiveDamage(int damage)
    {
        health -= damage;
        playerAudioSource.Play();
        if (health <= 0)
        {
            health = 0;
            healthSlider.value = health;
            Die();
        }
        healthSlider.value = health;
        playerHitFirstPhase = true;
    }

    private void RotateOnHit()
    {
        if (playerHitFirstPhase)
        {
            baseRotation = transform.rotation;
            Vector3 rotationVec3 = transform.rotation.eulerAngles;
            targetRotation = new(rotationVec3.x, rotationVec3.y, rotationVec3.z + rotationAfterHit);
            quaternionTargetRotation = Quaternion.Euler(targetRotation);
            playerHitFirstPhase = false;
            playerHitSecondPhase = true;
        }

        if (playerHitSecondPhase)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, quaternionTargetRotation,
                playerTurningRate * Time.deltaTime);

            if (transform.rotation == quaternionTargetRotation)
            {
                playerHitSecondPhase = false;
                playerHitThirdPhase = true;
            }
        }

        if (playerHitThirdPhase)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, baseRotation,
                            playerTurningRate * Time.deltaTime);

            if (transform.rotation == baseRotation)
            {
                playerHitThirdPhase = false;
            }
        }
    }

    private void Die()
    {
        Time.timeScale = 0;
        deathUI.SetActive(true);
        dead = true;
    }

    public void Win()
    {
        Time.timeScale = 0;
        winUI.SetActive(true);
        dead = true;
    }

    private void RotateGunUp()
    {
        gun.transform.localRotation = Quaternion.RotateTowards(gun.transform.localRotation,
                gunQuaternionRotation, upTurningRate * Time.deltaTime);
    }

    private void RotateGunDown()
    {
        gun.transform.localRotation = Quaternion.RotateTowards(gun.transform.localRotation,
                gunBaseRotation, downTurningRate * Time.deltaTime);
    }


    private void HandleCamera()
    {
        if (dead)
        {
            return;
        }

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
        if (dead)
        {
            return;
        }

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
        else if (lookingAt == null)
        {
            grabButtonScript.SetButton(false);
        }
        Debug.Log(metalHit);
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
