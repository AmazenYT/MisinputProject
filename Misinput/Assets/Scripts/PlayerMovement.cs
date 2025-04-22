using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;

public class PlayerMovementTutorial : MonoBehaviour
{
    [Header("Camera")]
    public PlayerCam playerCam;

    [Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    bool readyToJump;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("Step Climb")]
    public float stepHeight = 0.3f;
    public float stepCheckDistance = 0.4f;
    public LayerMask stepLayerMask;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    public Transform orientation;

    float horizontalInput;
    float verticalInput;

    Vector3 moveDirection;

    Rigidbody rb;

    public MovementState state;
    public enum MovementState
    {
        walking,
        sprinting,
        crouching,
        air
    }

    // === Ability Locks ===
    private bool canMoveForward = true;
    private bool canMoveBackward = false;
    private bool canMoveLeft = false;
    private bool canMoveRight = false;
    private bool canJump = false;
    private bool canSprint = false;
    private bool canCrouch = false;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        readyToJump = true;

        startYScale = transform.localScale.y;
    }

    private void Update()
    {
        // ground check
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.3f, whatIsGround);

        MyInput();
        SpeedControl();
        StateHandler();

        // handle drag
        if (grounded)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;
    }

    private void StateHandler()
    {

        //Mode - Crouching 
           if (Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed;
        }

        //Mode - Sprinting
        if (grounded && canSprint && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed;
        }

        //Mode - Walking
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed;
        }

        //Mode - Air
        else
        {
            state = MovementState.air;
        }
    }


    private void FixedUpdate()
    {
        MovePlayer();
        StepClimb();

    }

    private void StepClimb()
    {
        Vector3 origin = transform.position;
        Vector3[] directions = new Vector3[]
        {
        orientation.forward,
        Quaternion.AngleAxis(45f, Vector3.up) * orientation.forward,
        Quaternion.AngleAxis(-45f, Vector3.up) * orientation.forward
        };

        foreach (Vector3 dir in directions)
        {
            // Lower ray (foot level)
            bool lowerRayHit = Physics.Raycast(origin + Vector3.up * 0.1f, dir, out RaycastHit lowerHit, stepCheckDistance, stepLayerMask);

            // Upper ray (head level)
            bool upperRayHit = Physics.Raycast(origin + Vector3.up * stepHeight, dir, out RaycastHit upperHit, stepCheckDistance, stepLayerMask);

            if (lowerRayHit && !upperRayHit)
            {
                rb.position += new Vector3(0f, stepHeight, 0f);
                break; // Only step once per frame
            }
        }
    }


    private void MyInput()
    {
        float rawH = Input.GetAxisRaw("Horizontal");
        float rawV = Input.GetAxisRaw("Vertical");

        // restrict input based on unlocked keys
        horizontalInput = 0f;
        verticalInput = 0f;

        if (canMoveForward && rawV > 0)
            verticalInput = rawV;
        if (canMoveBackward && rawV < 0)
            verticalInput = rawV;
        if (canMoveRight && rawH > 0)
            horizontalInput = rawH;
        if (canMoveLeft && rawH < 0)
            horizontalInput = rawH;

        // handle jump input
        if (canJump && Input.GetKey(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }

        // start crouch
        if (Input.GetKeyDown(crouchKey) && canCrouch)
        {
            transform.localScale = new Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);
        }

        // stop crouch
        if (Input.GetKeyUp(crouchKey) && canCrouch)
        {
            transform.localScale = new Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }

    }





    private void MovePlayer()
    {
        // calculate movement direction
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // on ground
        if (grounded)
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);

        // in air
        else
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        // limit velocity if needed
        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }
    }

    private void Jump()
    {
        // reset y velocity
        rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    // === Called by KeyPickup ===
    public void UnlockAbility(string key)
    {
        switch (key)
        {
            case "W":
                canMoveForward = true;
                break;
            case "A":
                canMoveLeft = true;
                break;
            case "S":
                canMoveBackward = true;
                break;
            case "D":
                canMoveRight = true;
                break;
            case "Space":
                canJump = true;
                break;
            case "Shift":
                canSprint = true;
                break;
            case "Control":
                canCrouch = true;
                    break;
            case "Mouse":
                if (playerCam != null)
                    playerCam.SetMouseLookState(true);
                break;
            default:
                Debug.LogWarning("Unknown ability key: " + key);
                break;
        }
        Debug.Log("Unlocked: " + key);
    }

}
